using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Simple culling this time: The walls to the south and east of the camera (-z and +x respectively)
// will always be culled. Reacts to gravity flips to swap culls appropriately.
// However, this is not intelligent: it is driven by tags, which allows us to design the look the way we want it.
public class IsoCulling : MonoBehaviour
{
    GravityManager gravityManager;
    GameObject[] levelCeiling;
    GameObject[] levelFloor;
    GameObject[] eastWalls;
    GameObject[] southWalls;
    GameObject[] westWalls;
    IsoCam ic;
    public LayerMask transparentMask;

    // Constants for mode in which culling raycast hits are processed.
    const int CULL_ALL = 0;
    const int CULL_CLOSEST = 1;

    [Tooltip("Up/down adjustment of the lower transparency raycast (debug ray is purple).")]
    public float lowerRayAdjust = 0.25f;
    Vector3 lowerRayAdjustVector;

    [Tooltip("Up/down adjustment of the upper transparency raycast (debug ray is purple).")]
    public float upperRayAdjust = 0.25f;
    Vector3 upperRayAdjustVector;

    void Start()
    {
        // Get the camera, which points at our selected player.
        ic = GetComponent<IsoCam>();

        // Set up gravity manager, floor/ceiling hide, and start by hiding ceiling immediately.
        gravityManager = GameObject.Find("GravityManager").GetComponent<GravityManager>();
        levelCeiling = GameObject.FindGameObjectsWithTag("LevelCeiling");
        levelFloor = GameObject.FindGameObjectsWithTag("LevelFloor");
        eastWalls = GameObject.FindGameObjectsWithTag("EastWall");
        southWalls = GameObject.FindGameObjectsWithTag("SouthWall");
        westWalls = GameObject.FindGameObjectsWithTag("WestWall");
        HideCeilingAndSideWalls();
        HideSouthWalls();

        // Set up ray adjustments as vectors for easier use later.
        lowerRayAdjustVector = new Vector3(0f, lowerRayAdjust, 0f);
        upperRayAdjustVector = new Vector3(0f, upperRayAdjust, 0f);
    }

    private void Update()
    {
        if (gravityManager.isFlipping && gravityManager.degreesRotated >= 90)
            HideCeilingAndSideWalls();

        // Set objects occluding player in the level to be transparent.
        ProcessHits(RaycastsPlayerToCamera(transparentMask), CULL_ALL);
        // TODO: Add a spherecast for objects close to the player that aren't hit by a ray.
        // Consider replacing the ray with this spherecast.
    }

    private RaycastHit[] RaycastsPlayerToCamera(LayerMask mask)
    {
        Transform playerTransform = ic.player;
        float playerHeight = ic.player.gameObject.GetComponent<CapsuleCollider>().height;

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

    private void ProcessHits(RaycastHit[] hits, int mode)
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

    private void ProcessHitsMultipleArrays(RaycastHit[][] hitArrays, int mode)
    {
        foreach (RaycastHit[] hits in hitArrays)
        {
            ProcessHits(hits, mode);
        }
    }


    // Hide the entire floor/ceiling, and east/west walls, all at once
    private void HideCeilingAndSideWalls()
    {
        GameObject[] toShow;
        GameObject[] toHide;
        // TODO: combine these floor/ceiling + wall arrays in Start instead, later
        if (gravityManager.isGravityFlipped)
        {
            toShow = levelCeiling.Concat(eastWalls).ToArray();
            toHide = levelFloor.Concat(westWalls).ToArray();
        }
        else
        {
            toShow = levelFloor.Concat(westWalls).ToArray();
            toHide = levelCeiling.Concat(eastWalls).ToArray();
        }

        foreach (GameObject hide in toHide)
            hide.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        foreach (GameObject show in toShow)
            show.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
    }

    // Hide south walls, always. They always remain on the bottom end of the camera view.
    // This could be done in the editor if desired, instead of here, but this gives us some
    // flexibility.
    private void HideSouthWalls()
    {
        foreach (GameObject southWall in southWalls)
            southWall.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

}
