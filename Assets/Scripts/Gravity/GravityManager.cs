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
    private GameObject flippableContent;

    // Public vars for other classes to check on progress.
    public bool isGravityFlipped = false;
    [HideInInspector] public int degreesRotated = 0;
    public Animator flippableAnimator;
    public Animator lookUpFadeAnimator;
    bool looking = false;

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
        flippableContent = GameObject.FindGameObjectWithTag("Flippable");

        // Our target of rotation starts with the player by default.
        target = player;
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2")) && readyToFlip)
        {
            Debug.Log("Gravity flip activated!");
            readyToFlip = false;
            isGravityFlipped = !isGravityFlipped;
            StartCoroutine(FlipLevel());
            if (usePostProcessingEffects)
                StartCoroutine(PostProcessingEffects());
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            looking = true;
            lookUpFadeAnimator.SetTrigger("Start");
        }
        else if (Input.GetKeyUp(KeyCode.L))
        {
            looking = false;
            lookUpFadeAnimator.SetTrigger("Start");
        }

    }

    IEnumerator FlipLevel()
    {
        float x = Time.realtimeSinceStartup;

        string trigger = isGravityFlipped ? "Flip1" : "Flip2";
        flippableAnimator.SetTrigger(trigger);
        isFlipping = true;

        Vector3 savedPlayerHeading = player.transform.forward;
        Vector3 savedRobotHeading = robot.transform.forward;
        Physics.gravity = Vector3.zero;

        yield return new WaitWhile(() => isFlipping); // Waiting on flip animation

        Physics.gravity = savedGravity;
        StartCoroutine("FlipCooldownTimer");

        // Reorient player & robot rotation while they fall to the "new" ground.
        yield return new WaitForSeconds(Mathf.Clamp(0.25f - Time.deltaTime, 0f, 0.25f));
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

        float y = Time.realtimeSinceStartup;
        Debug.Log(y - x);
    }

    public void SetFlipping(bool value)
    {
        isFlipping = value;
    }

    public void InstantFlip()
    {
        if (looking)
        {
            // readyToFlip = false;
            isGravityFlipped = !isGravityFlipped;
            Physics.gravity = -savedGravity;
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponent<CapsuleCollider>().enabled = false;
            flippableContent.transform.localScale = new Vector3(
                flippableContent.transform.localScale.x,
                -flippableContent.transform.localScale.y,
                flippableContent.transform.localScale.z
            );
            player.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<CapsuleCollider>().enabled = true;
            lookUpFadeAnimator.ResetTrigger("Start");
        }
        else
        {
            readyToFlip = true;
            isGravityFlipped = !isGravityFlipped;
            Physics.gravity = savedGravity;
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponent<CapsuleCollider>().enabled = false;
            flippableContent.transform.localScale = new Vector3(
                flippableContent.transform.localScale.x,
                -flippableContent.transform.localScale.y,
                flippableContent.transform.localScale.z
            );
            player.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<CapsuleCollider>().enabled = true;
            lookUpFadeAnimator.ResetTrigger("Start");
        }
    }

    IEnumerator FlipLevel_Old()
    {
        // float x = Time.realtimeSinceStartup;

        // Turn off gravity while flipping ...
        Vector3 savedPlayerheading = player.transform.forward;
        isFlipping = true;
        Physics.gravity = Vector3.zero;

        // ... Do the actual flipping ...
        if (isGravityFlipped)
        {
            while (degreesRotated < 180)
            {
                flippableContent.transform.Rotate(new Vector3((float)degreesPerRotationStep, 0f, 0f));
                degreesRotated += degreesPerRotationStep;
                yield return new WaitForSeconds(Mathf.Clamp(rotationStepTime - Time.deltaTime, 0f, rotationStepTime));
            }
        }
        else
        {
            while (degreesRotated < 180)
            {
                flippableContent.transform.Rotate(new Vector3(-(float)degreesPerRotationStep, 0f, 0f));
                degreesRotated += degreesPerRotationStep;
                yield return new WaitForSeconds(Mathf.Clamp(rotationStepTime - Time.deltaTime, 0f, rotationStepTime));
            }
        }

        // ... then turn gravity back on & let everyone know level rotation has finished.
        Physics.gravity = savedGravity;
        isFlipping = false;
        degreesRotated = 0;

        // Call the cooldown timer to stop player from spamming gravity flips.
        StartCoroutine("FlipCooldownTimer");

        // Reorient player & robot rotation while they fall to the "new" ground.
        // TODO: Test with DOTween, potentially nicer & more consistent results.
        yield return new WaitForSeconds(Mathf.Clamp(0.25f - Time.deltaTime, 0f, 0.25f));
        int characterDegreesRotated = 0;
        while (characterDegreesRotated < 180)
        {
            player.transform.Rotate(new Vector3(0f, 0f, (float)characterDegreesPerRotationStep), Space.Self);
            robot.transform.Rotate(new Vector3(0f, 0f, (float)characterDegreesPerRotationStep), Space.Self);
            characterDegreesRotated += characterDegreesPerRotationStep;
            yield return null;
        }
        player.transform.forward = savedPlayerheading;

        // float y = Time.realtimeSinceStartup;
        // Debug.Log(y - x);
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

        // Set up deltas for each post-process effect's changes over time, calibrated to a 180-degree flippableContent rotation.
        float time = 2f;
        float halfTime = time / 2;
        float quarterTime = halfTime / 2;

        // Apply effects over the course of the flippableContent rotation.
        float elapsedFirstHalf = 0f;
        while (elapsedFirstHalf < halfTime)
        {
            float scale = elapsedFirstHalf / halfTime;
            chromaticAberration.intensity.value = scale * 1f;
            lensDistortion.intensity.value = scale * 100f;
            colorGrading.saturation.value = scale * 100f;
            if (elapsedFirstHalf > quarterTime)
                colorGrading.postExposure.value = ((elapsedFirstHalf - (quarterTime)) / (quarterTime)) * 5f;
            elapsedFirstHalf += Time.deltaTime;
            yield return null;
        }
        float elapsedSecondHalf = 0f;
        while (elapsedSecondHalf < halfTime)
        {
            float scale = (halfTime - elapsedSecondHalf) / halfTime;
            chromaticAberration.intensity.value = scale * 1f;
            lensDistortion.intensity.value = scale * 100f;
            colorGrading.saturation.value = scale * 100f;
            if (elapsedSecondHalf <= quarterTime)
                colorGrading.postExposure.value = ((quarterTime) - (elapsedSecondHalf / 2)) / (quarterTime) * 5f;
            else if (colorGrading.postExposure.value > 0)
                colorGrading.postExposure.value -= .1f;
            elapsedSecondHalf += Time.deltaTime;
            yield return null;
        }

        chromaticAberration.active = false;
        lensDistortion.active = false;
        colorGrading.active = false;
    }

}

