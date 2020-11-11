using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// DEPRECATED
public class NewCulling : MonoBehaviour
{
    GravityManager gravityManager;
    GameObject[] levelCeiling;
    GameObject[] levelFloor;
    MouseCam mc;
    public LayerMask transparentMask;
    public LayerMask wallMask;
    public LayerMask gravityFlipMask;

    // Constants for mode in which culling raycast hits are processed.
    const string CULL_ALL = "Cull All Hits";
    const string CULL_CLOSEST = "Cull Closest Hit Only";

    [Tooltip("Angle of the two lateral wall culling raycasts (debug ray is green).")]
    public float wallRayAngle = 30f;

    [Tooltip("Up/down adjustment of the lower transparency raycast (debug ray is purple).")]
    public float lowerRayAdjust = 0.25f;
    Vector3 lowerRayAdjustVector;

    [Tooltip("Up/down adjustment of the upper transparency raycast (debug ray is purple).")]
    public float upperRayAdjust = 0.25f;
    Vector3 upperRayAdjustVector;

    void Start()
    {
        // Get the camera, which points at our selected player.
        mc = GetComponent<MouseCam>();

        // Set up gravity manager, floor/ceiling hide, and start by hiding ceiling immediately.
        gravityManager = GameObject.Find("GravityManager").GetComponent<GravityManager>();
        levelCeiling = GameObject.FindGameObjectsWithTag("LevelCeiling");
        levelFloor = GameObject.FindGameObjectsWithTag("LevelFloor");
        HideCeiling();

        // Set up ray adjustments as vectors for easier use later.
        lowerRayAdjustVector = new Vector3(0f, lowerRayAdjust, 0f);
        upperRayAdjustVector = new Vector3(0f, upperRayAdjust, 0f);
    }

    // DEPRECATED
    // private void Update()
    // {
    //     // Handle closest-wall culling.
    //     ProcessHitsMultipleArrays(RaycastsPlayerToWalls(wallMask), CULL_CLOSEST);

    //     if (gravityManager.isFlipping)
    //     {
    //         // Swap hiding of floor/ceiling depending on current level rotation.
    //         if (gravityManager.degreesRotated >= 90)
    //             HideCeiling();

    //         // Cull or transparent all level geometry in between player & camera during rotation.
    //         ProcessHits(RaycastsPlayerToCamera(gravityFlipMask), CULL_ALL);
    //     }
    //     else
    //     {
    //         // Set objects occluding player in the level to be transparent.
    //         ProcessHits(RaycastsPlayerToCamera(transparentMask), CULL_ALL);
    //     }
    // }


    // Shoot one ray from the player's feet and another from his head, ray origins both adjusted
    // by upper and lower adjustment variables.
    // Dependent on transform pivot point: might need separate case when we have a finished robot!
    // Return the array of hits corresponding to the given layer mask.
    private RaycastHit[] RaycastsPlayerToCamera(LayerMask mask)
    {
        Transform playerTransform = mc.player;
        float playerHeight = (mc.player.tag == "Player" ? mc.player.gameObject.GetComponent<CapsuleCollider>().height :
                                                          /*mc.player.gameObject.GetComponent<BoxCollider>().size.y*/
                                                          mc.player.gameObject.GetComponent<CapsuleCollider>().height);

        // Construct lower ray, shoot ray and collect hits, and draw matching debug ray
        Vector3 lowerPoint = playerTransform.position + lowerRayAdjustVector;
        Vector3 lowerOrigin = transform.position;
        Vector3 lowerDirection = (lowerPoint - transform.position).normalized;
        float lowerMaxDistance = (lowerPoint - transform.position).magnitude;
        RaycastHit[] lowerHits = Physics.RaycastAll(lowerOrigin, lowerDirection, lowerMaxDistance, mask);
        Debug.DrawRay(lowerOrigin, lowerDirection * lowerMaxDistance, Color.magenta);

        // Construct upper ray, shoot ray and collect hits, and draw matching debug ray
        Vector3 upperPoint = playerTransform.position + new Vector3(0f, playerHeight, 0f) + upperRayAdjustVector;
        Vector3 upperOrigin = transform.position;
        Vector3 upperDirection = (upperPoint - transform.position).normalized;
        float upperMaxDistance = (upperPoint - transform.position).magnitude;
        RaycastHit[] upperHits = Physics.RaycastAll(upperOrigin, upperDirection, upperMaxDistance, mask);
        Debug.DrawRay(upperOrigin, upperDirection * upperMaxDistance, Color.magenta);

        // Concatenate the hits of both rays and return the result. Note we may sometimes have redundant hits
        return lowerHits.Concat(upperHits).ToArray();
    }


