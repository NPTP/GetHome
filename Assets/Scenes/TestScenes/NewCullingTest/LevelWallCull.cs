using System.Collections;
using UnityEngine;

public class LevelWallCull : MonoBehaviour
{
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
    }

    public void CullOneFrame()
    {
        StartCoroutine(DisableRendererThisFrame());
    }

    IEnumerator DisableRendererThisFrame()
    {
        r.enabled = false;
        yield return new WaitForEndOfFrame(); // Waits one frame
        r.enabled = true;
    }
}
