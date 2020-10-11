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

    private Transform originalParent;

    Rigidbody playerRidgidBody;

    // Start is called before the first frame update
    void Start()
    {
        player = playerObject.transform;
        playerRidgidBody = playerObject.GetComponent<Rigidbody>();
        originalParent = transform.parent;
    }


    //this whole thing aint working, switch to get the movement, and if the player is pressing the use button
    // Update is called once per frame
    void Update()
    {

        
        var cubeDir = transform.position - player.position;
        var angle = Vector3.Angle(cubeDir, player.forward);

        if (!Input.GetKey(KeyCode.E))
        {
            timer = 0;
        }

        //if the player is next to the box, on the same plane (idk if == is the right move here)\

        sameLevel = (Mathf.Abs(transform.position.y - player.position.y) < 1.0f);
        if (cubeDir.magnitude < 1.5f && sameLevel){
            //Debug.Log("Close enough to grab?");
            if (angle < maxAngle) {
                //put the prompt on the screen to grab
                // maybe if we want to make this work with a controller we actually need to check and set a flag here
                // ie. onButtonDown set it and then onButtonUp (????) clear flag!
                if (Input.GetKey(KeyCode.E)|| Input.GetButtonDown("Fire3"))
                {
                    //take the prompt down
                    Debug.Log("Grabbing!");
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
                            // Ok so this works with keyboard but not controller?
                            player.GetComponent<ThirdPersonCharacter>().isGrabbingSomething = true;
                            // TODO: This is where we would make the characters animation change?

                            xDist = Mathf.Abs(transform.position.x - player.position.x);
                            zDist = Mathf.Abs(transform.position.z - player.position.z);

                            // (Mathf.Floor(transform.position.x) == Mathf.Floor(player.position.x))
                            if (xDist < zDist)
                            {
                                //Debug.Log("Im IN!");
                                transform.parent = player;
                                // TODO: We want to snap the player to the box here?
                                // player.position = new Vector3(transform.position.x, player.position.y, player.position.z);
                                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation |
                                RigidbodyConstraints.FreezePositionX;

                                // if theyre on the same z axis, lock it to x movement
                            }
                            else // if (Mathf.Floor(transform.position.z) == Mathf.Floor(player.position.z))
                            {
                                //Debug.Log("Im IN!");
                                transform.parent = player;
                                // player.position = new Vector3(player.position.x, player.position.y, transform.position.z);
                                playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotation |
                                RigidbodyConstraints.FreezePositionZ;
                            }

                        }
                    }
                    //if theyre on the same x axis, lock it to z movement
                }
                else
                {
                    player.GetComponent<ThirdPersonCharacter>().isGrabbingSomething = false;
                    // let go of box
                    transform.parent = originalParent;
                    grabbing = false;
                    // unfreeze player rotations
                    playerRidgidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                }
            }
        }    

    }
}
