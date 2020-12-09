using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPuzzleController : MonoBehaviour
{
    AudioSource audioSource;

    private int solvedCount = 0;
    public int totalBoxes;

    private bool isSolved = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void addSolved()
    {
        solvedCount++;
        audioSource.Play();
        if (solvedCount >= totalBoxes)
        {
            this.GetComponent<BoxCollider>().enabled = false;
            this.GetComponent<Renderer>().enabled = false;
            isSolved = true;
        }
    }

    public void removeSolved()
    {
        solvedCount--;
    }

    public bool getSolved()
    {
        return isSolved;
    }

}
