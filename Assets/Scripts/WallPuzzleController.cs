using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPuzzleController : MonoBehaviour
{

    private int solvedCount = 0;
    public int totalBoxes; 

    private bool isSolved = false;

    public void addSolved(){
        solvedCount++;
        if(solvedCount >= totalBoxes){
            this.GetComponent<BoxCollider>().enabled =false;
            this.GetComponent<Renderer>().enabled = false;
            isSolved = true;
        }
    }

    public void removeSolved(){
        solvedCount--;
    }

    public bool getSolved(){
        return isSolved;
    }

}
