using System.Collections;
using UnityEngine;

public class CullWallTile : MonoBehaviour, CullTile
{
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
    }

    public void CullThisFrame()
    {
        StartCoroutine(DisableRendererThisFrame());
    }

    IEnumerator DisableRendererThisFrame()
    {
        r.enabled = false;                    // 1. Hide tile
        yield return new WaitForEndOfFrame(); // 2. Wait for frame to render
        r.enabled = true;                     // 3. Get tile ready to show again
    }
}
