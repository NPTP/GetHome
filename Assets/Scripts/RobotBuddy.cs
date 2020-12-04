﻿using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RobotBuddy : MonoBehaviour
{
    StateManager stateManager;

    [SerializeField] float r_MovingTurnSpeed = 360;
    [SerializeField] float r_StationaryTurnSpeed = 180;
    [Range(1f, 4f)] [SerializeField] float r_GravityMultiplier = 2f;
    [SerializeField] float r_MoveSpeedMultiplier = 1f;
    [SerializeField] float r_AnimSpeedMultiplier = 1f;
    [SerializeField] float r_GroundCheckDistance = 1.5f;

    public float PlayerHeadTowardsMaxDistance = 2.5f;   // If we're more than this units far away, head towards the player
    public float PlayerAvoidMinDistance = 0.5f;         // if we're less than this many units close, back away from player

    private ThirdPersonCharacter playerThirdPersonCharacter;

    public GameObject following;
    public GameObject roboBody;
    public GameObject roboSpotlight;

    public GameObject warpPrefab;

    public bool used = false;
    public float speed = 3f;

    Rigidbody r_Rigidbody;
    Animator r_Animator;
    public bool r_IsGrounded;
    float r_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float r_TurnAmount;
    float r_ForwardAmount;
    Vector3 r_GroundNormal;
    Vector3 moveDelta;
    private AudioSource footsounds;
    private AudioSource warpsound;

    private float RecheckGroundFrames = 5;  // check for ground every 5 frames
    private float RecheckCount = 0;

    private ParticleSystem sparks;

    public Vector3 spotlightDirection = new Vector3(0, 1.5f, 0);

    private float timeSinceLastSpark;
 

    // Start is called before the first frame update
    void Start()
    {
        // controller = GetComponent<CharacterController>();
        stateManager = GameObject.FindObjectOfType<StateManager>();
        roboBody = GameObject.Find("RoboAnim");
        roboSpotlight = GameObject.Find("RobotSpotlight");

        if (!following) following = GameObject.FindWithTag("Player");
        playerThirdPersonCharacter = following.GetComponent<ThirdPersonCharacter>();
        r_Rigidbody = GetComponent<Rigidbody>();

        sparks = GameObject.Find("RoboSparks").GetComponent<ParticleSystem>();
        timeSinceLastSpark = 0.0f;

        r_Animator = GameObject.Find("RoboAnim").GetComponent<Animator>();
        footsounds = GetComponents<AudioSource>()[0];
        warpsound = GetComponents<AudioSource>()[1];

        r_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        r_OrigGroundCheckDistance = r_GroundCheckDistance;
        r_IsGrounded = true;    // start the robot grounded
    }

    public void WarpToPlayer(Vector3 warpTo)
    {
        Vector3 warpFrom = transform.position;  // Use this if we want to animate something at the place the robot warped from
        // TODO: When we get the command, warp this robot to be where the player is!

        // fade out char - coroutine and alpha?

        // move this gameobject - transform.translate? or just change transform.position = Vector3?
        StartCoroutine(WarpEffect(warpTo));
        warpsound.Play();
        // fade in char - coroutine and alpha?
        transform.position = warpTo;
    }

    IEnumerator WarpEffect(Vector3 warpTo)
    {
        GameObject fx = Instantiate(warpPrefab, warpTo, Quaternion.identity);
        yield return new WaitForSeconds(0.8f);
        Destroy(fx);
    }

    IEnumerator SparkEffect()
    {
        sparks.Play();
        float sparktotal = 0.7f + Random.Range(0, 0.2f);
        yield return new WaitForSeconds(sparktotal);
        sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

      public void PlayGravAnimation()
    {
        r_Animator.Play("GravFlip");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeSinceLastSpark += Time.deltaTime;


        if (timeSinceLastSpark > 10 && Random.Range(0, 100) == 1)
        {
            StartCoroutine("SparkEffect");
            timeSinceLastSpark = 0;
        }

        // Only allow movement when not gravity-flipping (even gravity is not applied during flip).
        StateManager.State state = stateManager.GetState();
        if (state == StateManager.State.Normal || state == StateManager.State.Looking)
        {
            if (r_IsGrounded && playerThirdPersonCharacter.m_IsGrounded && stateManager.CheckReadyToFlip())
            {
                Vector3 curpos = transform.position;                        // robot position
                Vector3 target = following.transform.position;              // player (or other target?) position
                Vector3 moveamount = (target - curpos).normalized;          // limit our movement amount, we'll multiply this by speed later
                moveamount.y = 0;
                float distToPlayer = (target - curpos).magnitude;           // how far is the player from the robot

                if (!used && (distToPlayer > PlayerHeadTowardsMaxDistance)) // Move towards the player if we're far away
                {
                    if (distToPlayer < (PlayerAvoidMinDistance * 2))  // dampen movement if we get too close to player
                    {
                        moveamount *= (distToPlayer / 2.0f);
                    }
                    Move(moveamount * speed);
                }
                else if ((stateManager.GetSelected() != this.gameObject) && distToPlayer < PlayerAvoidMinDistance)    // avoid the player if we're too close
                {
                    // TODO: Make the robot avoid player if the player walks towards the robot
                    // for now, just make the robot and player not collide
                    //moveamount = -moveamount;
                    //Move(moveamount);
                    //r_Rigidbody.velocity *= 0.5f;
                    r_Rigidbody.velocity = Vector3.zero;
                    r_Rigidbody.angularVelocity = Vector3.zero;
                }
                else if (distToPlayer < 0.15f)   // don't ever let us bump into player
                {
                    r_Rigidbody.velocity = Vector3.zero;
                    r_Rigidbody.angularVelocity = Vector3.zero;
                }
                moveDelta = new Vector3(moveamount.x / 20, 0, moveamount.z / 20);   // TODO: Temporary, this will be replaced with animator delta once we have animations!
            }
        }
        else
        {
            r_Rigidbody.velocity = Vector3.zero;
            r_Rigidbody.angularVelocity = Vector3.zero;
        }
        if (r_Rigidbody.velocity.magnitude < 0.15f)   // if we stopped moving / aren't moving lots?
        {
            footsounds.Stop();
        }
        UpdateAnimator(r_Rigidbody.velocity);
        if (stateManager.CheckReadyToFlip())
        {
            Vector3 newForwardLook = new Vector3(roboSpotlight.transform.forward.x, 0, roboSpotlight.transform.forward.z);
            if ((newForwardLook.sqrMagnitude > 0.01f))
            {
                roboBody.transform.forward = newForwardLook;
            }
        }
        // We don't want to get pushed around by the player
    }

    public void Move(Vector3 move)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        spotlightDirection = move.normalized;
        Vector3 movedupe = move;
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, r_GroundNormal);
        r_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (r_IsGrounded)
        {
            HandleGroundedMovement();
        }
        else
        {
            // TODO: We CAN'T move when we're in the air so CHANGE this setting!
            HandleAirborneMovement();
        }

        // send input and other state parameters to the animator
        UpdateAnimator(move);    // TODO

        // we preserve the existing y part of the current velocity.
        move *= speed;
        move.y = r_Rigidbody.velocity.y;
        r_Rigidbody.velocity = move;

        movedupe.y = 0;
        if (!footsounds.isPlaying && movedupe.magnitude >= 0.3f)
        {
            footsounds.Play();
        }
    }

    public void StopFootsounds()
    {
        footsounds.Stop();
    }

    public void setFollowing(GameObject tofollow)
    {
        following = tofollow;
    }

    public void breakranks()
    {
        used = true;
    }

    public void unbreakranks()
    {
        r_IsGrounded = true;
        used = false;
    }

    void CheckGroundStatus()
    {
        // increased offset from 0.1f to 0.5f to place origin of raycast further inside the character
        // float rayCastOriginOffset = 0.1f;

        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position, transform.position + Vector3.down);
