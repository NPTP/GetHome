using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotActivationAction : MonoBehaviour, IObjectAction
{
    // Start is called before the first frame update
    public ThirdPersonUserControl player;
    public void action(){
        player.canSelectBot = true;
    }
}
