﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollide : MonoBehaviour
{
    public GameObject ControlledObject;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("We have a collision");
        if (collision.gameObject.tag == "robot")
        {
            Debug.Log("We're colliding with a robot!");
            ControlledObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        ControlledObject.SetActive(true);
    }
}
