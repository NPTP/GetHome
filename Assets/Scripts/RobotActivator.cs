using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotActivator : MonoBehaviour, IObjectAction
{
    RobotBuddy robo;
    ThirdPersonUserControl tpu;
    GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        robo = GameObject.FindGameObjectWithTag("robot").GetComponent<RobotBuddy>();
        tpu = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonUserControl>();
        parent = GameObject.Find("robot trigger");
    }
    public void action()
    {
        GameObject.Find("RobotTutorial").GetComponent<TutorialScreen>().StartTutorial();
        robo.used = false;
        tpu.canSelectBot = true;
    }
}
