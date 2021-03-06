﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class friggedLiftAction : MonoBehaviour, IObjectAction
{
    StateManager stateManager;
    public bool lifted;
    public Transform ogParent;

    Vector3 originalPosition;

    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        originalPosition = new Vector3(121, -156, 1);
    }

    public void action()
    {

        if (lifted)
        {
            // transform.position+= new Vector3(0,-2,0);
            transform.DOMove(transform.position + new Vector3(0, -3, 0), 1f).SetEase(Ease.OutCubic);
            lifted = false;
        }
        else
        {
            // transform.position += new Vector3(0,2,0);
            transform.DOMove(transform.position + new Vector3(0, 3, 0), 1f).SetEase(Ease.OutCubic);
            lifted = true;
        }
    }

/*
    void OnCollisionEnter(Collision collision)
    {
        collision.transform.SetParent(transform);
    }

    void OnCollisionExit(Collision collision)
    {
        collision.transform.parent = ogParent;
    }
*/
}
