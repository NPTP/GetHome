using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonUserControl : MonoBehaviour
{
    StateManager stateManager;
    public event EventHandler<SwitchCharArgs> OnSwitchChar;
    public class SwitchCharArgs : EventArgs
    {
        public GameObject selected;
    }

    private SceneLoader sl;

    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    //private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

    private GameObject selected;            // keep track of whether the player or a bot is selected
    public GameObject firstbot;             // points to the first robot object
    private GameObject p_Obj;               // keep track of the players gameobject
    private GameObject pauseEffect;         // vhs pause effect
    private RobotBuddy r_Character;

    public float roboSpeed = 3.0f;

    [Tooltip("Amount of time triggers need to be held down to reset scene")]
    public float resetSceneTimer = 1.5f;

    private bool playerSelected;

    public Vector3 playerMoveInWorld;

    private float resetSceneCount;
    public bool HoldingUseButton;           // we'll check here if the player is holding down the use button so any script can just grab from here

    private float PushPullTimer;    // counter to accumulate pushing and pulling in

    [Tooltip("The amount of seconds the player has to push/pull before action is initiated")]
    public float PushPullThreshold = 1.0f;

    private LayerMask m_LayerMask;

    [Tooltip("Amount of seconds it takes to push a crate from one space to another")]
    public float completeMovingTime = 2.0f; // how many seconds does it take to complete the push/pull animation?
    private float movingAnimationCount;
    public bool isInMovingAnimation;        // if we are in the crate moving animation, lock input and just slide char
    private Vector3 playerMoveTarget;       // where are we moving the player TO
    private Vector3 pushedObjectTarget;     // where are we moving the crate TO 
    private Vector3 playerMoveOrig;         // where are we moving the player FROM
    private Vector3 pushedObjectOrig;       // where are we moving the crate FROM
    private BoxStacking boxstack;

    private GameObject pauseMenu;

    [Tooltip("If the robot is this distance or less away, just make the bot follow again, don't warp (Assuming it's a clear path)")]
    public float OnlyFollowNoWarpDistance = 5.0f;       // If we're this distance or less away, just start following again!

    bool firstPush;                         // first push should be a bit shorter than the rest
    bool pushForward;                       // players current status is pushing a crate forward
    bool pullBackwards;                     // players current status is pulling a crate towards themselves

    bool dropCrateWhenAnimationDone;

    bool isPaused;

    bool robotFollowing;
    public bool canSelectBot;

    private void Start()
    {
        stateManager = GameObject.FindObjectOfType<StateManager>();
        sl = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();

        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPersonCharacter>();
        r_Character = GameObject.FindGameObjectWithTag("robot").GetComponent<RobotBuddy>();
        if (firstbot == null) firstbot = GameObject.FindWithTag("robot");

        resetSceneCount = 0;
        // Our player starts selected (Instead of robot)
        selected = this.gameObject;
        p_Obj = this.gameObject;
        playerSelected = true;
        // get the transform of the main camera
        m_Cam = Camera.main.transform;
        robotFollowing = true;

        // Pause Menu setup
        pauseMenu = GameObject.FindWithTag("PauseMenu");
        pauseMenu.SetActive(false);
        pauseEffect = GameObject.FindWithTag("VHSPauseEffect");
        pauseEffect.SetActive(false);
        isPaused = false;

        // Flags and counters for pushing and pulling crates
        firstPush = true;
        PushPullTimer = 0.0f;
        pushForward = false;
        pullBackwards = false;
        isInMovingAnimation = false;
        movingAnimationCount = 0.0f;
        dropCrateWhenAnimationDone = false;

        m_LayerMask = ~(1 << 17 | 1 << 11 | 1 << 12);    // don't collide with occlusion volumes, triggers, or NoFlip Zones
    }

    public GameObject GetSelected()
    {
        return selected;
    }

    public void unpause()
    {
        Time.timeScale = 1;
        pauseEffect.SetActive(false);
        pauseMenu.SetActive(false);
        isPaused = false;
        pauseEffect.GetComponent<AudioSource>().Stop();
        GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>().UnPause();
        stateManager.SetPaused(false);
    }

    private void Update()
    {


        // always check for scene reset
        // check for reset scene by holding down triggers
        // TODO: We use Input sometimes and CrossPlatformInputManager other times? Not ideal :(
        float lTrigger = Input.GetAxis("TriggerL");
        float rTrigger = Input.GetAxis("TriggerR");

        if ((lTrigger > 0.8f && rTrigger > 0.8f) || (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.T)))
        {
            resetSceneCount += Time.deltaTime;
        }
        else
        {
            resetSceneCount = 0;
        }

        if (resetSceneCount > resetSceneTimer)      // Reset scenes like this for checkpoint system
        {
            ResetScene();
        }

        // always check for pause menu, no matter the state
        if (Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Time.timeScale = 0;
                pauseEffect.SetActive(true);
                GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>().Pause();
                pauseEffect.GetComponent<AudioSource>().Play();
                pauseMenu.SetActive(true);
                pauseMenu.GetComponentInChildren<PauseMenuManager>().SelectFirstButton();
                r_Character.StopFootsounds();
                stateManager.SetPaused(true);
            }
            else
            {
                unpause();
            }
        }

        // Don't take inputs for character movement unless we're in the right state.
        StateManager.State state = stateManager.GetState();
        if (state != StateManager.State.Normal && state != StateManager.State.Looking)
            return;



        // make sure we always check if we're holding the use button or not
        // since other scripts may depend on this happening?
        if (Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.E))
        {
            // Player has starte holding down the grab button
            HoldingUseButton = true;
        }
        if (Input.GetButtonUp("Interact") || Input.GetKeyUp(KeyCode.E))
        {
            // Player lets go of the grab button
            HoldingUseButton = false;
        }

        // check for pausing
        // TODO: Move all this to StateManager?


        // if we're in a pushing animation, don't deal with input for now
        if (isInMovingAnimation)
        {
            return;
        }

        // Only allow inputs when not gravity-flipping.
        if (stateManager.CheckReadyToFlip())
        {
            // check to recapture bot
            if (canSelectBot && (Input.GetButtonDown("CaptureRobot") || Input.GetKeyDown(KeyCode.C)))
            {
                // Vector between player middle and robot transform used for various calulations below
                Vector3 diff = transform.position + new Vector3(0, 0.8f, 0) - firstbot.transform.position;

                // Check if we're within warping distance or just refollowing distance
                if (diff.magnitude < OnlyFollowNoWarpDistance)
                {
                    // Check if there are any walls or other colliders between player and bot
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(firstbot.transform.position, diff, diff.magnitude, m_LayerMask);
                    bool blocked = false;
                    foreach (var didhit in hits)
                    {
                        if (didhit.transform.gameObject.CompareTag("Player") || didhit.transform.gameObject.CompareTag("robot"))
                        {
                            continue;
                        }
                        else
                        {
                            blocked = true;
                        }
                    }

                    if (!blocked)
                    {
                        // if we get here, robot is close to player and there is nothing between them, so just set the robot
                        // to begin following the player again
                        r_Character.unbreakranks(); // make it follow the player again
                        return;
                    }
                }

                // If we get here, either the player is far from the robot OR there is some collider betweent the robot and the player
                // ie. we are on the otherside of a thin wall or something

                // Find a place around us to warp the robot to
                // make a list of places we should check
                Vector3 playpos = transform.position + new Vector3(0, 1, 0);
                // check behind the player first since that's the most natural place to put the robot, then beside, then in front only in a pickler
                Vector3[] offsets = { playpos - transform.forward, playpos + transform.right, playpos - transform.right, playpos + transform.forward };
                Vector3 warpTo = new Vector3(0, 0, 0);
                bool foundSpot = false;
                foreach (Vector3 checkLoc in offsets)
                {
                    if (!Physics.CheckSphere(checkLoc, 0.2f, m_LayerMask))
                    {
                        warpTo = checkLoc;
                        foundSpot = true;
                        break;
                    }
                }
                if (!foundSpot)
                {
                    // couldn't find somewhere to warp the bot, FUGGEDABOUDIT
                    
                    m_Character.DoError("CAN'T WARP ROBOT HERE");
                    return;
                }
                r_Character.WarpToPlayer(warpTo);
                r_Character.unbreakranks(); // make it follow the player again
            }

            if (canSelectBot && Input.GetButtonDown("SwitchChar"))
            {
                SwitchChar();
            }
        }



        // read inputs
        // Check if we should end the game!
        // this eventually will kick to main menu or something instead




        if (Input.GetKey(KeyCode.N) && Input.GetKey(KeyCode.M))
        {
            if (sl)
                sl.LoadNextScene();
        }
    }

    public void ResetScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void SwitchChar()
    {
        // Here we change to our robot!
        // If we're only doing one robot then there is an easier way to do this
        // but for now we'll keep it scalable!
        if (playerSelected)
        {
            selected = firstbot;
            stateManager.SetSelected(firstbot);
            firstbot.GetComponent<Light>().color = Color.green;
            playerSelected = false;
            m_Character.GetComponent<ThirdPersonCharacter>().StopMoving();
        }
        else
        {
            // We had the robot selected, change to the player
            selected.GetComponent<RobotBuddy>().StopMoving();
            selected = this.gameObject;
            playerSelected = true;
            firstbot.GetComponent<Light>().color = Color.red;
            selected = this.gameObject;
            stateManager.SetSelected(this.gameObject);
        }

        // Send the camera to whatever game object we're currently selecting
        // and send an event that we have changed characters.
        CameraControl.CC.ChangeTarget(selected.transform, .4f);
        OnSwitchChar?.Invoke(this, new SwitchCharArgs { selected = this.selected });
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        // TODO: Inputs should be taken in Update() only so as to avoid missed inputs on the fixed timestep,
        // setting bools which are then read by FixedUpdate(). Also will make it easier to stop all inputs
        // during a gravity flip by only having to block them in one place (Update). For now, for the
        // prototype, we block inputs in both functions.

        // Only allow inputs when not gravity-flipping.
        if (!stateManager.CheckReadyToFlip())
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
            firstPush = true;
        }

        if (isInMovingAnimation)
        {
            movingAnimationCount += Time.fixedDeltaTime;
            // Right now, this is just snapping the transform to the next square, so let's change that a bit!
            Vector3 moveamount = p_Obj.transform.position;
            Vector3 movXZ = new Vector3(moveamount.x, 0, moveamount.z);
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
            //moveamount = p_Obj.transform.position - moveamount;
            Vector3 pObjPosXZ = new Vector3(p_Obj.transform.position.x, 0, p_Obj.transform.position.z);
            movXZ = pObjPosXZ - movXZ;
            m_Character.grabbedBox.GetComponent<BoxStacking>().DoMove(movXZ);
            // hack to make it look like the char is at least trying
            // TODO: Fix this when we get better animations
            m_Character.DoPushPullAnim((movXZ.magnitude * 3) * (pullBackwards ? -2 : 1 ));
            if (movingAnimationCount >= completeMovingTime)
            {
                m_Character.StopPushPullAnim();
                // we're done pushing!
                isInMovingAnimation = false;
                movingAnimationCount = 0.0f;
                m_Character.grabbedBox.GetComponent<BoxPush>().StopSound();
                if (!HoldingUseButton)
                {
                    // player let go of using button during the move animation
                    dropCrateWhenAnimationDone = true;
                }
            }
            return;
        }

        // TODO: Is it OK to read these inputs in here? This is how the character was originally setup
        // from the asset store!
        float h = 0f;
        float v = 0f;
        StateManager.State state = stateManager.GetState();
        if (state == StateManager.State.Normal || state == StateManager.State.Looking)
        {
            h = CrossPlatformInputManager.GetAxis("Horizontal");
            v = CrossPlatformInputManager.GetAxis("Vertical");
        }

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
         * todo: boxes should move two squares at once
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

            if ((firstPush && PushPullTimer >= PushPullThreshold / 3.0f) || PushPullTimer >= PushPullThreshold)    // make first push shorter
            {
                PushPullTimer = 0.0f;   // reset the timer (Don't clear flag so player can just hold push or pull direction to continously do that)
                Vector3 ForceDirection = transform.forward;
                if (pullBackwards)
                {
                    // we're trying to pull towards us!
                    ForceDirection = -ForceDirection;
                }

                Vector3 halfBoxSize;
                if (pullBackwards)
                {
                    halfBoxSize = new Vector3(0.9f, 0.7f, 0.9f);    // if we're pulling bacwards, allow our player some wiggle room to step onto small curbs
                }
                else
                { 
                    halfBoxSize = new Vector3(0.9f, 0.9f, 0.9f);    // if we're pushing forward, check with no mercy
                }

                // Here, we check if there is anything in the way of where we are going to place the box
                // Since we are checking from the crates transform, if we are pushing forward, the center of the box we check with is 1.5 units if we are pushing forward
                // or 2.5 units if we are pulling toward the player (Since we include the players grid square, plus we need to check the one behind the player)
                bool canMove = true;


                // check behind or in front, as needed
                Collider[] hitColliders = Physics.OverlapBox(m_Character.grabbedBox.transform.position + (ForceDirection * (pushForward ? 2 : 3)), halfBoxSize, Quaternion.identity, m_LayerMask);

                if (pullBackwards)
                {
                    // if we're pulling backwards, check where the player is currently standing;
                    Collider[] pHitCollider = Physics.OverlapBox(m_Character.grabbedBox.transform.position + (ForceDirection * 2), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.identity, m_LayerMask);
                    int hitCollidersOgSize = hitColliders.Length;
                    Array.Resize<Collider>(ref hitColliders, hitCollidersOgSize + pHitCollider.Length);
                    Array.Copy(pHitCollider, 0, hitColliders, hitCollidersOgSize, pHitCollider.Length);
                }

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
                            if (pushForward)
                            {
                                m_Character.DoError("CAN'T PUSH CRATE HERE");
                            }
                            else
                            {
                                m_Character.DoError("CAN'T PULL CRATE HERE");
                            }
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

                // Once we get here, we can start pushing the crate!
                m_Character.StartPushPullAnim();            // start pushing animation
                firstPush = false;
                playerMoveOrig = p_Obj.transform.position;
                pushedObjectOrig = m_Character.grabbedBox.transform.position;
                playerMoveTarget = p_Obj.transform.position + ForceDirection * 2;   // * 2 since we're moving 2 game units!
                pushedObjectTarget = m_Character.grabbedBox.transform.position + ForceDirection * 2;    // ""        ""
                movingAnimationCount = 0.0f;                                        // reset the moving animation timer
                isInMovingAnimation = true;
                // play the pushing sound
                m_Character.grabbedBox.GetComponent<BoxPush>().PlaySound();
            }
        }
        else
        {
            if (playerSelected)
            {
                // ** STATE 1, we are controlling the player directly **
                m_Character.Move(m_Move);
            }
            else
            {
                // ** STATE 2, we are controlling the robot directly **

                // Ok, we're controlling the robot
                r_Character.Move(m_Move);     //normalized prevents char moving faster than it should with diagonal input
                if (robotFollowing && m_Move.magnitude > 0.2f)  // 0.2f is sort of an arbitrary number, represented a decent move
                {
                    r_Character.breakranks();     //if we actually move bot, make it not follow anymore

                }
            }
        }
    }

    public GameObject GetSelectedCharacter()
    {
        return selected;
    }
}
