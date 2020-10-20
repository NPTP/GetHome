using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelWallTransparency : MonoBehaviour
{
    public bool intersecting = false;
    Renderer r;

    void Start()
    {
        r = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (intersecting)
            r.enabled = false;
        else
            r.enabled = true;
        intersecting = false;
    }
}
