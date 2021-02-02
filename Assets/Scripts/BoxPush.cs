using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPush : MonoBehaviour
{
    StateManager stateManager;
    UIManager uiManager;
    bool showingPrompt = false;

    private ThirdPersonUserControl playerControls;
    private ThirdPersonCharacter m_Character;
    private GameObject playerObject;
    private Rigidbody playerRidgidBody;
    private Transform player;


    [Tooltip("How long does a player have to push before a crate moves")]
    public float pushThreshold = 0.2f;
    [Tooltip("From what distance away can a player snap to a crate")]
    public float maxGrabDistance = 2.0f;
    [Tooltip("Max. vertical distance to consider a player on the same level as a crate")]
    public float maxVerticalGrabDistance = 0.8f;
    [Tooltip("Maximum angle player can be away from crate")]
    public float maxAngle = 50;

    private bool grabbing;
    private bool sameLevel;
    private float xDist;
    private float zDist;
    private float playHalfHeight;
    private bool snapOnce;
    private float secondsOfPushing = 0.0f;
    private AudioSource pushAudio;
    private Rigidbody boxRigidbody;

    bool canPushCrate;
    bool playerHoldingUse = false;
    bool playerGrabbing = false;
    bool playerIsPushing = false;



    public void PlaySound()
    {
        if (!pushAudio.isPlaying)
        {
            // set random start time so it sounds a little different every time
            pushAudio.time = Random.Range(0, pushAudio.clip.length);
            pushAudio.Play();
        }
    }

    public void StopSound()
    {
        pushAudio.Stop();
    }

    // Start is called before the first frame update
    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        uiManager = FindObjectOfType<UIManager>();

        playerObject = GameObject.FindWithTag("Player");
        player = playerObject.transform;
        playHalfHeight = 0.8f;
        playerRidgidBody = playerObject.GetComponent<Rigidbody>();
        playerControls = playerObject.GetComponent<ThirdPersonUserControl>();
        playerControls.OnSwitchChar += HandleSwitchChar;
        m_Character = playerObject.GetComponent<ThirdPersonCharacter>();

        boxRigidbody = GetComponent<Rigidbody>();
        // Store our original parent so we can restore once player releases their grasp
        // originalParent = transform.parent;
        snapOnce = true;

        pushAudio = GetComponent<AudioSource>();
    }

    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        GameObject selected = args.selected;
        DropCrate();
        if (selected.tag == "Player" && canPushCrate)
        {
            ShowPrompt();
        }
    }

    void ShowPrompt()
    {
        uiManager.EnterRange(playerObject.tag, "(Hold)");
        showingPrompt = true;
    }

    void DropCrate()
    {
        // Here, we are letting go of grabbing a box
        // reset timer and flags
        snapOnce = true;
        m_Character.isGrabbingSomething = false;
        m_Character.disallowRotation = false;
        m_Character.StopPushPullAnim();
        m_Character.grabbedBox = null;
        m_Character.lockOnZAxis = false;
        m_Character.lockOnXAxis = false;
        secondsOfPushing = 0.0f;
        playerIsPushing = false;
        playerGrabbing = false;
        boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }

    void HidePrompt()
    {
        uiManager.ExitRange(playerObject.tag);
        showingPrompt = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (playerControls.isInMovingAnimation)
        {
            // don't check anything while we're in moving animation!
            return;
        }
        var cubeDir = transform.position - player.position;
        var angle = Vector3.Angle(cubeDir, player.forward);

        //if the player is next to the box, on the same plane (Within reason, this might need to be samller)
        sameLevel = (Mathf.Abs(transform.position.y - (player.position.y + playHalfHeight)) < maxVerticalGrabDistance);

        // Is the player currently in a state where they can push the box?
        canPushCrate = (cubeDir.magnitude < maxGrabDistance && sameLevel && angle < maxAngle);

        // check here if the player is holding down the use key, we let this be dealt with in TPUserControls
        // and just grab the flag from there
        playerHoldingUse = playerControls.HoldingUseButton;

        if (stateManager.GetSelected().tag == "Player" && canPushCrate)
        {
            // TODO: Display "Push" here?
            // check if the player is holding down the E key
            // or they are holding down the proper button on
            // controller
            ShowPrompt();
        }
        else if (showingPrompt)
        {
            HidePrompt();
        }

        // we can push the crate and we're holding the use button AND the player is selected
        if (!playerIsPushing && canPushCrate && playerHoldingUse && stateManager.GetSelected().tag == "Player")
        {
            if (!playerGrabbing)
            {
                // this is triggered the first time the player is pressing use and
                // is close to the crate, so we 
                playerGrabbing = true;
                m_Character.StopMoving();
                // once our character grabs something, stop taking rotational input
                m_Character.disallowRotation = true;
                m_Character.PlayGrabAnim();
                // initialize timer
                secondsOfPushing = 0.0f;
            }
            else if (playerGrabbing)
            {
                // here, we count up to a second
                secondsOfPushing += Time.deltaTime;
            }
        }
        // we are in pushing state, but still counting towards pushing
        if (!playerIsPushing && (secondsOfPushing > pushThreshold))
        {
            // initiate pushing of crate
            playerIsPushing = true;
        }

        if ((playerIsPushing || playerGrabbing) && !playerHoldingUse)
        {
            DropCrate();
        }

        if (playerIsPushing)
        {
            if (!playerHoldingUse)
            {
                DropCrate();
                playerControls.isGrabbing = playerGrabbing || playerIsPushing;
                return;
            }
            // here, we've just started pushing
            // let the character know we're grabbing something 
            player.GetComponent<ThirdPersonCharacter>().isGrabbingSomething = true;

            // figure out if we're closer to being locked into the X- or Z- axis
            xDist = Mathf.Abs(transform.position.x - player.position.x);
            zDist = Mathf.Abs(transform.position.z - player.position.z);

            // TODO: Animate snapping the player into position
            if (xDist < zDist)
            {
                if (snapOnce)
                {
                    snapOnce = false;
                    float pz = transform.position.z;
                    pz -= player.transform.forward.z * 1.7f;
                    player.position = new Vector3(transform.position.x, player.position.y, pz/*player.position.z*/);  //TODO: This may need to be closer to the box at somepoint
                    Vector3 lookTarget = new Vector3(transform.position.x, player.position.y, transform.position.z);
                    player.LookAt(lookTarget);
                }
                boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                m_Character.lockOnXAxis = true;
                // if theyre on the same z axis, lock it to x movement
            }
            else // zDist <= xDist
            {
                if (snapOnce)
                {
                    snapOnce = false;
                    float px = transform.position.x;
                    px -= player.transform.forward.x * 1.7f;
                    player.position = new Vector3(px/*player.position.x*/, player.position.y, transform.position.z);
                    Vector3 lookTarget = new Vector3(transform.position.x, player.position.y, transform.position.z);
                    player.LookAt(lookTarget);

                }
                boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                m_Character.lockOnZAxis = true;
            }

            playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
            m_Character.StopMoving();   // take away player momentum
            m_Character.grabbedBox = this.gameObject;
        }
        playerControls.isGrabbing = playerGrabbing || playerIsPushing;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent red and green cube at the transforms positions that are checked for objects
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position + (new Vector3(1, 0, 0) * 2), new Vector3(1.8f, 1.8f, 1.8f));    // Note that these Vector3 are SIZE but when we BoxCast we are using HalfExtents
        Gizmos.DrawCube(transform.position + (new Vector3(-1, 0, 0) * 2), new Vector3(1.8f, 1.8f, 1.8f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, 1f) * 2), new Vector3(1.8f, 1.8f, 1.8f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, -1f) * 2), new Vector3(1.8f, 1.8f, 1.8f));

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position + (new Vector3(1, 0, 0) * 3), new Vector3(1.8f, 1.8f, 1.8f));    // Note that these Vector3 are SIZE but when we BoxCast we are using HalfExtents
        Gizmos.DrawCube(transform.position + (new Vector3(-1, 0, 0) * 3), new Vector3(1.8f, 1.8f, 1.8f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, 1) * 3), new Vector3(1.8f, 1.8f, 1.8f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, -1) * 3), new Vector3(1.8f, 1.8f, 1.8f));
    }

}
