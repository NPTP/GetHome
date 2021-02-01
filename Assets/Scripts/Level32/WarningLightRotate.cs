using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningLightRotate : MonoBehaviour
{
    float rotateSpeed = 270f;

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);
    }
}
