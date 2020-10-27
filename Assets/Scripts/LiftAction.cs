using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftAction : MonoBehaviour, IObjectAction
{
    bool lifted = false;
    public void action(){

        if(lifted){
            transform.position+= new Vector3(0,-1,0);
            lifted = false;
            
        }else{
            transform.position+= new Vector3(0,1,0);
            lifted = true;
        }
    }

    void OnCollisionEnter(Collision collision){
        collision.transform.SetParent(transform);
    }

    void OnCollisionExit(Collision collision){
        collision.transform.parent = null;
    }
}
