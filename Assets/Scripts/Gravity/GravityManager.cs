using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public bool gravityFlip = false;
    public float flipRotationSpeed = 5.0f;
    private Vector3 normalGravity;
    private Vector3 flippedGravity;
    private float normalGravityStrength = -19.8f;
    private float flippedGravityStrength = 19.8f;
    // Start is called before the first frame update
    void Start()
    {
        normalGravity = new Vector3(0, normalGravityStrength, 0);
        flippedGravity = new Vector3(0, flippedGravityStrength, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2"))
        {
            Debug.Log("Flipping gravity!");
            gravityFlip = !gravityFlip;
        }
    }

    // Keeping this function in case it was called somewhere else still?
    public void flipGrav()
    {
        gravityFlip = !gravityFlip;
    }
}
