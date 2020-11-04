using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    //private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

    private GameObject selected;            // keep track of whether the player or a bot is selected
    public GameObject firstbot;             // points to the first robot object
    private GameObject p_Obj;               // keep track of the players gameobject

    public float roboSpeed = 3.0f;

    public float resetSceneTimer = 1.5f;

    private bool playerSelected;
    private GravityManager gravityManager;

    public Vector3 playerMoveInWorld;

    private float resetSceneCount;
    public bool HoldingUseButton;           // we'll check here if the player is holding down the use button so any script can just grab from here

    private float PushPullTimer;    // counter to accumulate pushing and pulling in

    public float PushPullThreshold = 1.0f;


    public LayerMask m_LayerMask;

    public float completeMovingTime = 2.0f; // how many seconds does it take to complete the push/pull animation?
    private float movingAnimationCount;
    public bool isInMovingAnimation;               // if we are in the crate moving animation, lock input and just slide char
    private Vector3 playerMoveTarget;       // where are we moving the player TO
    private Vector3 pushedObjectTarget;     // where are we moving the crate TO 
    private Vector3 playerMoveOrig;         // where are we moving the player FROM
    private Vector3 pushedObjectOrig;       // where are we moving the crate FROM

    bool pushForward;                       // players current status is pushing a crate forward
    bool pullBackwards;                     // players current status is pulling a crate towards themselves

    bool dropCrateWhenAnimationDone;
    bool isPaused;

    private void Start()
    {

        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPersonCharacter>();

        resetSceneCount = 0;
        // Our player starts selected (Instead of robot)
        selected = this.gameObject;
        p_Obj = this.gameObject;
        playerSelected = true;
        // get the transform of the main camera
        m_Cam = Camera.main.transform;

        gravityManager = GameObject.Find("GravityManager").GetComponent<GravityManager>();

        // Flags and counters for pushing and pulling crates
        PushPullTimer = 0.0f;
        pushForward = false;
        pullBackwards = false;
        isInMovingAnimation = false;
        movingAnimationCount = 0.0f;
        dropCrateWhenAnimationDone = false;
    }


    private void Update()
    {

        // make sure we always check if we're holding the use button or not
        // since other scripts may depend on this happening?
        if (Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.E))
        {
            // Player has starte holding down the grab button
            HoldingUseButton = true;
        }
        if (Input.GetButtonUp("Fire3") || Input.GetKeyUp(KeyCode.E))
        {
            // Player lets go of the grab button
            HoldingUseButton = false;
        }

        // if we're in a pushing animation, don't deal with input for now
        if (isInMovingAnimation)
        {
            return;
        }



        // Only allow inputs when not gravity-flipping.
        if (gravityManager.readyToFlip)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // Here we change to our robot!
                // If we're only doing one robot then there is an easier way to do this
                // but for now we'll keep it scalable!
                if (playerSelected)
                {
                    selected = firstbot;
                    firstbot.GetComponent<Light>().color = Color.green;
                    playerSelected = false;
                }
                else
                {
                    // This is here for if we have more than one robot buddy
                    // If we stick with one robot buddy, we can clean this script up
                    selected = selected.GetComponent<RobotBuddy>().getSibling();

                    // make sure we remove any velocity from the player so they stop moving
                    // m_Character.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);   // doesn't work
                    // m_Character.Move(new Vector3(0, 0, 0));
                    m_Character.GetComponent<ThirdPersonCharacter>().StopMoving();
                }

                if (selected == null)
                {
                    // This means we've selected off the end of our chain of robot buddies
                    // so select the main player 
                    playerSelected = true;
                    firstbot.GetComponent<Light>().color = Color.red;
                    selected = this.gameObject;
                }
                // Point the mouse camera at whatever game object we're currently selecting
                // and make sure we point the culler at it as well
                // ic.player = selected.transform;
                CameraControl.CC.ChangeTarget(selected.transform, .4f);
            }
        }



        // read inputs
        // Check if we should end the game!
        // this eventually will kick to main menu or something instead
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // check for reset scene by holding down triggers
        // TODO: We use Input sometimes and CrossPlatformInputManager other times? Not ideal :(
        float lTrigger = Input.GetAxis("TriggerL");
        float rTrigger = Input.GetAxis("TriggerR");

        if (lTrigger > 0.8f && rTrigger > 0.8f)
        {
            resetSceneCount += Time.deltaTime;
        }
        else
        {
            resetSceneCount = 0;
        }

        if (resetSceneCount > resetSceneTimer)      // Reset scenes like this for checkpoint system
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        // TODO: Inputs should be taken in Update() only so as to avoid missed inputs on the fixed timestep,
        // setting bools which are then read by FixedUpdate(). Also will make it easier to stop all inputs
        // during a gravity flip by only having to block them in one place (Update). For now, for the
        // prototype, we block inputs in both functions.

        // Only allow inputs when not gravity-flipping.
        if (!gravityManager.readyToFlip)
        {
            // otherwise, freeze character position while flipping.
            m_Character.FreezeRigidbodyXZPosition();
            return;
        }
        // once we're here, we know gravity isn't flipping

        // Unfreeze XZ position once flipping is done. TODO: "flipping" only means during the rotation
        // right now. It will need to include the entire length of falling to the ground.
        // This can probably be handled in the character control, whenever the character is not grounded,
        // x and z should be frozen.
        m_Character.UnfreezeRigidbodyXZPosition();

        if (dropCrateWhenAnimationDone && !isInMovingAnimation)
        {
            dropCrateWhenAnimationDone = false;
            m_Character.isGrabbingSomething = false;
        }

        if (isInMovingAnimation)
        {
            movingAnimationCount += Time.fixedDeltaTime;
            // Right now, this is just snapping the transform to the next square, so let's change that a bit!
            if (pushForward)
            {
                // move crate first, then player
                m_Character.grabbedBox.transform.position = Vector3.Lerp(pushedObjectOrig, pushedObjectTarget, movingAnimationCount / completeMovingTime); //.Translate(ForceDirection, Space.World);
                p_Obj.transform.position = Vector3.Lerp(playerMoveOrig, playerMoveTarget, movingAnimationCount / completeMovingTime); //.Translate(ForceDirection, Space.World);
            }
            else if (pullBackwards)
            {
                // move player first, then crate
                p_Obj.transform.position = Vector3.Lerp(playerMoveOrig, playerMoveTarget, movingAnimationCount / completeMovingTime); //.Translate(ForceDirection, Space.World);
                m_Character.grabbedBox.transform.position = Vector3.Lerp(pushedObjectOrig, pushedObjectTarget, movingAnimationCount / completeMovingTime); //.Translate(ForceDirection, Space.World);
            }
            if (movingAnimationCount >= completeMovingTime)
            {
                // we're done pushing!
                isInMovingAnimation = false;
                movingAnimationCount = 0.0f;
                if (!HoldingUseButton)
                {
                    // player let go of using button during the move animation
                    dropCrateWhenAnimationDone = true;
                }
            }
            //// Check if player releases the use button during the animation
            //if (Input.GetButtonUp("Fire3") || Input.GetKeyUp(KeyCode.E))
            //{
            //    // Player lets go of the grab button
            //    HoldingUseButton = false;
            //    // player will also release box when animation is finished
            //    m_Character.isGrabbingSomething = false;
            //}

            return;
        }

        // TODO: Is it OK to read these inputs in here? This is how the character was originally setup
        // from the asset store!
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");

        // calculate move direction to pass to character
        if (m_Cam != null)
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v * m_CamForward + h * m_Cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera   // TODO: We'll always have a camera, so remove this branch?
            m_Move = v * Vector3.forward + h * Vector3.right;
        }

        playerMoveInWorld = m_Move;
        /*
            *  So here for box pushing, there are basically three states:
            *      1) Regular, we're not holding anything and controlling the player. control player directly
            *      2) We are controlliing the robot, control the robot directly
            *      3) We are grabbing a box!
            *      4) We are moving a box! if we are, we don't want to grab any input and just play our animation
            */
        // pass all parameters to the character control script

        /*
         * todo: move each state to a separate function
         * - get clarification on box pushing specs, how do we know if a space is really empty or not?
         * - tween motion of char and crates
         * 
         */


        if (m_Character.isGrabbingSomething)
        {
            // ** STATE 3 **, we are the player and they are grabbing something
            m_Character.Move(Vector3.zero);

            // figure out which axis we should be locked into and only consider those components
            Vector3 grabbedMoveAmount = new Vector3(0, 0, 0);
            if (m_Character.lockOnXAxis)
            {
                grabbedMoveAmount = new Vector3(0, 0, m_Move.z);
            }
            else if (m_Character.lockOnZAxis)
            {
                grabbedMoveAmount = new Vector3(m_Move.x, 0, 0);
            }
            if (grabbedMoveAmount.magnitude < 0.5)
            {
                // only accept BIG inputs, if they are just touching the stick a little, ignore it?
                // TODO: Move this to a public variable so we can tweak it
                return;
            }

            // SO if we know which way we're pushing, we should accumulate the movement for  bit
            // once we've applied enough pressure in one of these directions, we should begin a push in
            // that particular direction.

            // figure out differences between if we're pushing and pulling here
            if ((m_Character.transform.forward - grabbedMoveAmount).magnitude < (-m_Character.transform.forward - grabbedMoveAmount).magnitude)
            {
                // We're pushing
                if (pushForward)
                {
                    // we're already pushing forward so accumulate timer
                    PushPullTimer += Time.deltaTime;
                }
                else
                {
                    // we've just started pushing
                    pullBackwards = false;
                    pushForward = true;
                    PushPullTimer = 0.0f;
                }
            }
            else
            {
                // we're pulling
                if (pullBackwards)
                {
                    // we've already been pulling, so accumulate timer
                    PushPullTimer += Time.deltaTime;
                }
                else
                {
                    // we've just started pulling
                    pushForward = false;
                    pullBackwards = true;
                    PushPullTimer = 0.0f;
                }
            }

            // So, we've got enough force, NOW cast a ray from either the front of the box or the back of the cbaracter
            // into the appropriate place in the square either in fron of the box or behind the player
            // if this raycast doesn't hit anything, we can move into the next square
            // so tween push the box and character to the next appropriate location 
            // either be (1, 0, 0), (-1, 0, 0), (0, 0, 1) or (0, 0, -1)
            // we should probably reset some things after that such as the push timer?

            if (PushPullTimer >= PushPullThreshold)
            {
                PushPullTimer = 0.0f;   // reset the timer (Don't clear flag so player can just hold push or pull direction to continously do that)
                Vector3 ForceDirection = transform.forward;
                Vector3 RayOrig = new Vector3(0, 0, 0);
                if (pullBackwards)
                {
                    // we're trying to pull towards us!
                    ForceDirection = -ForceDirection;
                }

                Vector3 halfBoxSize = new Vector3(0, 0, 0);
                if (m_Character.lockOnZAxis)
                {
                    halfBoxSize = new Vector3(0.45f, 0.9f, 0.9f);
                }
                else if (m_Character.lockOnXAxis)
                {
                    halfBoxSize = new Vector3(0.9f, 0.9f, 0.45f);
                }

                // Here, we check if there is anything in the way of where we are going to place the box
                // Since we are checking from the crates transform, if we are pushing forward, the center of the box we check with is 1.5 units if we are pushing forward
                // or 2.5 units if we are pulling toward the player (Since we include the players grid square, plus we need to check the one behind the player)
                bool canMove = true;
                Collider[] hitColliders = Physics.OverlapBox(m_Character.grabbedBox.transform.position + (ForceDirection * (pushForward ? 1.5f : 2.5f)), halfBoxSize, Quaternion.identity, m_LayerMask);
                if (hitColliders.Length != 0)
                {
                    foreach (Collider c in hitColliders)
                    {
                        if (c.gameObject.CompareTag("Player") || c.gameObject.CompareTag("robot") || c.gameObject == this.gameObject)
                        {
                            // Ignore player, robot, and ourselves
                            continue;
                        }
                        else
                        {
                            // We've found a non-player, non-robot collider, so don't allow the move to happen
                            canMove = false;
                            break;
                        }
                    }
                }

                if (!canMove)
                {
                    // can't move so we're done here?
                    return;
                }

                playerMoveOrig = p_Obj.transform.position;
                pushedObjectOrig = m_Character.grabbedBox.transform.position;
                playerMoveTarget = p_Obj.transform.position + ForceDirection;
                pushedObjectTarget = m_Character.grabbedBox.transform.position + ForceDirection;
                movingAnimationCount = 0.0f;
                isInMovingAnimation = true;

            }
        }
        else
        {
            if (playerSelected)
            {
                // ** STATE 1, we are controlling the player directly **
                m_Character.Move(m_Move /*, crouch, m_Jump*/);
            }
            else
            {
                // ** STATE 2, we are controlling the robot directly **
                m_Character.GetComponent<ThirdPersonCharacter>().StopMoving();  // does this need to happen every time?! we can move this somewhere else!

                // Ok, we're controlling the robot
                // we control directly here
                if (m_Move.magnitude > 0.1)
                {
                    if (selected.gameObject.tag == "robot")
                    {
                        selected.GetComponent<RobotBuddy>().breakranks();
                    }
                }
                selected.GetComponent<RobotBuddy>().Move(m_Move);     //normalized prevents char moving faster than it should with diagonal input
            }
        }
    }

    public GameObject GetSelectedCharacter()
    {
        return selected;
    }
}
