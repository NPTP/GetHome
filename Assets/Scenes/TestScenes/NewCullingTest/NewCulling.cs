using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCulling : MonoBehaviour
{
    GravityManager gravityManager;
    Transform ceiling;
    Transform floor;
    Transform playerTransform;
    float playerHeight;
    public LayerMask transparentMask;
    public LayerMask wallMask;
    public float playerMidpointAdjustment = -0.5f;
    float wallRayAngle = 30f;

    void Start()
    {
        // Set up gravity manager, floor/ceiling hide, and start by hiding ceiling immediately.
        gravityManager = GameObject.Find("GravityManager").GetComponent<GravityManager>();
        ceiling = GameObject.Find("Ceiling").transform;
        floor = GameObject.Find("Floor").transform;
        HideCeiling();

        // Set up necessary player properties.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        playerHeight = player.GetComponent<CapsuleCollider>().height;
    }

    private void Update()
    {
        // Swap hiding of floor/ceiling depending on current level rotation.
        if (gravityManager.isFlipping)
            HideCeiling();

        // Handle interior geometry (transparent) culling.
        InteriorGeometryTransparent();

        // Handle wall culling.
        WallCulling();
    }

    /* Send a ray to the midpoint of the player's collider, adjusted up or down
    ** by 'Player Midpoint Adjustment' in the inspector. Return array of hits
    ** corresponding to mask. */
    private void InteriorGeometryTransparent()
    {
        Vector3 playerMidpoint = playerTransform.position + new Vector3(0f, (playerHeight / 2) + playerMidpointAdjustment, 0f);
        Vector3 origin = transform.position;
        Vector3 direction = (playerMidpoint - transform.position).normalized;
        float maxDistance = (playerMidpoint - transform.position).magnitude;

        // Process the array of raycast hits. (Ignore objects in mask)
        ProcessTransparentHits(Physics.RaycastAll(origin, direction, maxDistance, transparentMask));
        Debug.DrawRay(origin, direction * maxDistance, Color.magenta);
    }

    private void WallCulling()
    {
        Vector3 playerMidpoint = playerTransform.position + new Vector3(0f, (playerHeight / 2) + playerMidpointAdjustment, 0f);
        Vector3 dirToCamera = new Vector3(
            transform.position.x - playerMidpoint.x,
            0f,
            transform.position.z - playerMidpoint.z
        );

        // Raycast towards the wall closest to camera.
        ProcessWallHits(Physics.RaycastAll(playerMidpoint, dirToCamera, Mathf.Infinity, wallMask));
        Debug.DrawRay(playerMidpoint, dirToCamera * 100f, Color.green);

        // Raycast towards the wall to the left of the camera.
        ProcessWallHits(Physics.RaycastAll(playerMidpoint, Quaternion.AngleAxis(wallRayAngle, Vector3.up) * dirToCamera, Mathf.Infinity, wallMask));
        Debug.DrawRay(playerMidpoint, Quaternion.AngleAxis(wallRayAngle, Vector3.up) * dirToCamera * 100f, Color.green);

        // Raycast towards the wall to the left of the camera.
        ProcessWallHits(Physics.RaycastAll(playerMidpoint, Quaternion.AngleAxis(-wallRayAngle, Vector3.up) * dirToCamera, Mathf.Infinity, wallMask));
        Debug.DrawRay(playerMidpoint, Quaternion.AngleAxis(-wallRayAngle, Vector3.up) * dirToCamera * 100f, Color.green);
    }

    private void ProcessTransparentHits(RaycastHit[] hits)
    {
        // Bail if no hits.
        if (hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            GameObject hitObject = hits[i].collider.gameObject;
            if (hitObject.tag == "SingleTile")
            {
                hitObject.GetComponent<LevelInteriorTransparency>().intersecting = true;
            }
            else // this tile must be grouped and therefore have a parent GameObject.
            {
                foreach (Transform child in hitObject.transform.parent)
                    child.gameObject.GetComponent<LevelInteriorTransparency>().intersecting = true;
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

        // Cull the wall that we found was closest.
        Transform wallParent = hits[closestIndex].collider.gameObject.transform.parent.gameObject.transform;
        foreach (Transform child in wallParent)
            child.gameObject.GetComponent<LevelWallTransparency>().intersecting = true;
    }

    /* Hide the entire floor or ceiling by getting the floor/ceiling parent and
    ** enabling/disabling the renderer for every child. */
    private void HideCeiling()
    {
        bool isGravityFlipped = gravityManager.isGravityFlipped;

        Transform toShow = (isGravityFlipped ? ceiling : floor);
        Transform toHide = (isGravityFlipped ? floor : ceiling);

        foreach (Transform child in toHide)
            child.GetComponent<Renderer>().enabled = false;
        foreach (Transform child in toShow)
            child.GetComponent<Renderer>().enabled = true;
    }

}
