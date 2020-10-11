using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCam : MonoBehaviour
{
    public float turnSpeed = 4.0f;
    public Transform player;

    public float height;
    public float distance;

    private Vector3 offsetX;

    void Start()
    {

        offsetX = new Vector3(0, height, distance);
    }

    void LateUpdate()
    {
        // TODO: Allow zooming in and out on character here using Mouse Y
        offsetX = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * turnSpeed, Vector3.up) * offsetX;
        transform.position = player.position + offsetX;
        transform.LookAt(player.position);
    }
}
