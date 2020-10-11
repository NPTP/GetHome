﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RobotBuddy : MonoBehaviour
{
    public GameObject following;
    private CharacterController controller;
    public GameObject sibling;

    public bool used = false;

    public float speed = 14f;

    private int tcycle;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        tcycle = Random.Range(1, 1000);
    }

    // Update is called once per frame
    void Update()
    {
        if (!used)
        {
            // if we've already been used we stay where we are
            Vector3 curpos = transform.position;
            Vector3 target = following.transform.position;
            Vector3 moveamount = (target - curpos).normalized * speed;
            moveamount.x += Mathf.Sin(tcycle) / 5;
            moveamount.z += Mathf.Cos(tcycle) / 5;
            if ((target - curpos).magnitude > 1.5f) // keep away from the player, don't crowd them!
                // we should also make this so that if the player is trying to back into the robot
                // the robot moves away?
            {
                controller.Move(moveamount * Time.deltaTime);
            }
        }
        // gravity always affects us
        controller.Move(Physics.gravity * Time.deltaTime);
    }

    public GameObject getSibling()
    {
        if (sibling)
            return sibling;
        return null;
    }

    public void setFollowing(GameObject tofollow)
    {
        following = tofollow;
    }

    public void breakranks()
    {
        // Debug.Log("Robot buddy broken ranks");
        used = true;
        if (sibling)
        {
            sibling.GetComponent<RobotBuddy>().setFollowing(following);
        }
    }
}