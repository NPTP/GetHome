using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityStandardAssets.Characters.ThirdPerson;

public class BoxPush : MonoBehaviour
{
    Transform player;
    public GameObject playerObject;
    private float timer = 0.0f;
    public float maxAngle;
    private bool grabbing;

    private bool sameLevel;

    private float xDist;
    private float zDist;

    private bool snapOnce;

    private Transform originalParent;

    Rigidbody playerRidgidBody;

    bool HoldingGrabController;

    // Start is called before the first frame update
    void Start()
    {
        player = playerObject.transform;
        playerRidgidBody = playerObject.GetComponent<Rigidbody>();
        // Store our original parent so we can restore once player releases their grasp
        originalParent = transform.parent;
        snapOnce = true;
    }

    // Update is called once per frame
    void Update()
    {

        
        var cubeDir = transform.position - player.position;
        var angle = Vector3.Angle(cubeDir, player.forward);

        if (!Input.GetKey(KeyCode.E))
        {
            timer = 0;
        }

        //if the player is next to the box, on the same plane (Within reason, this might need to be samller)
        sameLevel = (Mathf.Abs(transform.position.y - player.position.y) < 2.5f);
        float PlayerToBoxLevelDistance = Mathf.Abs(transform.position.y - player.position.y);
        // If you want to see info about a box, just tag it TestBox
        if (gameObject.tag == "TestBox")
        {
            Debug.Log("PlayerToBoxDistance: " + PlayerToBoxLevelDistance);
            Debug.Log("Angel: " + angle);
            Debug.Log("CubeMagnitude: " + cubeDir.magnitude);
        }
        if (cubeDir.magnitude < 5.0f && sameLevel){

            if (angle < maxAngle) {

                // TODO: Put prompt on screen to grab box

                if (Input.GetButtonDown("Fire3"))
                {
                    // Player has starte holding down the grab button
                    HoldingGrabController = true;
                }
                if (Input.GetButtonUp("Fire3"))
                {
                    // Player lets go of the grab button
                    HoldingGrabController = false;
                }

                if (Input.GetKey(KeyCode.E)|| HoldingGrabController)
                {
                    //take the prompt down
                    if (!grabbing)
                    {
                        // here we've just started grabbing a box!
                        grabbing = true;
                        timer = 20.0f;
                    }
                    else
                    {
                        if (timer > 0)
                        {
                            timer--;
                        }
                        else
                        {

                            player.GetComponent<ThirdPersonCharacter>().isGrabbingSomething = true;
                            // TODO: This is where we would make the characters animation change?

                            // figure out if we're closer to being locked into the X- or Z- axis
                            xDist = Mathf.Abs(transform.position.x - player.position.x);
                            zDist = Mathf.Abs(transform.position.z - player.position.z);

                            // (Mathf.Floor(transform.position.x) == Mathf.Floor(player.position.x))
                            if (xDist < zDist)
                            {
                                transform.parent = player;
                                // TODO: We want to snap the player to the box here?
                                if (snapOnce)
                                {
                                    snapOnce = false;
                                    player.position = new Vector3(transform.position.x + (transform.localScale.x /2), player.position.y, player.position.z);
                                }
                                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation |
                                RigidbodyConstraints.FreezePositionZ;   

                                // if theyre on the same z axis, lock it to x movement
                            }
                            else // if (Mathf.Floor(transform.position.z) == Mathf.Floor(player.position.z))
                            {
                                transform.parent = player;
                                if (snapOnce)
                                {
                                    snapOnce = false;
                                    player.position = new Vector3(player.position.x, player.position.y, transform.position.z + (transform.localScale.z / 2));
                                }
                                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation |
                                RigidbodyConstraints.FreezePositionX;
                            }

                        }
                    }

                }
                else
                {
                    snapOnce = true;
                    player.GetComponent<ThirdPersonCharacter>().isGrabbingSomething = false;
                    // let go of box
                    transform.parent = originalParent;
                    grabbing = false;
                    // unfreeze player rotations
                    playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                }
            }
        }
        else
        {
            // hmmm, is this how it works?
            HoldingGrabController = false;
        }

    }
}
