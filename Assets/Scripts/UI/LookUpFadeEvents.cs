using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookUpFadeEvents : MonoBehaviour
{
    void InstantFlipEvent()
    {
        GameObject.FindObjectOfType<GravityManager>().InstantFlip();
        GameObject.FindObjectOfType<IsoCulling>().HideCeiling();
    }
}
