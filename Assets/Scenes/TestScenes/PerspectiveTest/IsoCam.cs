using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoCam : MonoBehaviour
{
    public Transform player;

    public float x;
    public float y;
    public float z;
    private Vector3 offsetX;

    void Start()
    {
        offsetX = new Vector3(x, y, z);
    }

    void LateUpdate()
    {
        transform.position = player.position + offsetX;
        transform.LookAt(player.position);
    }
}
