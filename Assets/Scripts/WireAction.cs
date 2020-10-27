using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireAction : MonoBehaviour, IObjectAction
{
    public void action(){
        Destroy(this.gameObject);
    }
}
