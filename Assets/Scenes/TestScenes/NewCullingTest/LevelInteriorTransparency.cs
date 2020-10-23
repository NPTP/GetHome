using System.Collections;
using UnityEngine;

public class LevelInteriorTransparency : MonoBehaviour
{
    bool culling = false;
    Material originalMat;
    Material transparentMat;
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
        originalMat = r.material;
        transparentMat = Resources.Load<Material>("TransparentMat");
    }
    
    public void CullOneFrame()
    {
        StartCoroutine(ChangeMaterialThisFrame());
    }

    IEnumerator ChangeMaterialThisFrame()
    {
        r.material = transparentMat;
        yield return new WaitForEndOfFrame(); // Waits one frame
        r.material = originalMat;
    }
}
