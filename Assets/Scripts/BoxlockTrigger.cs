using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxlockTrigger : MonoBehaviour
{
    public Collider ConnectedBox;

    public WallPuzzleController Connectedwall;

    void OnTriggerEnter(Collider box){
        Connectedwall.addSolved();
    }

    void OnTriggerExit(Collider box){
        Connectedwall.removeSolved();
    }
}
