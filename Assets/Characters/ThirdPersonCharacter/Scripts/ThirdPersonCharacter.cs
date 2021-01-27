using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonCharacter : MonoBehaviour
{
    [SerializeField] float m_MovingTurnSpeed = 360;
    [SerializeField] float m_StationaryTurnSpeed = 180;
    // [SerializeField] float m_JumpPower = 12f;
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
    // [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 0.1f;


    Rigidbody m_Rigidbody;
    Animator m_Animator;
    public bool m_IsGrounded;
    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    CapsuleCollider m_Capsule;
    private LayerMask m_LayerMask;
    private bool inPushingAnim;
    //bool m_Crouching;

    private ItemUI itemUI;
    public bool HasKey;

    public bool lockOnXAxis;
    public bool lockOnZAxis;
    public bool disallowRotation;

    public bool isGrabbingSomething;

    public GameObject grabbedBox;

    private float tMoveSpeed;   // temporary move speed for if player is grabbing something

    public AudioClip[] feetNoises;
    public AudioClip errorSound;

    public AudioClip landingSound;
    private bool waitingToLand;

    AudioSource audios;
    public float FootstepDelay = 0.2f;
    private float footstepcount;

    private float RecheckGroundFrames = 2;  // check for ground every 5 frames
    private float RecheckCount = 0;
    private string pickupName;

    private GameObject errorCanvas;
    private bool errorCooldown;

    StateManager stateManager;
    GravityManager gravityManager;

    void Start()
    {
        waitingToLand = false;
        stateManager = GameObject.FindObjectOfType<StateManager>();
        gravityManager = GameObject.FindObjectOfType<GravityManager>();

        m_Animator = GetComponentInChildren<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        lockOnXAxis = false;
        lockOnZAxis = false;
        disallowRotation = false;

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;

        audios = GetComponent<AudioSource>();
        footstepcount = 0;

        m_LayerMask = ~((1 << 17) | (1 << 9));
        inPushingAnim = false;

        itemUI = GameObject.Find("ItemUI").GetComponent<ItemUI>();
        errorCanvas = GameObject.Find("WarningCanvas");
        errorCanvas.SetActive(false);
        errorCooldown = false;
    }


    public void Move(Vector3 move)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        // if we're starting to grab a box, or grabbing a box, don't allow rotation
        if (!isGrabbingSomething && !disallowRotation)
        {
            m_TurnAmount = Mathf.Atan2(move.x, move.z);
        }
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            footstepcount += move.magnitude;
            if (footstepcount > FootstepDelay)
            {
                PlayFootSound();
                footstepcount = 0;
            }
            HandleGroundedMovement(/*crouch, jump*/);
        }
        else
        {
            // TODO: We CAN'T move when we're in the air so CHANGE this setting!
            HandleAirborneMovement();
        }

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    public void StopMoving()
    {
        // TODO: should maybe consider checking whether we are moving first

        CheckGroundStatus();

        Vector3 stop = Vector3.zero;

        m_TurnAmount = Mathf.Atan2(stop.x, stop.z);
        m_ForwardAmount = stop.z;

        m_Rigidbody.velocity = stop;

        UpdateAnimator(Vector3.zero);
    }


    public void PlayGrabAnim()
    {
        inPushingAnim = true;
        m_Animator.SetBool("PushPull", true);
        m_Animator.Play("Base Layer.Grab");
        m_Animator.Update(0);
    }
    public void StartPushPullAnim()
    {
        // we'll move the player manually so disable root motion
        m_Animator.applyRootMotion = false;
        inPushingAnim = true;
        // let the animator know we're pushing something
        m_Animator.SetBool("PushPull", true);
        m_Animator.Play("Base Layer.PPTree");
        m_Animator.Update(0);
    }

    public void UseWatch()
    {
        m_Animator.Play("Base Layer.Use Watch");
        m_Animator.Update(0);
    }
    public void DoPushPullAnim(float m_amount)
    {
        // Start pushing animation
        m_Animator.SetFloat("Forward", m_amount);
        m_Animator.Update(Time.deltaTime);
        //m_Animator.SetBool("OnGround", m_IsGrounded);
    }

    public void StopPushPullAnim()
    {
        //// End pushing animation
        //// let animator help with movement
        inPushingAnim = false;
        m_Animator.applyRootMotion = true;
        m_Animator.SetBool("PushPull", false);
        m_Animator.Play("Base Layer.Grounded");
        m_Animator.Update(0);
    }

    void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        if (!inPushingAnim)
        {
            // We set manually if we're in push/pull anim
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        }
        m_Animator.SetBool("OnGround", m_IsGrounded);

        if (!m_IsGrounded)
        {
            // we DO want to keep the jump animation since this is really the player falling 
            m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_IsGrounded && move.magnitude > 0)
        {
            m_Animator.speed = m_AnimSpeedMultiplier;
        }
        else if (!m_IsGrounded)
        {
            // don't use that while airborne
            m_Animator.speed = 1;
        }
        else if (gravityManager.isFlipping)
        {
            m_Animator.SetFloat("Forward", 0, 0.1f, Time.deltaTime);
            // If the character isn't moving, don't apply any speed from animation
            m_Animator.speed = 0;
        }
    }


    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);
        // m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
    }


    void HandleGroundedMovement(/*bool crouch, bool jump*/)
    {
        /* anything special that happens on the ground happens here */
        return;
    }

    void ApplyExtraTurnRotation()
    {
        if (isGrabbingSomething)
        {
            // No rotating while grabbing something!
            return;
        }
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }


    public void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (m_IsGrounded && Time.deltaTime > 0 && !inPushingAnim)
        {
            Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
            // we preserve the existing y part of the current velocity.
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }

    public void Update()
    {
        RecheckCount++;
        if (RecheckCount >= RecheckGroundFrames)
        {
            RecheckCount = 0;
            CheckGroundStatus();
            UpdateAnimator(m_Rigidbody.velocity);
        }
    }

    void CheckGroundStatus()
    {
        // increased offset from 0.1f to 0.5f to place origin of raycast further inside the character
        float rayCastOriginOffset = 0.5f;

        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * rayCastOriginOffset), transform.position + (Vector3.up * rayCastOriginOffset) + (Vector3.down * m_GroundCheckDistance));
