using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof (ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    //private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

    private GameObject selected;
    public GameObject firstbot;

    public MouseCam mc;
    public float roboSpeed = 2.0f;

    private bool playerSelected;


    private void Start()
    {
        // Our player starts selected (Instead of robot)
        selected = this.gameObject;
        playerSelected = true;
        // get the transform of the main camera
        m_Cam = Camera.main.transform;
        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPersonCharacter>();
    }


    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // Here we change to our robot!
            // If we're only doing one robot then there is an easier way to do this
            // but for now we'll keep it scalable!
            if (playerSelected)
            {
                selected = firstbot;
                playerSelected = false;
            }
            else
            {
                // This is here for if we have more than one robot buddy
                // If we stick with one robot buddy, we can clean this script up
                selected = selected.GetComponent<RobotBuddy>().getSibling();
                // make sure we remove any velocity from the player so they stop moving
                // m_Character.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);   // doesn't work
                m_Character.Move(new Vector3(0, 0, 0));
            }

            if (selected == null)
            {
                // This means we've selected off the end of our chain of robot buddies
                // so select the main player 
                playerSelected = true;
                selected = this.gameObject;
            }
            // Point the mouse camera at whatever game object we're currently selecting
            mc.player = selected.transform;
        }
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        // read inputs
        // Check if we should end the game!
        // this eventually will kick to main menu or something instead
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // TODO: Does this work with controllers? Should be good!
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");

        // calculate move direction to pass to character
        if (m_Cam != null)
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v*m_CamForward + h*m_Cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            m_Move = v*Vector3.forward + h*Vector3.right;
        }
#if !MOBILE_INPUT
		// walk speed multiplier
	    if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

        // pass all parameters to the character control script
        if (playerSelected)
        {
            m_Character.Move(m_Move /*, crouch, m_Jump*/);
        }
        else
        {
            // Ok, we're controlling the robot
            // we control directly here
            if (m_Move.magnitude > 0.1)
            {
                if (selected.gameObject.tag == "robot")
                {
                    selected.GetComponent<RobotBuddy>().breakranks();
                }
            }
            selected.GetComponent<CharacterController>().Move(m_Move.normalized * Time.deltaTime * roboSpeed);     //normalized prevents char moving faster than it should with diagonal input
        }
    }
}
