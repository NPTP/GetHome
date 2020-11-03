using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookUpFadeEvents : MonoBehaviour
{
    void LookUpEvent()
    {
        GameObject.FindObjectOfType<GravityManager>().LookUp();
        GameObject.FindObjectOfType<IsoCulling>().HideCeiling();
    }

    void StopLookingEvent()
    {
        GameObject.FindObjectOfType<GravityManager>().StopLooking();
        GameObject.FindObjectOfType<IsoCulling>().HideCeiling();
    }
}
