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

    public float PlayerHeadTowardsMaxDistance = 2.5f;   // If we're more than this units far away, head towards the player
    public float PlayerAvoidMinDistance = 0.5f;         // if we're less than this many units close, back away from player

    private ThirdPersonCharacter playerThirdPersonCharacter;

    public GameObject following;
    public GameObject roboBody;
    public GameObject roboSpotlight;
    private LayerMask r_LayerMask;

    public GameObject warpPrefab;

    public bool used = false;
    private bool waitingToLand = false;
    public float speed = 5f;

    Rigidbody r_Rigidbody;
    Animator r_Animator;
    public bool r_IsGrounded;
    float r_TurnAmount;
    float r_ForwardAmount;
    Vector3 r_GroundNormal;

    private AudioSource footsounds;
    private AudioSource warpsound;
    private bool footstepsplaying = false;

    [Space]
    [Header("Speech sounds")]
    public AudioSource speechSource;
    public AudioClip selectSound;
    public AudioClip deselectSound;
    public AudioClip interactSound;
    public AudioClip followSound;

    public AudioClip landingSound;
    public AudioClip scuttlingSound;

    private Light thisLight;

    private float RecheckGroundFrames = 5;  // check for ground every 5 frames
    private float RecheckCount = 0;

    private ParticleSystem sparks;

    [Space]
    public Vector3 spotlightDirection = new Vector3(0, 1.5f, 0);

    private float timeSinceLastSpark;

    // Player position tracking
    private Queue<Vector3> playerPos;
    public int playerPosUpdateFrames = 5;
    private int curPlayerPosUpdateFrame = 0;
    public int robotPosUpdateFrames = 7;
    private int robotPosCurrentFrame = 0;
    private Vector3 lastPos;
    private Vector3? currentRobotTarget = null;
    private int maxPlayerPos = 75;


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
        speechSource = GetComponents<AudioSource>()[2];

        thisLight = GetComponent<Light>();

        r_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        r_IsGrounded = true;    // start the robot grounded

        //r_LayerMask = ~((1 << 17) | (1 << 9));
        r_LayerMask = ~(1 << 17 | 1 << 11 | 1 << 12 | 1 << 9 | 1 << 10);    // don't collide with occlusion volumes, triggers, or NoFlip Zones
        playerPos = new Queue<Vector3>();
    }

    public void PlaySpeechSound(string soundName)
    {
        AudioClip sound;
        switch (soundName.ToLower())
        {
            case "select":
                sound = selectSound;
                break;
            case "deselect":
                sound = deselectSound;
                break;
            case "interact":
                sound = interactSound;
                break;
            case "follow":
                sound = followSound;
                break;
            default:
                sound = selectSound;
                break;
        }
        speechSource.clip = sound;
        speechSource.pitch = Random.Range(0.9f, 1.1f);  // Add variety to the sounds
        speechSource.Play();                            // We WANT single-channel audio here so we don't have overlapping bot speech sounds.
    }

    public void SelectJuice()
    {
        SelectedLightSound(true);
    }

    public void DeselectJuice()
    {
        SelectedLightSound(false);
    }

    private void SelectedLightSound(bool isSelected)
    {
        if (isSelected)
        {
            PlaySpeechSound("select");
            thisLight.color = Color.green;
        }
        else
        {
            PlaySpeechSound("deselect");
            thisLight.color = Color.red;
        }
    }

    public void ClearQ()
    {
        currentRobotTarget = null;
        // StopMoving();
        playerPos.Clear();  // we need to start a new player position queue after warping;
    }

    public void FindPlayer()
    {
        // Where is the player!
        Vector3 playpos = playerThirdPersonCharacter.transform.position;
        Transform playert = playerThirdPersonCharacter.transform;
        // check behind the player first since that's the most natural place to put the robot, then beside, then in front only in a pickler
        Vector3[] offsets = { playpos - (playert.forward * 2),
            playpos + (playert.right * 2),
            playpos - (playert.right * 2),
            playpos + (playert.forward * 2) };
        Vector3 newTarg = new Vector3(0, 0, 0);
        bool foundSpot = false;
        foreach (Vector3 checkLoc in offsets)
        {
            if (!Physics.CheckSphere(checkLoc, 0.2f, r_LayerMask))
            {
                newTarg = checkLoc;
                foundSpot = true;
                break;
            }
        }
        if (!foundSpot)
        {

            currentRobotTarget = playpos;
        }
        else
        {

            currentRobotTarget = newTarg;
        }
    }

    public void WarpToPlayer(Vector3 warpTo)
    {
        Vector3 warpFrom = transform.position;
        StartCoroutine(WarpEffect(warpTo));
        warpsound.Play();
        ClearQ();
        StopMoving();
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
        StateManager.State state = stateManager.GetState();
        // if we're in tape dialog, don't do anything
        if (state == StateManager.State.Dialog)
        {
            StopMoving();
            return;
        }

        // orient the robot towards the player
        if (stateManager.CheckReadyToFlip())
        {
            Vector3 newForwardLook = new Vector3(roboSpotlight.transform.forward.x, 0, roboSpotlight.transform.forward.z);
            if ((newForwardLook.sqrMagnitude > 0.01f))
            {
                roboBody.transform.forward = newForwardLook;
            }
        }

        // If the robot is the selected game object, we don't need to do any automated movement, so break here
        if (stateManager.GetSelected() == this.gameObject)
        {
            return;
        }

        // Handle random robot sparks
        timeSinceLastSpark += Time.deltaTime;
        if (timeSinceLastSpark > 10 && Random.Range(0, 100) == 1)
        {
            StartCoroutine("SparkEffect");
            timeSinceLastSpark = 0;
        }

        // Check if conditions for automatic robot movement is correct.
        // Only allow movement when not gravity-flipping (even gravity is not applied during flip).
        bool moveRobot = false;
        if (state == StateManager.State.Normal || state == StateManager.State.Looking)
        {
            if (r_IsGrounded && playerThirdPersonCharacter.m_IsGrounded && stateManager.CheckReadyToFlip() && !used)
            {
                                moveRobot = true;
            }
        }

        // If we're gonna move the robot, now is the time to do it!
        if (moveRobot)
        {
            Vector3 curPlayerPos = playerThirdPersonCharacter.transform.position;

            // update our player frame counter
            curPlayerPosUpdateFrame++;
            if (curPlayerPosUpdateFrame > playerPosUpdateFrames)
            {
                curPlayerPosUpdateFrame = 0;
                // if the player has moved, add a new position

                if ((lastPos != null) && (curPlayerPos - lastPos).sqrMagnitude > 0.0005f)
                {
                    playerPos.Enqueue(curPlayerPos);
                    if (playerPos.Count > maxPlayerPos) // limit the number of saved positions
                    {
                        playerPos.Dequeue();
                    }
                }
                // if we're really close to the player, just make them our target, minus a bit so we don't bump into them!
                if (((curPlayerPos - transform.position).magnitude) < 2)
                {
                    while (playerPos.Count > 3)
                    {
                        // trim our queue down to three elements and grab the last item
                        currentRobotTarget = playerPos.Dequeue();
                    }
                }
                lastPos = curPlayerPos;
            }

            // If we get really close to our last target, grab a new one
            bool forceGetNewTarget = false;
            if (currentRobotTarget != null)
            {
                forceGetNewTarget = (((Vector3)currentRobotTarget - transform.position).magnitude < 2.0f);
            }

            // playerPos.Count > 3 so we don't get too close to the player ever
            if (playerPos.Count > 3 && (forceGetNewTarget || (robotPosCurrentFrame > robotPosUpdateFrames)))
            {
                robotPosCurrentFrame = 0;
                currentRobotTarget = playerPos.Dequeue();
            }

            robotPosCurrentFrame++;
            Vector3 moveamount = Vector3.zero;
            Vector3 curpos = transform.position;                            // robot position

            if (r_IsGrounded && (curPlayerPos - curpos).magnitude < 2)
            {
                // if we're very close to the player, move us away from the player
                moveamount = -((curPlayerPos - curpos).normalized * speed);
                ClearQ();
            }
            else if (currentRobotTarget != null)
            {
                Vector3 targetPos = (Vector3)currentRobotTarget;
                curpos.y = 0;
                targetPos.y = 0;
                moveamount = targetPos - curpos;
                // TODO: check if moving would bump us into something and don't do it if it would?
                if (playerPos.Count == 3) moveamount /= 2;
                
            }
            // Make our move
            Move(moveamount);
        }




        // This checks if we have been left used on a slope and stops the robot from sliding down it
        if ((stateManager.GetSelected() != this.gameObject) 
            && r_IsGrounded && used 
            && (Mathf.Abs(r_GroundNormal.x) > 0.1f || Mathf.Abs(r_GroundNormal.z) > 0.1f))
        {
            Move(Vector3.zero);
        }

        UpdateAnimator(r_Rigidbody.velocity);



        // control and velocity handling is different when grounded and airborne:
        if (r_IsGrounded)
        {
            HandleGroundedMovement();
        }
        else
        {
            // TODO: What should happen here?
            HandleAirborneMovement();
        }
    }

    public void Move(Vector3 move)
    {
        CheckGroundStatus();
        if (!r_IsGrounded)
        {
            // Don't apply ANY movement if we're airborne!
            return;
        }
        float old_y = r_Rigidbody.velocity.y;

        // Need to make sure we don't start sliding down a slope if we've been left on one
        // by the player


        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (used)
        {
            spotlightDirection = move.normalized;
        }
        else
        {
            // always look at player if we aren't used
            spotlightDirection = (playerThirdPersonCharacter.transform.position - transform.position).normalized;
        }
        Vector3 movedupe = move;
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        move = Vector3.ProjectOnPlane(move, r_GroundNormal);
        r_ForwardAmount = move.z;

        ApplyExtraTurnRotation();
        
        // we preserve the existing y part of the current velocity.
        move *= speed;
        move.y = old_y;
        r_Rigidbody.velocity = move;
        // r_Rigidbody.AddForce(move, ForceMode.VelocityChange);

        // send input and other state parameters to the animator
        UpdateAnimator(move);

        movedupe.y = 0;
        if (!footstepsplaying && r_IsGrounded && movedupe.magnitude >= 0.3f)
        {
            footstepsplaying = true;
            footsounds.Play();
        }
        else if (!r_IsGrounded || movedupe.magnitude < 0.05f)   // if we stopped moving / aren't moving lots?
        {
            StopFootsounds();
        }
    }

    IEnumerator PlayLandSound()
    {
        footsounds.volume = 0.04f + Mathf.Abs(r_Rigidbody.velocity.y / 10);
        footsounds.clip = landingSound;
        footsounds.PlayOneShot(footsounds.clip);
        yield return new WaitForSeconds(0.2f);
        footsounds.volume = 0.2f;
        footsounds.clip = scuttlingSound;
    }

    public void StopFootsounds()
    {
        footsounds.Stop();
        footstepsplaying = false;
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
        PlaySpeechSound("Follow");
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
        int flip = stateManager.state == StateManager.State.Looking ? -1 : 1;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0) * flip, Vector3.down * flip, out hitInfo, r_GroundCheckDistance, r_LayerMask))
        {
            if (waitingToLand)
            {
                StartCoroutine("PlayLandSound");
            }
            r_IsGrounded = true;
            r_GroundNormal = hitInfo.normal;
            // r_Animator.applyRootMotion = true;
            waitingToLand = false;
        }
        else
        {
            waitingToLand = true;
            r_IsGrounded = false;
            r_GroundNormal = Vector3.up;
            // r_Animator.applyRootMotion = false;
        }
    }

    public void StopMoving()
    {
        // Update ground status
        CheckGroundStatus();
        // Set velocity, etc. to zero
        Vector3 stop = Vector3.zero;
        r_TurnAmount = Mathf.Atan2(stop.x, stop.z);
        r_ForwardAmount = stop.z;
        r_Rigidbody.velocity = new Vector3(0, 0, 0);
        // Kill footstep sounds
        StopFootsounds();
        // Update animator
        UpdateAnimator(r_Rigidbody.velocity);
        // Make sure forward velocity of animator is set to 0
        r_Animator.SetFloat("Forward", 0);

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
        // update the animator parameters
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
        r_Rigidbody.velocity += extraGravityForce * Time.fixedDeltaTime;
    }


    void HandleGroundedMovement()
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
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (r_IsGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (r_Animator.deltaPosition * r_MoveSpeedMultiplier) / Time.deltaTime;
            /*
            // we preserve the existing y part of the current velocity.
            v.y = r_Rigidbody.velocity.y;
            r_Rigidbody.velocity = v;
            */
        }
    }
}