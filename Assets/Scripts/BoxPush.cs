using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
//using UnityStandardAssets.Characters.ThirdPerson;

// INSTEAD OF BOX PUSHING, MAKE IT LIKE A GRAVITY GUN KINDA THING THAT CAN MOVE CRATES AROUND

public class BoxPush : MonoBehaviour
{
    Transform player;
    public GameObject playerObject;
    // public ThirdPersonUserControl playerControls;
    public float maxAngle;
    private bool grabbing;

    private bool sameLevel;

    private float xDist;
    private float zDist;

    private bool snapOnce;

    private Transform originalParent;

    Rigidbody playerRidgidBody;

    // bool HoldingGrabController;

    bool canPushCrate;
    bool playerHoldingUse = false;
    bool playerGrabbing = false;
    bool playerIsPushing = false;
    float secondsOfPushing = 0.0f;
    float pushThreshold = 0.8f;

    private bool hasJoint;
    private FixedJoint grab;

    private Rigidbody boxRigidbody;


    // Start is called before the first frame update
    void Start()
    {
        player = playerObject.transform;
        playerRidgidBody = playerObject.GetComponent<Rigidbody>();
        // playerControls = playerObject.GetComponent<ThirdPersonUserControl>();
        boxRigidbody = GetComponent<Rigidbody>();
        // Store our original parent so we can restore once player releases their grasp
        originalParent = transform.parent;
        snapOnce = true;
        hasJoint = false;
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
        - don't lock the player to the box, figure out the force the player would be exerting on 
        the box, and push it but make it's always lined up with x or z axis?
        */

        // Is the player currently in a state where they can push the box?
        canPushCrate = (cubeDir.magnitude < 3.0f && sameLevel && angle < maxAngle); // TODO: Make this public and tweak to find good-good

        // check here if the player is holding down the use ky
        if (Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.E))
        {
            // Player has starte holding down the grab button
            playerHoldingUse = true;
        }
        if (Input.GetButtonUp("Fire3") || Input.GetKeyUp(KeyCode.E))
        {
            // Player lets go of the grab button
            playerHoldingUse = false;
        }

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
            player.GetComponent<ThirdPersonCharacter>().isGrabbingSomething = false;
            secondsOfPushing = 0.0f;
            playerIsPushing = false;
            playerGrabbing = false;
            hasJoint = false;
            Destroy(grab);  // break joint connecting
            boxRigidbody.drag = 1;
            boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            // boxRigidbody.drag = 9999;
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
                    player.position = new Vector3(transform.position.x, player.position.y, player.position.z);
                    Vector3 lookTarget = new Vector3(transform.position.x, player.position.y, transform.position.z);
                    player.LookAt(lookTarget);
                }
                // transform.parent = player;
                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                boxRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;

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

            }
            boxRigidbody.drag = 0;
            // create a joint between box and player
            //grab = new FixedJoint();
            //grab.breakForce = 0.5f;
            //grab.connectedBody = playerRidgidBody;
            if (!hasJoint)
            {
                grab = transform.gameObject.AddComponent<FixedJoint>();
                grab.breakForce = 50;
                // j.massScale = j.connectedBody.mass / root.GetComponent<Rigidbody>().mass;
                grab.massScale = 5f;
                grab.connectedBody = playerRidgidBody;
                hasJoint = true;
            }
        }
    }
}
