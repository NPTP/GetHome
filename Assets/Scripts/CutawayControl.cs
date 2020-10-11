using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class CutawayControl : MonoBehaviour
{
    //The player to shoot the ray at
    public Transform player;
    //The camera to shoot the ray from
    public Transform camera;

    public float offset;

    //List of all objects that we have hidden.
    public List<Transform> hiddenObjects;

    //Layers to hide
    public LayerMask layerMask;

    public float radius;

    public GameObject gravManager;

    private GravityManager gravityManagerScript;

    public GameObject floor;
    public GameObject ceiling;

    public float maxAngle;


    private void Start()
    {
        //Initialize the list
        hiddenObjects = new List<Transform>();
        gravityManagerScript = gravManager.GetComponent<GravityManager>();
    }

    bool inCone(Vector3 intersection, Vector3 direc){

        float cosAngle = Vector3.Dot((camera.position - intersection).normalized, direc.normalized);
        // Expensive!
        float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;
        return angle < maxAngle;
    }

    /* New one maybe
     bool inCone(Vector3 intersection, Vector3 direc){
        float angle = Vector3.Angle((Player.position - intersection), direc);
        return angle < maxAngle;
    }
    */
    void Update()
    {
        // TOOO: We can make this  coroutine since we probably don't need to do all this math EVERY frame, we
        // can simple do it a few times a second?

        //Find the direction from the camera to the player
        Vector3 direc = player.position - camera.position;

        //Raycast and store all hit objects in an array. Also include the layermaks so we only hit the layers we have specified
        List<RaycastHit> allHits = new List<RaycastHit>();
        int numhits = 0;

        RaycastHit[] hits = Physics.SphereCastAll(camera.position, radius, direc, direc.magnitude - offset, layerMask);
        allHits.AddRange(hits);
        numhits += hits.Length;

        //Go through the objects
        for (int i = 0; i < hits.Length; i++)
        {
            Transform currentHit = hits[i].transform;

            //Only do something if the object is not already in the list
            if (!hiddenObjects.Contains(currentHit))
            {   
                if(inCone(currentHit.position, camera.position - player.position)){
                    //Add to list and disable renderer
                    hiddenObjects.Add(currentHit);
                    if (currentHit.GetComponent<Renderer>())
                        currentHit.GetComponent<Renderer>().enabled = false;
                }
            }
        }
        //}

        //clean the list of objects that are in the list but not currently hit.
        for (int i = 0; i < hiddenObjects.Count; i++)
        {
            bool isHit = false;
            //Check every object in the list against every hit
            for (int j =0; j < numhits; j++)
            {
                if (allHits[j].transform == hiddenObjects[i])
                {
                    isHit = true;
                    break;
                }
            }

            //If it is not among the hits
            if (!isHit)
            {
                //Enable renderer, remove from list, and decrement the counter because the list is one smaller now
                Transform wasHidden = hiddenObjects[i];
                if (wasHidden.GetComponent<Renderer>())
                    wasHidden.GetComponent<Renderer>().enabled = true;
                hiddenObjects.RemoveAt(i);
                i--;
            }
        }
    }
}