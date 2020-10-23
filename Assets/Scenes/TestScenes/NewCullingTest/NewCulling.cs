using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewCulling : MonoBehaviour
{
    GravityManager gravityManager;
    Transform ceiling;
    Transform floor;
    MouseCam mc;
    public LayerMask transparentMask;
    public LayerMask wallMask;

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
        ceiling = GameObject.Find("Ceiling").transform;
        floor = GameObject.Find("Floor").transform;
        HideCeiling();

        // Set up ray adjustments as vectors for easier use later.
        lowerRayAdjustVector = new Vector3(0f, lowerRayAdjust, 0f);
        upperRayAdjustVector = new Vector3(0f, upperRayAdjust, 0f);
    }

    private void Update()
    {
        // Swap hiding of floor/ceiling depending on current level rotation.
        if (gravityManager.degreesRotated >= 90)
            HideCeiling();

        // Handle interior geometry transparency.
        OccludingObjectsTransparency();

        // Handle wall culling.
        WallCulling();
    }

    // Shoot one ray from the player's feet and another from his head, ray origins both adjusted
    // by upper and lower adjustment variables. Dependent on player transform pivot: might need to 
    // create a separate case when we have a proper robot model!
    // Return array of hits corresponding to the object transparent mask.
    private void OccludingObjectsTransparency()
    {
        Transform playerTransform = mc.player;
        float playerHeight = (mc.player.tag == "Player" ? mc.player.gameObject.GetComponent<CapsuleCollider>().height :
                                                          mc.player.gameObject.GetComponent<BoxCollider>().size.y);

        // Construct lower ray, shoot ray and collect hits, and draw matching debug ray
        Vector3 lowerPoint = playerTransform.position + lowerRayAdjustVector;
        Vector3 lowerOrigin = transform.position;
        Vector3 lowerDirection = (lowerPoint - transform.position).normalized;
        float lowerMaxDistance = (lowerPoint - transform.position).magnitude;
        RaycastHit[] lowerHits = Physics.RaycastAll(lowerOrigin, lowerDirection, lowerMaxDistance, transparentMask);
        Debug.DrawRay(lowerOrigin, lowerDirection * lowerMaxDistance, Color.magenta);

        // Construct upper ray, shoot ray and collect hits, and draw matching debug ray
        Vector3 upperPoint = playerTransform.position + new Vector3(0f, playerHeight, 0f) + upperRayAdjustVector;
        Vector3 upperOrigin = transform.position;
        Vector3 upperDirection = (upperPoint - transform.position).normalized;
        float upperMaxDistance = (upperPoint - transform.position).magnitude;
        RaycastHit[] upperHits = Physics.RaycastAll(upperOrigin, upperDirection, upperMaxDistance, transparentMask);
        Debug.DrawRay(upperOrigin, upperDirection * upperMaxDistance, Color.magenta);

        // Process the concatenation of both hit arrays
        ProcessTransparentHits(lowerHits.Concat(upperHits).ToArray());
    }

    private void WallCulling()
    {
        Transform playerTransform = mc.player;
        Vector3 playerLowPoint = playerTransform.position + lowerRayAdjustVector;
        Vector3 dirToCamera = new Vector3(
            transform.position.x - playerLowPoint.x,
            0f,
            transform.position.z - playerLowPoint.z
        );

        // Raycast towards the wall closest to camera.
        ProcessWallHits(Physics.RaycastAll(playerLowPoint, dirToCamera, Mathf.Infinity, wallMask));
        Debug.DrawRay(playerLowPoint, dirToCamera * 100f, Color.green);

        // Raycast towards the wall to the left of the camera.
        ProcessWallHits(Physics.RaycastAll(playerLowPoint, Quaternion.AngleAxis(wallRayAngle, Vector3.up) * dirToCamera, Mathf.Infinity, wallMask));
        Debug.DrawRay(playerLowPoint, Quaternion.AngleAxis(wallRayAngle, Vector3.up) * dirToCamera * 100f, Color.green);

        // Raycast towards the wall to the left of the camera.
        ProcessWallHits(Physics.RaycastAll(playerLowPoint, Quaternion.AngleAxis(-wallRayAngle, Vector3.up) * dirToCamera, Mathf.Infinity, wallMask));
        Debug.DrawRay(playerLowPoint, Quaternion.AngleAxis(-wallRayAngle, Vector3.up) * dirToCamera * 100f, Color.green);
    }

    private void ProcessTransparentHits(RaycastHit[] hits)
    {
        // Bail if no hits.
        if (hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            // Make transparent the object that we found was closest.
            // Check if it's a single tile or a group of multiple tiles.
            GameObject hitObject = hits[i].collider.gameObject;
            Transform objectParent = hitObject.transform.parent;
            if (objectParent.gameObject.layer == hitObject.layer)
            {
                // Group of multiple tiles
                foreach (Transform child in objectParent)
                    child.gameObject.GetComponent<LevelInteriorTransparency>().CullOneFrame();
            }
            else
            {
                // Single tile
                hitObject.GetComponent<LevelInteriorTransparency>().CullOneFrame();
            }
        }
    }

    // Cull only the closest wall that was hit by the raycast, ignore the others.
    private void ProcessWallHits(RaycastHit[] hits)
    {
        // Bail if no hits.
        if (hits.Length == 0) return;

        // Hits array may not be correctly ordered by distance, so find the closest hit.
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

        // Now cull the wall that we found was closest.
        // Check if it's a single tile or a wall of multiple tiles.
        // REPEATED CODE: consider having tile cull classes inherit from some kind of interface
        // so we can clean this up.
        GameObject wallTile = hits[closestIndex].collider.gameObject;
        Transform wallParent = wallTile.transform.parent;
        if (wallParent.gameObject.layer == wallTile.layer)
        {
            // Wall of multiple tiles
            foreach (Transform child in wallParent)
                child.gameObject.GetComponent<LevelWallCull>().CullOneFrame();
        }
        else
        {
            // Single tile
            wallTile.GetComponent<LevelWallCull>().CullOneFrame();
        }
    }

    /* Hide the entire floor or ceiling by getting the appropriately tagged tiles. */
    private void HideCeiling()
    {
        GameObject[] toShow = (gravityManager.isGravityFlipped ? GameObject.FindGameObjectsWithTag("LevelCeiling") : GameObject.FindGameObjectsWithTag("LevelFloor"));
        GameObject[] toHide = (gravityManager.isGravityFlipped ? GameObject.FindGameObjectsWithTag("LevelFloor") : GameObject.FindGameObjectsWithTag("LevelCeiling"));

        foreach (GameObject hide in toHide)
            hide.GetComponent<Renderer>().enabled = false;
        foreach (GameObject show in toShow)
            show.GetComponent<Renderer>().enabled = true;
    }

}
