using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class InitVhsPause : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<VideoPlayer>().targetCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
}
