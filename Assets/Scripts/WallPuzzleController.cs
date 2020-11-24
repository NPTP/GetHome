using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPuzzleController : MonoBehaviour
{

    private int solvedCount = 0;
    public int totalBoxes; 
    public void addSolved(){
        solvedCount++;
        if(solvedCount >= totalBoxes){
            Destroy(this.gameObject);
        }
    }

    public void removeSolved(){
        solvedCount--;
    }

}
