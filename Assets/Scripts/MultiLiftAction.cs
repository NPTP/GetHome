using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiLiftAction : MonoBehaviour, IObjectAction
{
    public static int size;
    
    public LiftAction[] lifts = new LiftAction[size];

    public void action(){
        foreach (LiftAction lift in lifts){
            lift.action();
        }
    }



}
