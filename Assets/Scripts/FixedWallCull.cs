using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedWallCull : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.parent.GetComponent<GameObject>().SetActive(false);
    }

    void OnTriggerEnter(Collider col){

    }

    
}
