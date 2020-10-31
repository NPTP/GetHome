using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipEvents : MonoBehaviour
{
    void HalfwayFlipped()
    {
        GameObject.FindObjectOfType<IsoCulling>().HideCeilingAndSideWalls();
    }

    void FinishedFlipping()
    {
        GameObject.FindObjectOfType<GravityManager>().SetFlipping(false);
    }
}
