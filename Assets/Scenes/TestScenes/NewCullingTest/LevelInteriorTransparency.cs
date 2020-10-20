using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInteriorTransparency : MonoBehaviour
{
    public bool intersecting = false;
    Material originalMat;
    Material transparentMat;
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
        originalMat = r.material;
        transparentMat = Resources.Load<Material>("TransparentMat");
    }

    void LateUpdate()
    {
        if (intersecting)
        {
            r.material = transparentMat;
            intersecting = false;
        }
        else
            r.material = originalMat;
    }
}
