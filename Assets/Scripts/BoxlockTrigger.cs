using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxlockTrigger : MonoBehaviour
{
    public Collider ConnectedBox;

    public GameObject Connectedwall;

    void OnTriggerEnter(Collider box){
        if (box.Equals(ConnectedBox)){
            Destroy(Connectedwall);
        }
    }
}
