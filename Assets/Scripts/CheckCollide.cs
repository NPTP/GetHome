using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollide : MonoBehaviour
{
    public GameObject ControlledObject;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "robot")
        {
            ControlledObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        ControlledObject.SetActive(true);
    }
}
