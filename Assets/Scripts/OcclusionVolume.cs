using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OcclusionVolume : MonoBehaviour
{
    StateManager stateManager;
    ThirdPersonUserControl thirdPersonUserControl;

    BoxCollider boxCollider;
    private List<Collider> liveColliders = new List<Collider>();
    public List<Collider> GetColliders() { return liveColliders; }
    private Collider[] levelColliders;
    private Collider[] interiorObjects;

    // List of tuples with the light and its saved ORIGINAL intensity.
    private List<Tuple<Light, float>> lightIntensityPairs;

    float lightFadetime = 0.25f;
    bool playerInside = false;
    bool robotInside = false;

    [HideInInspector] public LayerMask levelCollidersMask;   // Not using this right now.
    [Header("Occlusion Options")]
    public bool hideLights = true;
    public bool hideInteriorObjects = true;
    public LayerMask interiorObjectsMask;

    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;
        boxCollider = GetComponent<BoxCollider>();
        lightIntensityPairs = new List<Tuple<Light, float>>();

        // GetLevelColliders();
        GetInteriorObjects();
        GetLights();
        HideRoom();
    }

    // If we switch chars, we only want to show the room containing the selected char.
    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        if (this.boxCollider.bounds.Contains(args.selected.transform.position))
            ShowRoom();
        else
            HideRoom();
    }

    void GetLights()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light l in allLights)
        {
            // Don't capture the robot's spotlight.
            if (this.boxCollider.bounds.Contains(l.transform.position) && l.transform.parent != null && l.transform.parent.gameObject.tag != "robot") 
            {
                lightIntensityPairs.Add(new Tuple<Light, float>(l, l.intensity));
            }
        }
    }

    void GetLevelColliders()
    {
        levelColliders = Physics.OverlapBox(
            gameObject.transform.position,
            transform.localScale / 2,
            Quaternion.identity * Quaternion.Euler(0f, 90f, 0f),
            levelCollidersMask
        );
    }

    void GetInteriorObjects()
    {
        Vector3 size = transform.TransformVector(boxCollider.size / 2);
        size.x = Mathf.Abs(size.x);
        size.y = Mathf.Abs(size.y);
        size.z = Mathf.Abs(size.z);
        interiorObjects = Physics.OverlapBox(
            transform.position, // boxCollider.transform.TransformPoint(boxCollider.center),
            size, // boxCollider.transform.TransformVector(boxCollider.size * 0.5f),
            Quaternion.identity,// * Quaternion.Euler(0f, 90f, 0f),
            interiorObjectsMask
        );
        //foreach (Collider collider in interiorObjects)
        //{
        //    print(collider.name);
        //}
    }

    Tween HideLights()
    {
        Tween tween = null;
        foreach (Tuple<Light, float> t in lightIntensityPairs)
        {
            tween = t.Item1.DOIntensity(0f, lightFadetime);
        }
        return tween;
    }

    void ShowLights()
    {
        foreach (Tuple<Light, float> t in lightIntensityPairs)
        {
            t.Item1.DOIntensity(t.Item2, lightFadetime);
        }
    }

    void HideLevelColliders()
    {
        foreach (Collider collider in levelColliders)
        {
            collider.gameObject.GetComponent<Renderer>().enabled = false;
        }
    }

    void ShowLevelColliders()
    {
        foreach (Collider collider in levelColliders)
        {
            collider.gameObject.GetComponent<Renderer>().enabled = true;
        }
    }

    void HideInteriorObjects()
    {
        foreach (Collider collider in interiorObjects)
        {
            Renderer r = collider.gameObject.GetComponent<Renderer>();
            if (r != null)
                r.enabled = false;
            else
                collider.gameObject.SetActive(false);
        }
    }

    void ShowInteriorObjects()
    {
        foreach (Collider collider in interiorObjects)
        {
            Renderer r = collider.gameObject.GetComponent<Renderer>();
            if (r != null)
                r.enabled = true;
            else
                collider.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.name + " entered " + gameObject.name);

        if (other.tag == "Player")
            playerInside = true;
        else if (other.tag == "robot")
            robotInside = true;

        if (thirdPersonUserControl.GetSelected() == other.gameObject &&
            stateManager.GetState() != StateManager.State.Flipping)
        {
            ShowRoom();
        }
        if (!liveColliders.Contains(other)) { liveColliders.Add(other); }
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log(other.name + " left " + gameObject.name);

        if (other.tag == "Player")
            playerInside = false;
        else if (other.tag == "robot")
            robotInside = false;

        if (!IsSelectedInVolume() && stateManager.GetState() != StateManager.State.Flipping)
        {
            HideRoom();
        }
        liveColliders.Remove(other);
    }

    void ShowRoom()
    {
        StopCoroutine("HideRoomProcess");
        // ShowLevelColliders();
        if (hideInteriorObjects) ShowInteriorObjects();
        if (hideLights) ShowLights();
    }

    void HideRoom()
    {
        StartCoroutine("HideRoomProcess");
    }

    IEnumerator HideRoomProcess()
    {
        if (hideLights)
        {
            Tween lightFade = HideLights();
            if (lightFade != null)
                yield return new WaitWhile(() => lightFade != null & lightFade.IsPlaying());
        }
        // HideLevelColliders();
        if (hideInteriorObjects) HideInteriorObjects();
        yield return null;
    }

    bool IsSelectedInVolume()
    {
        return this.boxCollider.bounds.Contains(thirdPersonUserControl.GetSelected().transform.position);
    }

    void OnDestroy()
    {
        thirdPersonUserControl.OnSwitchChar -= HandleSwitchChar;
    }
}