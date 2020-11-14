using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OcclusionVolume : MonoBehaviour
{
    StateManager stateManager;

    public LayerMask layerMask;
    BoxCollider boxCollider;
    private List<Collider> liveColliders = new List<Collider>();
    public List<Collider> GetColliders() { return liveColliders; }
    private Collider[] levelColliders;

    // List of tuples with the light and its saved ORIGINAL intensity.
    private List<Tuple<Light, float>> lightIntensityPairs;
    float lightFadetime = 0.25f;

    bool playerInside = false;
    bool robotInside = false;

    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        boxCollider = GetComponent<BoxCollider>();
        lightIntensityPairs = new List<Tuple<Light, float>>();

        GetLevelColliders();
        HideLevelColliders();
        GetAllLights();
        HideLights();
    }

    void GetAllLights()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light l in allLights)
        {
            if (this.boxCollider.bounds.Contains(l.transform.position))
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
            layerMask
        );
    }

    void HideLights()
    {
        foreach (Tuple<Light, float> t in lightIntensityPairs)
        {
            t.Item1.DOIntensity(0f, lightFadetime);
        }
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " entered " + gameObject.name);

        if (other.tag == "Player")
            playerInside = true;
        else if (other.tag == "robot")
            robotInside = true;

        if (playerInside || robotInside)
            // ShowLevelColliders();
            ShowLights();

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
            // HideLevelColliders();
            HideLights();

        liveColliders.Remove(other);
    }
}