    // Fires multiple rays and returns an array of hit arrays
    private RaycastHit[][] RaycastsPlayerToWalls(LayerMask mask)
    {
        Transform playerTransform = mc.player;
        Vector3 playerLowPoint = playerTransform.position + lowerRayAdjustVector;
        Vector3 dirToCamera = new Vector3(
            transform.position.x - playerLowPoint.x,
            0f,
            transform.position.z - playerLowPoint.z
        );

        // Raycast towards the wall closest to camera & draw debug ray for scene view.
        RaycastHit[] toCameraFront = Physics.RaycastAll(playerLowPoint, dirToCamera, Mathf.Infinity, mask);
        Debug.DrawRay(playerLowPoint, dirToCamera * 100f, Color.green);

        // Raycast towards the wall to the left of the camera & draw debug ray for scene view.
        RaycastHit[] toCameraLeft = Physics.RaycastAll(playerLowPoint, Quaternion.AngleAxis(wallRayAngle, Vector3.up) * dirToCamera, Mathf.Infinity, mask);
        Debug.DrawRay(playerLowPoint, Quaternion.AngleAxis(wallRayAngle, Vector3.up) * dirToCamera * 100f, Color.green);

        // Raycast towards the wall to the left of the camera & draw debug ray for scene view.
        RaycastHit[] toCameraRight = Physics.RaycastAll(playerLowPoint, Quaternion.AngleAxis(-wallRayAngle, Vector3.up) * dirToCamera, Mathf.Infinity, mask);
        Debug.DrawRay(playerLowPoint, Quaternion.AngleAxis(-wallRayAngle, Vector3.up) * dirToCamera * 100f, Color.green);

        // Put all hit arrays into an array and return the result. Note we may sometimes have redundant hits betweent the hit arrays.
        RaycastHit[][] hitArrays = { toCameraFront, toCameraLeft, toCameraRight };
        return hitArrays;
    }


    private void ProcessHits(RaycastHit[] hits, string mode)
    {
        // Bail if no hits.
        if (hits.Length == 0) return;

        if (mode == CULL_ALL)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                CullObject(hits[i].collider.gameObject);
            }
        }
        else if (mode == CULL_CLOSEST)
        {
            // Find closest hit. Prevents e.g. culling passing through walls to other rooms.
            int closestIndex = 0;
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].distance < minDistance)
                {
                    minDistance = hits[i].distance;
                    closestIndex = i;
                }
            }
            CullObject(hits[closestIndex].collider.gameObject);
        }
    }


    // Cull an object group tile or individual tile
    private void CullObject(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        if (parent.gameObject.layer == obj.layer)
        {
            // Group of multiple tiles to cull at once
            foreach (Transform child in parent)
                child.gameObject.GetComponent<CullTile>().CullThisFrame();
        }
        else
        {
            // Single tile to cull alone
            obj.GetComponent<CullTile>().CullThisFrame();
        }
    }


    private void ProcessHitsMultipleArrays(RaycastHit[][] hitArrays, string mode)
    {
        foreach (RaycastHit[] hits in hitArrays)
        {
            ProcessHits(hits, mode);
        }
    }


    // Hide the entire floor or ceiling at once by manually getting the appropriately tagged tiles.
    private void HideCeiling()
    {
        // DEPRECATED
        // GameObject[] toShow = (gravityManager.isGravityFlipped ? levelCeiling : levelFloor);
        // GameObject[] toHide = (gravityManager.isGravityFlipped ? levelFloor : levelCeiling);

        // foreach (GameObject hide in toHide)
        //     hide.GetComponent<Renderer>().enabled = false;
        // foreach (GameObject show in toShow)
        //     show.GetComponent<Renderer>().enabled = true;
    }

}
