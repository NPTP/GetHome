﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStarter : MonoBehaviour
{
    public GameObject fireToStart;
    public GameObject fireWall;

    public void OnTriggerEnter(Collider other)
    {
        fireWall.SetActive(true);
        fireToStart.SetActive(true);
        CameraControl.CC.ScreenShake(1f, 3f);
        Destroy(this.gameObject);
    }
}