#endif
        bool hasGround;
        if (stateManager.state == StateManager.State.Looking || gravityManager.isFlipping)
        {
            // here, we need to cast our ray upwards
            hasGround = Physics.SphereCast(transform.position + (Vector3.down * rayCastOriginOffset), 0.2f, Vector3.up, out hitInfo, (rayCastOriginOffset + m_GroundCheckDistance), m_LayerMask);
        }
        else
        {
            hasGround = Physics.SphereCast(transform.position + (Vector3.up * rayCastOriginOffset), 0.2f, Vector3.down, out hitInfo, (rayCastOriginOffset + m_GroundCheckDistance), m_LayerMask);
        }
        // rayCastOriginOffset is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        // if (Physics.SphereCast(transform.position + (Vector3.up * rayCastOriginOffset), 0.2f, Vector3.down, out hitInfo, (rayCastOriginOffset + m_GroundCheckDistance), m_LayerMask))
        if (hasGround)
        {
            if (waitingToLand && !gravityManager.isFlipping)
            {
                // if we're in the air and then landing, PLUS we aren't flipping (there's an extra one of these there)
                // then play a landing sound
                StartCoroutine("PlayLandSound");
            }
            waitingToLand = false;
            m_IsGrounded = true;
            m_GroundNormal = hitInfo.normal;
            m_Animator.applyRootMotion = true;
        }
        else
        {
            waitingToLand = true;
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;

        }
    }

    public void GetItem(string itemname)
    {
        pickupName = itemname;
        HasKey = true;
    }

    private void OnGUI()
    {
        if (HasKey)
        {
            itemUI.AcquireItem(pickupName);
        }
        else
        {
            itemUI.NoItem();
        }
    }

    public void useKey()
    {
        HasKey = false;
    }

    public void FreezeRigidbodyXZPosition()
    {
        m_Rigidbody.constraints = m_Rigidbody.constraints | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        m_Rigidbody.velocity.Set(0f, m_Rigidbody.velocity.y, 0f); // = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    public void UnfreezeRigidbodyXZPosition()
    {
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    IEnumerator PlayLandSound()
    {
        audios.volume = 0.03f + Mathf.Abs(m_Rigidbody.velocity.y / 10);
        audios.clip = landingSound ;
        audios.PlayOneShot(audios.clip);
        yield return new WaitForSeconds(0.15f);
        audios.volume = 0.1f;
    }


    private void PlayFootSound()
    {
        int n = Random.Range(1, feetNoises.Length);
        audios.clip = feetNoises[n];
        audios.PlayOneShot(audios.clip);
        feetNoises[n] = feetNoises[0];
        feetNoises[0] = audios.clip;
    }

    public void DoError(string errMessage)
    {
        if (errorCooldown)
        {
            // If we're in our error cooldown period, just return
            return;
        }
        // set error cooldown
        errorCooldown = true;
        // activate canvas, display message, play sound, and wait for it all to finish up
        errorCanvas.SetActive(true);
        GameObject.Find("WarningText").GetComponent<TextMeshProUGUI>().SetText(errMessage);
        PlayErrorSound();
        StartCoroutine("FinishError");
    }

    public void PlayErrorSound()
    {
        audios.clip = errorSound;
        audios.PlayOneShot(audios.clip);
    }

    IEnumerator FinishError()
    {
        // wait one second, then hide canvas and clear cooldown flag
        yield return new WaitForSeconds(1f);
        errorCanvas.SetActive(false);
        errorCooldown = false;
    }
}


