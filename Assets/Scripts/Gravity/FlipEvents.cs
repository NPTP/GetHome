using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipEvents : MonoBehaviour
{
    public float fxScale = 0f;

    void HalfwayFlipped()
    {
        GameObject.FindObjectOfType<IsoCulling>().HideCeilingAndSideWalls();
    }

    void FinishedFlipping()
    {
        GameObject.FindObjectOfType<GravityManager>().SetFlipping(false);
    }
}
