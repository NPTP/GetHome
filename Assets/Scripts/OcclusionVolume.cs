using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionVolume : MonoBehaviour
{
    public LayerMask layerMask;
    BoxCollider boxCollider;
    private List<Collider> liveColliders = new List<Collider>();
    public List<Collider> GetColliders() { return liveColliders; }
    private Collider[] levelColliders;

    bool playerInside = false;
    bool robotInside = false;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        GetLevelColliders();
        HideLevelColliders(); // Start by hiding by default.
    }

    void GetLevelColliders()
    {
        levelColliders = Physics.OverlapBox(
            gameObject.transform.position,
            transform.localScale / 2,
            Quaternion.identity * Quaternion.Euler(0f, 90f, 0f),
            layerMask
        );
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " entered " + gameObject.name);

        if (other.tag == "Player")
            playerInside = true;
        else if (other.tag == "robot")
            robotInside = true;

        if (playerInside || robotInside)
            ShowLevelColliders();

        if (!liveColliders.Contains(other)) { liveColliders.Add(other); }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name + " left " + gameObject.name);

        if (other.tag == "Player")
            playerInside = false;
        else if (other.tag == "robot")
            robotInside = false;

        if (!playerInside && !robotInside)
            HideLevelColliders();

        liveColliders.Remove(other);
    }
}