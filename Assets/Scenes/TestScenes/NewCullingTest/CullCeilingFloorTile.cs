using System.Collections;
using UnityEngine;

// Currently this has the same behaviour as CullWallTile but good to have its own
// class for flexibility later.
public class CullCeilingFloorTile : MonoBehaviour, CullTile
{
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
    }

    public void CullThisFrame()
    {
        if (r.enabled) // Check to avoid interference with other ceilingfloor hiding
            StartCoroutine(DisableRendererThisFrame());
    }

    IEnumerator DisableRendererThisFrame()
    {
        r.enabled = false;                    // 1. Hide tile
        yield return new WaitForEndOfFrame(); // 2. Wait for frame to render
        r.enabled = true;                     // 3. Get tile ready to show again
    }
}
