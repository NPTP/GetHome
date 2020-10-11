using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRotation : MonoBehaviour
{
    // TODO: This still messes with X and Z coordinates! 
    // maybe it'll be easier to just flip everythings y scale or something?
    public GravityManager gravityManager;
    public GameObject target;
    public int degreesPerStep = 2; // Must be a strict integer multiple of 180.
    public float stepTime = .000001f;
    void Start()
    {
        if (180 % degreesPerStep != 0)
            Debug.Log("WARNING! Level rotation degreesPerStep is not a multiple of 180. Level rotation could break.");
    }

    void Update()
    {
        if (gravityManager.gravityFlip)
        {
            StartCoroutine("RotateLevel");
            // transform.localScale.Scale(new Vector3(0, -1, 0));
            gravityManager.gravityFlip = false;

        }
    }

    IEnumerator RotateLevel()
    {
        Vector3 targetPosFixed = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
        Vector3 currentRotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        float currentZ = transform.rotation.eulerAngles.z;

        if (transform.rotation.eulerAngles.z < 180.0f)
        {
            while (transform.rotation.eulerAngles.z < 180.0f)
            {
                transform.RotateAround(targetPosFixed, Vector3.forward, (float)degreesPerStep);
                yield return new WaitForSeconds(stepTime);
            }
            // transform.rotation.eulerAngles.Set(0.0f, 0.0f, 180.0f); // Safety calibration
        }
        else
        {
            float zDifference = currentZ - 180.0f;
            while (transform.rotation.eulerAngles.z > zDifference)// (transform.rotation.eulerAngles.z > 0.0f)
            {
                transform.RotateAround(targetPosFixed, Vector3.forward, -(float)degreesPerStep);
                yield return new WaitForSeconds(stepTime);
            }
            // transform.rotation.eulerAngles.Set(0.0f, 0.0f, 0.0f); // Safety calibration
        }
    }

}
