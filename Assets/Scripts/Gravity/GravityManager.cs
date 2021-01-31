using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GravityManager : MonoBehaviour
{
    public bool usePostProcessingEffects = true;

    StateManager stateManager;
    private GameObject player;
    ThirdPersonUserControl thirdPersonUserControl;
    ThirdPersonCharacter playerChar;
    Rigidbody playerRb;
    Rigidbody robotRb;
    private GameObject robot;
    RobotBuddy robotChar;
    private GameObject flippable;
    private FlipEvents flipEvents;
    PostProcessVolume postProcessVolume;
    Vector3 savedGravity;
    private Rigidbody[] allRigidbodies;
    private NoFlipZone[] noFlipZones;

    private float cooldownTime = 0.75f;
    private float currentCooldownTime = 0.0f;
    public bool isFlipping = false;

    [HideInInspector]
    public Animator flippableAnimator;
    public Animator lookUpFadeAnimator;
    Projector playerLookUpProjector;
    Projector robotLookUpProjector;
    private bool looking = false;
    StateManager.State postLookState = StateManager.State.Normal;

    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip cantFlipSound;

    GameObject noFlipUI;
    CanvasGroup noFlipCanvasGroup;
    Animation noFlipAnimation;

    [HideInInspector]
    public bool haveGravWatch = true;

    void Awake()
    {
        stateManager = GameObject.FindObjectOfType<StateManager>();
    }

    void Start()
    {
        // Gravity y must always be DOWNWARD on scene start.
        if (Physics.gravity.y > 0.0f)
        {
            Physics.gravity = new Vector3(Physics.gravity.x, -1f * Physics.gravity.y, Physics.gravity.z);
        }
        savedGravity = Physics.gravity;

        // Get the post-processing volume component.
        postProcessVolume = GetComponent<PostProcessVolume>();

        // Get flippable animator.
        flippableAnimator = GameObject.FindWithTag("Flippable").GetComponent<Animator>();

        noFlipUI = GameObject.Find("NoFlipCanvas");
        noFlipCanvasGroup = noFlipUI.GetComponent<CanvasGroup>();
        noFlipAnimation = noFlipUI.GetComponent<Animation>();
        noFlipUI.SetActive(false);

        // Get player, bot, and flippable level content
        player = GameObject.FindGameObjectWithTag("Player");
        thirdPersonUserControl = player.GetComponent<ThirdPersonUserControl>();
        playerChar = player.GetComponent<ThirdPersonCharacter>();
        playerRb = playerChar.GetComponent<Rigidbody>();
        robot = GameObject.FindGameObjectWithTag("robot");
        robotRb = robot.GetComponent<Rigidbody>();
        robotChar = robot.GetComponent<RobotBuddy>();
        flippable = GameObject.FindGameObjectWithTag("Flippable");
        flipEvents = GameObject.FindObjectOfType<FlipEvents>();
        audioSource = GetComponent<AudioSource>();
        playerLookUpProjector = GameObject.Find("PlayerLookUpProjector").GetComponent<Projector>();
        playerLookUpProjector.enabled = false;
        robotLookUpProjector = GameObject.Find("RobotLookUpProjector").GetComponent<Projector>();
        robotLookUpProjector.enabled = false;

        // Collect all the rigidbodies in the scene now for setting kinematic later.
        allRigidbodies = Rigidbody.FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[];

        // Collect all the no-flip-zones in the scene now for checking later.
        noFlipZones = NoFlipZone.FindObjectsOfType(typeof(NoFlipZone)) as NoFlipZone[];

        // Reset gravity when level resets, just in case things get jank
        Physics.gravity = new Vector3(0, -9.8f, 0);

    }

    void Update()
    {
        // don't allow flipping or looking up while player is holding a crate
        if (playerChar.isGrabbingSomething)
        {
            return;
        }
        StateManager.State state = stateManager.GetState();
        bool readyToFlip = stateManager.CheckReadyToFlip();
        //bool readyToFlip = robotChar.r_IsGrounded && playerChar.m_IsGrounded;

        /* Handle flips. */
        if (state == StateManager.State.Normal && Input.GetButtonDown("FlipGrav") && readyToFlip && haveGravWatch && !stateManager.IsPaused())
        {
            StopCoroutine("NoFlipAnimationSound");

            foreach (NoFlipZone noFlipZone in noFlipZones)
            {
                if (noFlipZone.characterInZone)
                {
                    StartCoroutine("NoFlipAnimationSound");
                    return;
                }
            }

            FlipGravity();
        }

        /* Handle looking up. (Y button or L key) */
        if (state == StateManager.State.Normal && readyToFlip && Input.GetButtonDown("LookUp") && !playerChar.isGrabbingSomething && !stateManager.IsPaused())
        {
            looking = true;
            lookUpFadeAnimator.ResetTrigger("StopLooking");
            lookUpFadeAnimator.SetTrigger("LookUp");
        }
        if (looking && readyToFlip && Input.GetButtonUp("LookUp"))
        {
            looking = false;
            lookUpFadeAnimator.ResetTrigger("LookUp");
            lookUpFadeAnimator.SetTrigger("StopLooking");
            postLookState = StateManager.State.Normal;
        }

        // ********************************************************************
        // Effects on/off for presentation only. Remove later.
        // ********************************************************************
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            usePostProcessingEffects = !usePostProcessingEffects;
        }

    }

    public void StopLookingOnPickup(string pickupType)
    {
        if (looking)
        {
            looking = false;
            lookUpFadeAnimator.ResetTrigger("LookUp");
            lookUpFadeAnimator.SetTrigger("StopLooking");

            switch (pickupType.ToLower())
            {
                case "tape":
                    postLookState = StateManager.State.Dialog;
                    break;

                case "gravitywatch":
                case "lookuptutorial":
                    postLookState = StateManager.State.Inert;
                    break;

                case "robotactivator":
                    postLookState = StateManager.State.Inert;
                    break;

                default:
                    postLookState = StateManager.State.Normal;
                    break;
            }
        }
    }

    IEnumerator NoFlipAnimationSound()
    {
        noFlipUI.SetActive(true);
        noFlipAnimation.Play();
        audioSource.Stop();
        audioSource.PlayOneShot(cantFlipSound, 0.75f);

        bool playerInSomeZone = false;
        bool robotInSomeZone = false;
        foreach (NoFlipZone noFlipZone in noFlipZones)
        {
            if (noFlipZone.playerInZone)
                playerInSomeZone = true;
            if (noFlipZone.robotInZone)
                robotInSomeZone = true;
        }

        if (stateManager.GetSelected().tag == "Player")
        {
            if (playerInSomeZone) { }
            // Do nothing
            else if (robotInSomeZone)
                thirdPersonUserControl.SwitchChar();
        }
        else
        {
            if (robotInSomeZone) { }
            // Do nothing
            else if (playerInSomeZone)
                thirdPersonUserControl.SwitchChar();
        }

        yield return new WaitForSeconds(1f);
        noFlipAnimation.Stop();
        noFlipUI.SetActive(false);
    }

    public void LookUp(bool engaged)
    {
        // Player setup for look-flip.
        Vector3 savedPlayerHeading = player.transform.forward;
        Quaternion playerStartRot = player.transform.localRotation;
        Quaternion playerEndRot = playerStartRot * Quaternion.Euler(180, 180, 0);
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<CapsuleCollider>().enabled = false;

        // Condition-specific quantities: engaging the look-up, or disengaging from it.
        stateManager.ToggleGravityOrientation();
        if (engaged)
        {
            stateManager.SetState(StateManager.State.Looking);
            playerLookUpProjector.enabled = true;
            robotLookUpProjector.enabled = true;
            Physics.gravity = -savedGravity;
        }
        else
        {
            stateManager.SetState(postLookState);
            playerLookUpProjector.enabled = false;
            robotLookUpProjector.enabled = false;
            Physics.gravity = savedGravity;
            lookUpFadeAnimator.ResetTrigger("StopLooking");
        }

        // Look-flip: a -1 scale of the y-axis to get the expected chirality.
        // This breaks the player on its own, which is why we set up & reset
        // her before and after the flip.
        flippable.transform.localScale = new Vector3(
            flippable.transform.localScale.x,
            -flippable.transform.localScale.y,
            flippable.transform.localScale.z
        );

        // Player reset scale and position.
        Vector3 playerEndPos = player.transform.position;
        player.transform.localScale = new Vector3(
            player.transform.localScale.x,
            -player.transform.localScale.y,
            player.transform.localScale.z
        );
        player.transform.position = playerEndPos;
        player.transform.localRotation = playerEndRot;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.GetComponent<CapsuleCollider>().enabled = true;

        // Reset camera position (skip the lerps)
        CameraControl.CC.SetDefaultPositionRotation();
    }

    void FlipGravity()
    {
        robot.GetComponent<RobotBuddy>().StopMoving();

        stateManager.SetReadyToFlip(false);
        stateManager.ToggleGravityOrientation();

        audioSource.PlayOneShot(flipSound);
        StartCoroutine("PlayerUseWatch");
        StartCoroutine(FlipLevel());
        if (usePostProcessingEffects)
            StartCoroutine(PostProcessingEffects());
    }

    IEnumerator PlayerUseWatch()
    {
        playerChar.UseWatch();
        yield return new WaitForSeconds(0.5f);
        playerRb.isKinematic = true;
    }

    IEnumerator FlipLevel()
    {
        stateManager.SetState(StateManager.State.Flipping);
        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb == playerRb) continue;   // set manually above
            rb.isKinematic = true;
        }
        //playerChar.UseWatch();
        robotChar.ClearQ();
        isFlipping = true;
        string trigger = stateManager.IsGravityFlipped() ? "Flip1" : "Flip2";
        Vector3 savedPlayerHeading = player.transform.forward;
        Vector3 savedRobotHeading = robot.transform.forward;

        flippableAnimator.SetTrigger(trigger);
        Physics.gravity = Vector3.zero;
        yield return new WaitWhile(() => isFlipping); // Waiting on flip animation
        Physics.gravity = savedGravity;
        // once we're done rotating, make us kinematic again
        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb != playerRb || rb != robotRb)
                rb.isKinematic = false;
        }
        robot.GetComponent<RobotBuddy>().PlayGravAnimation();
        stateManager.SetState(StateManager.State.Normal);

        StartCoroutine(FlipCooldownTimer());

        // Reorient player & robot rotation while they fall to the "new" ground.
        yield return new WaitForSeconds(Mathf.Clamp(.5f - Time.deltaTime, 0f, .5f));
        Quaternion playerStartRot = player.transform.localRotation;
        Quaternion playerEndRot = playerStartRot * Quaternion.Euler(180, 0, 0);
        Quaternion robotStartRot = robot.transform.localRotation;
        Quaternion robotEndRot = robotStartRot * Quaternion.Euler(180, 0, 0);
        float elapsed = 0f;
        float time = 0.25f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            t = t * t * (3f - 2f * t);
            player.transform.localRotation = Quaternion.Slerp(playerStartRot, playerEndRot, t);
            robot.transform.localRotation = Quaternion.Slerp(robotStartRot, robotEndRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.forward = savedPlayerHeading;
        robot.transform.forward = savedRobotHeading;
        playerRb.isKinematic = false;
        robotRb.isKinematic = false;
        // now we figure out where the player's position is in relation to
        // the new orientation of the level
        robotChar.FindPlayer();

        flippableAnimator.ResetTrigger(trigger);
    }

    public void SetFlipping(bool value)
    {
        isFlipping = value;
        stateManager.setIsGravityFlipping(value);
    }


    IEnumerator FlipCooldownTimer()
    {
        currentCooldownTime = 0;
        while (!(robotChar.r_IsGrounded && playerChar.m_IsGrounded) && (currentCooldownTime < cooldownTime))
        {
            currentCooldownTime += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        stateManager.SetReadyToFlip(true);
    }

    IEnumerator PostProcessingEffects()
    {
        // Set up chromatic aberration.
        ChromaticAberration chromaticAberration = null;
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        chromaticAberration.active = true;
        chromaticAberration.intensity.value = 0f;

        // // Set up lens distortion.
        LensDistortion lensDistortion = null;
        postProcessVolume.profile.TryGetSettings(out lensDistortion);
        lensDistortion.active = true;
        lensDistortion.intensity.value = 0f;

        // // Set up color grading.
        ColorGrading colorGrading = null;
        postProcessVolume.profile.TryGetSettings(out colorGrading);
        colorGrading.active = true;
        colorGrading.saturation.value = 0f;
        colorGrading.postExposure.value = 0f;

        // Apply effects over the course of the level rotation.
        while (isFlipping)
        {
            float scale = flipEvents.fxScale;
            chromaticAberration.intensity.value = scale * 1f;
            lensDistortion.intensity.value = -scale * 100f;
            colorGrading.saturation.value = scale * 100f;
            if (0.75 <= scale && scale <= 1)
                colorGrading.postExposure.value = ((scale - 0.75f) / 0.25f) * 5f;
            yield return null;
        }

        chromaticAberration.active = false;
        lensDistortion.active = false;
        colorGrading.active = false;
    }


}

