using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class InitVHSVideo : MonoBehaviour
{
    VideoPlayer vp;
    // Start is called before the first frame update
    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        vp.targetCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
}
