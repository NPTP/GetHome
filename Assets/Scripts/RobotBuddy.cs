using JetBrains.Annotations;
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

    private ThirdPersonCharacter playerThirdPersonCharacter;

    public GameObject following;
    public GameObject sibling;

    public bool used = false;
    public float speed = 3f;

    Rigidbody r_Rigidbody;
    // Animator r_Animator;
    public bool r_IsGrounded;
    float r_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float r_TurnAmount;
    float r_ForwardAmount;
    Vector3 r_GroundNormal;
    //float r_CapsuleHeight;
    //Vector3 r_CapsuleCenter;
    // BoxCollider r_Collider;
    Vector3 moveDelta;

    // Start is called before the first frame update
    void Start()
    {
        // controller = GetComponent<CharacterController>();
        stateManager = GameObject.FindObjectOfType<StateManager>();

        playerThirdPersonCharacter = following.GetComponent<ThirdPersonCharacter>();
        r_Rigidbody = GetComponent<Rigidbody>();

        // r_Animator = GetComponent<Animator>();


        r_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        r_OrigGroundCheckDistance = r_GroundCheckDistance;
        r_IsGrounded = true;    // start the robot grounded
        // breakranks();
    }

    public void WarpToPlayer()
    {
        // TODO: When we get the command, warp this robot to be where the player is!

        // fade out char - coroutine and alpha?

        // move this gameobject - transform.translate? or just change transform.position = Vector3?

        // fade in char - coroutine and alpha?
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // do moving of the char here?
        //Debug.Log(r_Rigidbody.velocity);
        // Only allow movement when not gravity-flipping (even gravity is not applied during flip).
        StateManager.State state = stateManager.GetState();
        if (state == StateManager.State.Normal || state == StateManager.State.Looking)
        {
            // TODO: clean this condition up!
            if (!used && r_IsGrounded && playerThirdPersonCharacter.m_IsGrounded && stateManager.CheckReadyToFlip())
            {

                // if we've already been used we stay where we are
                Vector3 curpos = transform.position;                        // robot position
                Vector3 target = following.transform.position;              // player (or other target?) position
                Vector3 moveamount = (target - curpos).normalized * speed;  // limit our movement amount by our speed
                float distToPlayer = (target - curpos).magnitude;
                moveDelta = new Vector3(moveamount.x / 20, 0, moveamount.z / 20);
                if (distToPlayer > 2.5f) // keep away from the player, don't crowd them!
                                         // we should also make this so that if the player is trying to back into the robot
                                         // the robot moves away?
                {
                    Move(moveamount * (distToPlayer * 0.2f));
                }
                if (distToPlayer < 1.5f)
                {
                    Move(-moveamount * 0.1f);
                }
            }
            // Move(Physics.gravity);  // always apply gravity!

        }
        else
        {
            r_Rigidbody.velocity = Vector3.zero;
            r_Rigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void Move(Vector3 move/*, bool crouch, bool jump*/)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        //if (!used && move.magnitude > 0.2f) // if we're moving and haven't broken ranks before, do it!
        //{
        //    breakranks();
        //}
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, r_GroundNormal);
        r_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (r_IsGrounded)
        {
            HandleGroundedMovement(/*crouch, jump*/);
        }
        else
        {
            // TODO: We CAN'T move when we're in the air so CHANGE this setting!
            HandleAirborneMovement();
        }

        // send input and other state parameters to the animator
        // UpdateAnimator(move);    // TODO

        // we preserve the existing y part of the current velocity.
        move *= speed;
        move.y = r_Rigidbody.velocity.y;
        r_Rigidbody.velocity = move;
    }

    public GameObject getSibling()
    {
        if (sibling)
            return sibling;
        return null;
    }

    public void setFollowing(GameObject tofollow)
    {
        following = tofollow;
    }

    public void breakranks()
    {
        used = true;
        if (sibling)
        {
            sibling.GetComponent<RobotBuddy>().setFollowing(following);
        }
    }

    public void unbreakranks()
    {
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
        if (Physics.Raycast(transform.position - new Vector3(0, 0.1f, 0), Vector3.down, out hitInfo, r_GroundCheckDistance))
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

        // UpdateAnimator(Vector3.zero);    // TODO
    }

    void UpdateAnimator(Vector3 move)
    {
        // TODO: Animations
        //// update the animator parameters
        //r_Animator.SetFloat("Forward", r_ForwardAmount, 0.1f, Time.deltaTime);
        //r_Animator.SetFloat("Turn", r_TurnAmount, 0.1f, Time.deltaTime);
        //r_Animator.SetBool("OnGround", r_IsGrounded);

        //if (!r_IsGrounded)
        //{
        //    // we DO want to keep the jump animation since this is really the player falling 
        //    r_Animator.SetFloat("Jump", r_Rigidbody.velocity.y);
        //}

        //// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        //// which affects the movement speed because of the root motion.
        //if (r_IsGrounded && move.magnitude > 0)
        //{
        //    r_Animator.speed = r_AnimSpeedMultiplier;
        //}
        //else
        //{
        //    // don't use that while airborne
        //    r_Animator.speed = 1;
        //}
    }


    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * r_GravityMultiplier) - Physics.gravity;
        r_Rigidbody.AddForce(extraGravityForce);

        r_GroundCheckDistance = r_Rigidbody.velocity.y < 0 ? r_OrigGroundCheckDistance : 0.01f;
    }


    void HandleGroundedMovement(/*bool crouch, bool jump*/)
    {
        return;
    }

    void ApplyExtraTurnRotation()
    {

        //// based on: https://answers.unity.com/questions/952747/how-do-i-make-a-character-walk-backwards-rather-th.html
        //// if player wants to go backwards, convert move to backward walk with a corresponding turn
        //// m_TurnAmount =~ PI if pure back
        //// m_TurnAmount =~ PI*0.75 if back and turn
        //if (Mathf.Abs(r_TurnAmount) > Mathf.PI * 0.5f && r_ForwardAmount < -1e-4f)
        //{
        //    r_ForwardAmount = -0.5f; // back
        //    if (Mathf.Abs(r_TurnAmount) < Mathf.PI * 0.9f) r_TurnAmount = 0.5f * Mathf.Sign(r_TurnAmount); // turn by 0.5 radians
        //    else r_TurnAmount = 0f; // pure back
        //}
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(r_StationaryTurnSpeed, r_MovingTurnSpeed, r_ForwardAmount);
        transform.Rotate(0, r_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }


    public void OnAnimatorMove()    // this won't be called since we don't have an animator??
    {
        // TODO: Animation speed stuff
        //// we implement this function to override the default root motion.
        //// this allows us to modify the positional speed before it's applied.
        if (r_IsGrounded && Time.deltaTime > 0)
        {

            //Vector3 v = (r_Animator.deltaPosition * r_MoveSpeedMultiplier) / Time.deltaTime;
            Vector3 v = (moveDelta * r_MoveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = r_Rigidbody.velocity.y;
            r_Rigidbody.velocity = v;
        }
    }
}