using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookUpFadeEvents : MonoBehaviour
{
    void LookUpEvent()
    {
        GameObject.FindObjectOfType<GravityManager>().LookUp(true);
        GameObject.FindObjectOfType<IsoCulling>().HideCeiling();
    }

    void StopLookingEvent()
    {
        GameObject.FindObjectOfType<GravityManager>().LookUp(false);
        GameObject.FindObjectOfType<IsoCulling>().HideCeiling();
    }
}
