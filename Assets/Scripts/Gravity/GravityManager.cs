using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GravityManager : MonoBehaviour
{
    private GameObject target;
    private ThirdPersonUserControl thirdPersonUserControl;
    PostProcessVolume postProcessVolume;

    Vector3 savedGravity = Physics.gravity;
    public float cooldownTime = 2f;
    public int degreesPerRotationStep = 1; // Must be a strict integer multiple of 90.
    int characterDegreesPerRotationStep = 10;
    public float rotationStepTime = 0.000001f;
    public bool usePostProcessingEffects = true;
    public bool isFlipping = false;
    public bool readyToFlip = true;

    private GameObject player;
    private GameObject robot;
    private GameObject flippable;
    private FlipEvents flipEvents;

    // Public vars for other classes to check on progress.
    public bool isGravityFlipped = false;
    [HideInInspector] public int degreesRotated = 0;
    public Animator flippableAnimator;
    public Animator lookUpFadeAnimator;

    Projector playerLookUpProjector;
    Projector robotLookUpProjector;
    bool allowInput = true;
    bool looking = false;

    private Rigidbody[] allRigidbodies;

    void Start()
    {
        // Warning if you've calibrated the rotation steps badly.
        if (90 % degreesPerRotationStep != 0)
            Debug.Log("WARNING! Level rotation degreesPerRotationStep is not a multiple of 90. Level rotation could break.");

        // Get the post-processing volume component.
        postProcessVolume = GetComponent<PostProcessVolume>();

        // Get player, bot, and flippable level content
        player = GameObject.FindGameObjectWithTag("Player");
        thirdPersonUserControl = player.GetComponent<ThirdPersonUserControl>();
        robot = GameObject.FindGameObjectWithTag("robot");
        flippable = GameObject.FindGameObjectWithTag("Flippable");
        flipEvents = GameObject.FindObjectOfType<FlipEvents>();
        playerLookUpProjector = GameObject.Find("PlayerLookUpProjector").GetComponent<Projector>();
        playerLookUpProjector.enabled = false;
        robotLookUpProjector = GameObject.Find("RobotLookUpProjector").GetComponent<Projector>();
        robotLookUpProjector.enabled = false;

        // Our target of rotation starts with the player by default.
        target = player;

        allRigidbodies = Rigidbody.FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[];


    }

    void Update()
    {
        /* Handle flips. */
        if ((Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2")) && readyToFlip && !looking)
        {
            Debug.Log("Gravity flip activated!");
            robot.GetComponent<RobotBuddy>().StopMoving();
            readyToFlip = false;
            isGravityFlipped = !isGravityFlipped;

            foreach (Rigidbody body in allRigidbodies)
            {
                Debug.Log(body);
                body.isKinematic = true;
            }

            StartCoroutine(FlipLevel());

            if (usePostProcessingEffects)
                StartCoroutine(PostProcessingEffects());
        }

        /* Handle looking up. (Y button or L key) */
        if (readyToFlip && Input.GetButtonDown("LookUp"))
        {
            lookUpFadeAnimator.SetTrigger("LookUp");
        }
        else if (readyToFlip && Input.GetButtonUp("LookUp"))
        {
            lookUpFadeAnimator.ResetTrigger("LookUp");
            lookUpFadeAnimator.SetTrigger("StopLooking");
        }
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
        isGravityFlipped = !isGravityFlipped;
        if (engaged)
        {
            looking = true;
            playerLookUpProjector.enabled = true;
            robotLookUpProjector.enabled = true;
            Physics.gravity = -savedGravity;
        }
        else
        {
            looking = false;
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

    IEnumerator FlipLevel()
    {
        isFlipping = true;
        string trigger = isGravityFlipped ? "Flip1" : "Flip2";
        Vector3 savedPlayerHeading = player.transform.forward;
        Vector3 savedRobotHeading = robot.transform.forward;

        flippableAnimator.SetTrigger(trigger);
        Physics.gravity = Vector3.zero;
        yield return new WaitWhile(() => isFlipping); // Waiting on flip animation
        Physics.gravity = savedGravity;

        StartCoroutine("FlipCooldownTimer");

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
        // once we're done rotating, make us kinematic again
        foreach (Rigidbody body in allRigidbodies)
        {
            body.isKinematic = false;
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
        readyToFlip = true;
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