#endif
        // rayCastOriginOffset is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0), Vector3.down, out hitInfo, r_GroundCheckDistance))
        {
            r_IsGrounded = true;
            r_GroundNormal = hitInfo.normal;
            // r_Animator.applyRootMotion = true;
        }
        else
        {
            r_IsGrounded = false;
            r_GroundNormal = Vector3.up;
            // r_Animator.applyRootMotion = false;

        }
    }

    public void StopMoving()
    {
        // TODO: should maybe consider checking whether we are moving first

        CheckGroundStatus();

        Vector3 stop = Vector3.zero;

        r_TurnAmount = Mathf.Atan2(stop.x, stop.z);
        r_ForwardAmount = stop.z;
        r_Rigidbody.velocity = new Vector3(0, 0, 0);

        UpdateAnimator(Vector3.zero);    // TODO
    }

    public void Update()
    {
        RecheckCount++;
        if (RecheckCount >= RecheckGroundFrames)
        {
            RecheckCount = 0;
            CheckGroundStatus();
            UpdateAnimator(r_Rigidbody.velocity);
        }
    }

    void UpdateAnimator(Vector3 move)
    {
        // TODO: Animations
        //// update the animator parameters
        float xZMovementMagnitude = new Vector3(move.x, 0f, move.z).magnitude;
        r_Animator.SetFloat("Forward", xZMovementMagnitude, 0.1f, Time.deltaTime);
        r_Animator.SetFloat("Turn", r_TurnAmount, 0.1f, Time.deltaTime);
        r_Animator.SetBool("OnGround", r_IsGrounded);

        if (!r_IsGrounded)
        {
            // we DO want to keep the jump animation since this is really the player falling 
            r_Animator.SetFloat("Jump", r_Rigidbody.velocity.y);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (r_IsGrounded && move.magnitude > 0)
        {
            r_Animator.speed = r_AnimSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            r_Animator.speed = 1;
        }
    }


    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * r_GravityMultiplier) - Physics.gravity;
        r_Rigidbody.AddForce(extraGravityForce);

        // r_GroundCheckDistance = r_Rigidbody.velocity.y < 0 ? r_OrigGroundCheckDistance : 0.01f;
    }


    void HandleGroundedMovement(/*bool crouch, bool jump*/)
    {
        return;
    }

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(r_StationaryTurnSpeed, r_MovingTurnSpeed, r_ForwardAmount);
        transform.Rotate(0, r_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }


    public void OnAnimatorMove()
    {
        // TODO: Animation speed stuff
        //// we implement this function to override the default root motion.
        //// this allows us to modify the positional speed before it's applied.
        if (r_IsGrounded && Time.deltaTime > 0)
        {

            //Vector3 v = (r_Animator.deltaPosition * r_MoveSpeedMultiplier) / Time.deltaTime;  // TODO: Do we get this from the animator?
            Vector3 v = (moveDelta * r_MoveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = r_Rigidbody.velocity.y;
            r_Rigidbody.velocity = v;
        }
    }
}