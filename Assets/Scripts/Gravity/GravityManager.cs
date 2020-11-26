using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GravityManager : MonoBehaviour
{
    public bool usePostProcessingEffects = true;

    StateManager stateManager;
    private GameObject player;
    ThirdPersonUserControl thirdPersonUserControl;
    private GameObject robot;
    private GameObject flippable;
    private FlipEvents flipEvents;
    PostProcessVolume postProcessVolume;
    Vector3 savedGravity = Physics.gravity;
    private Rigidbody[] allRigidbodies;
    private NoFlipZone[] noFlipZones;

    private float cooldownTime = 1.5f;
    private bool isFlipping = false;

    [HideInInspector]
    public Animator flippableAnimator;
    public Animator lookUpFadeAnimator;
    Projector playerLookUpProjector;
    Projector robotLookUpProjector;
    private bool looking = false;

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
        robot = GameObject.FindGameObjectWithTag("robot");
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
    }

    void Update()
    {
        StateManager.State state = stateManager.GetState();
        bool readyToFlip = stateManager.CheckReadyToFlip();

        /* Handle flips. */
        if (state == StateManager.State.Normal && Input.GetButtonDown("FlipGrav") && readyToFlip && haveGravWatch)
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
        if (state == StateManager.State.Normal && readyToFlip && Input.GetButtonDown("LookUp"))
        {
            lookUpFadeAnimator.SetTrigger("LookUp");
        }
        else if (state == StateManager.State.Looking && readyToFlip && Input.GetButtonUp("LookUp"))
        {
            lookUpFadeAnimator.ResetTrigger("LookUp");
            lookUpFadeAnimator.SetTrigger("StopLooking");
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

    // TODO: Have state Looking stop all inputs except those related to movement.
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
            stateManager.SetState(StateManager.State.Normal);
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
    }

    void FlipGravity()
    {
        robot.GetComponent<RobotBuddy>().StopMoving();

        stateManager.SetReadyToFlip(false);
        stateManager.ToggleGravityOrientation();

        audioSource.PlayOneShot(flipSound);

        StartCoroutine(FlipLevel());
        if (usePostProcessingEffects)
            StartCoroutine(PostProcessingEffects());
    }

    IEnumerator FlipLevel()
    {
        stateManager.SetState(StateManager.State.Flipping);
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.isKinematic = true;
        }
        isFlipping = true;
        string trigger = stateManager.IsGravityFlipped() ? "Flip1" : "Flip2";
        Vector3 savedPlayerHeading = player.transform.forward;
        Vector3 savedRobotHeading = robot.transform.forward;

        flippableAnimator.SetTrigger(trigger);
        Physics.gravity = Vector3.zero;
        yield return new WaitWhile(() => isFlipping); // Waiting on flip animation
        Physics.gravity = savedGravity;
        // once we're done rotating, make us kinematic again
        foreach (Rigidbody body in allRigidbodies)
        {
            body.isKinematic = false;
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
        flippableAnimator.ResetTrigger(trigger);
    }

    public void SetFlipping(bool value)
    {
        isFlipping = value;
    }


    IEnumerator FlipCooldownTimer()
    {
        yield return new WaitForSeconds(cooldownTime);
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

