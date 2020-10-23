using System.Collections;
using UnityEngine;

public class CullTransparentTile : MonoBehaviour, CullTile
{
    public MaterialsContainer materialsContainer;
    Material originalMat;
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
        originalMat = r.material;
    }

    public void CullThisFrame()
    {
        StartCoroutine(ChangeMaterialThisFrame());
    }

    IEnumerator ChangeMaterialThisFrame()
    {
        r.material = materialsContainer.transparentMat; // 1. Turn transparent
        yield return new WaitForEndOfFrame();           // 2. Wait for frame to render
        r.material = originalMat;                       // 3. Get original material ready again
    }
}
