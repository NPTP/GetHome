using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
//using UnityStandardAssets.Characters.ThirdPerson;

// INSTEAD OF BOX PUSHING, MAKE IT LIKE A GRAVITY GUN KINDA THING THAT CAN MOVE CRATES AROUND

public class BoxPush : MonoBehaviour
{
    Transform player;
    private GameObject playerObject;
    public float maxAngle = 50;
    private bool grabbing;

    private bool sameLevel;

    private float xDist;
    private float zDist;

    private bool snapOnce;

    private Transform originalParent;

    Rigidbody playerRidgidBody;

    bool canPushCrate;
    bool playerHoldingUse = false;
    bool playerGrabbing = false;
    bool playerIsPushing = false;
    float secondsOfPushing = 0.0f;
    public float pushThreshold = 0.2f;

    private Rigidbody boxRigidbody;
    private ThirdPersonUserControl playerControls;
    private ThirdPersonCharacter m_Character;


    // Start is called before the first frame update
    void Start()
    {
        //player = playerObject.transform;
        playerObject = GameObject.FindWithTag("Player");
        player = playerObject.transform;

        playerRidgidBody = playerObject.GetComponent<Rigidbody>();
        playerControls = playerObject.GetComponent<ThirdPersonUserControl>();
        m_Character = playerObject.GetComponent<ThirdPersonCharacter>();

        // playerControls = playerObject.GetComponent<ThirdPersonUserControl>();
        boxRigidbody = GetComponent<Rigidbody>();
        // Store our original parent so we can restore once player releases their grasp
        originalParent = transform.parent;
        snapOnce = true;
    }

    void ShowPrompt(float pushTime)
    {
        return;
    }

    // Update is called once per frame
    void Update()
    {
        var cubeDir = transform.position - player.position;
        var angle = Vector3.Angle(cubeDir, player.forward);

        //if the player is next to the box, on the same plane (Within reason, this might need to be samller)
        sameLevel = (Mathf.Abs(transform.position.y - player.position.y) < 1.5f);   // TODO: Make this variable so we can adjust
        float PlayerToBoxLevelDistance = Mathf.Abs(transform.position.y - player.position.y);
        // If you want to see info about a box, just tag it TestBox
        if (gameObject.tag == "TestBox")
        {
            Debug.Log("PlayerToBoxDistance: " + PlayerToBoxLevelDistance);
            Debug.Log("Angel: " + angle);
            Debug.Log("CubeMagnitude: " + cubeDir.magnitude);
        }
        /*
        First, check that we're within some distance and we're at the same level
        - if we are display the grab prompt
        - if they've been holding down push for long enough near the box, let the player grab it
        - if the player grabs the box, make the character move to where they should be standing
        while thy are going to push the box
        - Once player is grabbing the crate, we snap into quantized movement
        */

        // Is the player currently in a state where they can push the box?
        canPushCrate = (cubeDir.magnitude < 3.0f && sameLevel && angle < maxAngle); // TODO: Make this public and tweak to find good-good

        // check here if the player is holding down the use key, we let this be dealt with in TPUserControls
        // and just grab the flag from there
        playerHoldingUse = playerControls.HoldingUseButton;

        if (canPushCrate)
        {
            // TODO: Display "Push" here
            // check if the player is holding down the E key
            // or they are holding down the proper button on
            // controller
            ShowPrompt(secondsOfPushing);
        }

        // we can push the crate and we're holding the use button
        if (canPushCrate && playerHoldingUse)
        {
            if (!playerGrabbing)
            {
                // this is triggered the first time the player is pressing use and
                // is close to the crate, so we 
                playerGrabbing = true;
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
            // TODO: Is there other ways to get into this state that we need to check?
            // here, we let go of pushing a box
            // let go of box
            transform.parent = originalParent;
            // reset timer and flags
            snapOnce = true;
            m_Character.isGrabbingSomething = false;
            m_Character.grabbedBox = null;
            m_Character.lockOnZAxis = false;
            m_Character.lockOnXAxis = false;
            secondsOfPushing = 0.0f;
            playerIsPushing = false;
            playerGrabbing = false;
            boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }

        if (playerIsPushing)
        {
            // here, we've just started pushing
            // let the character know we're grabbing something 
            // TODO: This is where we would make the characters animation change?
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
                    player.position = new Vector3(transform.position.x, player.position.y, player.position.z);  //TODO: This may need to be closer to the box at somepoint
                    Vector3 lookTarget = new Vector3(transform.position.x, player.position.y, transform.position.z);
                    player.LookAt(lookTarget);
                }
                // transform.parent = player;
                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                m_Character.lockOnXAxis = true;

                // if theyre on the same z axis, lock it to x movement
            }
            else // zDist <= xDist
            {
                if (snapOnce)
                {
                    snapOnce = false;
                    player.position = new Vector3(player.position.x, player.position.y, transform.position.z);
                    Vector3 lookTarget = new Vector3(transform.position.x, player.position.y, transform.position.z);
                    player.LookAt(lookTarget);
                }
                // transform.parent = player;
                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                m_Character.lockOnZAxis = true;
            }
            m_Character.grabbedBox = this.gameObject;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent red and green cube at the transforms positions that are checked for objects
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position + (new Vector3(1, 0, 0) * 1.5f), new Vector3(1, 1.8f, 1.8f));    // Note that these Vector3 are SIZE but when we BoxCast we are using HalfExtents
        Gizmos.DrawCube(transform.position + (new Vector3(-1, 0, 0) * 1.5f), new Vector3(1, 1.8f, 1.8f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, 1) * 1.5f), new Vector3(1.8f, 1.8f, 1.0f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, -1) * 1.5f), new Vector3(1.8f, 1.8f, 1.0f));

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position + (new Vector3(1, 0, 0) * 2.5f), new Vector3(1, 1.8f, 1.8f));    // Note that these Vector3 are SIZE but when we BoxCast we are using HalfExtents
        Gizmos.DrawCube(transform.position + (new Vector3(-1, 0, 0) * 2.5f), new Vector3(1, 1.8f, 1.8f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, 1) * 2.5f), new Vector3(1.8f, 1.8f, 1.0f));
        Gizmos.DrawCube(transform.position + (new Vector3(0, 0, -1) * 2.5f), new Vector3(1.8f, 1.8f, 1.0f));
    }

}
