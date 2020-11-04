using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptAngleController : MonoBehaviour
{

    public Transform robot;
    public Transform camera;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.forward = robot.position-camera.position;
    }
}
