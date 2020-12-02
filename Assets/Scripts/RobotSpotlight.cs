using UnityEngine;

public class RobotSpotlight : MonoBehaviour
{
    Light spotlight;
    ThirdPersonUserControl thirdPersonUserControl;
    RobotBuddy robotBuddy;
    bool botSelected = false;

    void Start()
    {
        spotlight = GetComponent<Light>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;
        robotBuddy = FindObjectOfType<RobotBuddy>();
    }

    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        if (args.selected.tag == "robot")
            botSelected = true;
        else
            botSelected = false;
    }

    void Update()
    {
        float inputStrength = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
        if ((!botSelected || inputStrength > 0) && !robotBuddy.spotlightDirection.Equals(Vector3.zero))
        {
            transform.forward = Vector3.Lerp(transform.forward, robotBuddy.spotlightDirection, 20f * Time.deltaTime);
        }
    }
}
