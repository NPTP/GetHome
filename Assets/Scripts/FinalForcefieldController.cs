using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalForcefieldController : MonoBehaviour
{
    private int numPuzzles = 4;
    private int numSolved;

    void Start(){
        numSolved = 0;
    }
    public void addCompleted(){
        numSolved++;
        if (numSolved >= numPuzzles){
            Destroy(this.gameObject);
        }
    }
}
