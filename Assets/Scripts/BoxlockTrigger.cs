using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxlockTrigger : MonoBehaviour
{
    public Collider ConnectedBox;

    public WallPuzzleController Connectedwall;

    void OnTriggerEnter(Collider box){
        if(!Connectedwall.getSolved()){
            Connectedwall.addSolved();
        }
    }

    void OnTriggerExit(Collider box){
        if(!Connectedwall.getSolved()){
            Connectedwall.removeSolved();
        }
    }
}
