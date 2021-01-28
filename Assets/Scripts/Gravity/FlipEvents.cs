using System;
using UnityEngine;

public class FlipEvents : MonoBehaviour
{
    [HideInInspector] public float fxScale = 0f;

    public event EventHandler<EventArgs> OnHalfwayFlipped;

    void HalfwayFlipped()
    {
        GameObject.FindObjectOfType<IsoCulling>().HideCeilingAndSideWalls();
        OnHalfwayFlipped?.Invoke(this, EventArgs.Empty);
    }

    void FinishedFlipping()
    {
        GameObject.FindObjectOfType<GravityManager>().SetFlipping(false);
    }
}
