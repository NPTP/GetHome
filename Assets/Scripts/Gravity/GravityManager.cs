using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GravityManager : MonoBehaviour
{
    private GameObject target;
    private ThirdPersonUserControl thirdPersonUserControl;
    PostProcessVolume postProcessVolume;

    public float cooldownTime = 2f;
    public int degreesPerRotationStep = 2; // Must be a strict integer multiple of 90.
    int characterDegreesPerRotationStep = 5;
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
        if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2") && readyToFlip)
        {
            Debug.Log("Gravity flip activated!");
            readyToFlip = false;
            isGravityFlipped = !isGravityFlipped;
            StartCoroutine(FlipLevel());
            if (usePostProcessingEffects)
                StartCoroutine(PostProcessingEffects());
        }
    }

    IEnumerator FlipLevel()
    {
        // Turn off gravity while flipping ...
        isFlipping = true;
        Vector3 savedGravity = Physics.gravity;
        Physics.gravity = Vector3.zero;

        // ... Do the actual flipping ...
        if (isGravityFlipped)
        {
            while (degreesRotated < 180)
            {
                flippableContent.transform.Rotate(new Vector3(0f, 0f, (float)degreesPerRotationStep));
                degreesRotated += degreesPerRotationStep;
                yield return new WaitForSeconds(rotationStepTime);
            }
        }
        else
        {
            while (degreesRotated < 180)
            {
                flippableContent.transform.Rotate(new Vector3(0f, 0f, -(float)degreesPerRotationStep));
                degreesRotated += degreesPerRotationStep;
                yield return new WaitForSeconds(rotationStepTime);
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
        yield return new WaitForSeconds(0.25f);
        int characterDegreesRotated = 0;
        while (characterDegreesRotated < 180)
        {
            player.transform.Rotate(new Vector3(0f, 0f, (float)characterDegreesPerRotationStep), Space.Self);
            robot.transform.Rotate(new Vector3(0f, 0f, (float)characterDegreesPerRotationStep), Space.Self);
            characterDegreesRotated += characterDegreesPerRotationStep;
            yield return new WaitForSeconds(rotationStepTime);
        }
    }

    IEnumerator FlipCooldownTimer()
    {
        yield return new WaitForSeconds(cooldownTime);
        readyToFlip = true;
    }

    IEnumerator PostProcessingEffects()
    {
        // Delay for a moment.
        yield return new WaitForSeconds(rotationStepTime * 45f);

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
        float positiveDelta = (1f / 90f) * degreesPerRotationStep;
        float negativeDelta = -positiveDelta;

        // Apply effects over the course of the flippableContent rotation.
        for (int degreesTurned = 0; degreesTurned < 180; degreesTurned += degreesPerRotationStep)
        {
            if (degreesTurned == 90)
            {
                positiveDelta *= -1f;
                negativeDelta *= -1f;
            }
            chromaticAberration.intensity.value = 1f;
            lensDistortion.intensity.value = lensDistortion.intensity.value + negativeDelta * 100f;
            colorGrading.saturation.value = colorGrading.saturation.value + negativeDelta * 100f;
            if (60 <= degreesTurned && degreesTurned <= 120)
                colorGrading.postExposure.value = Mathf.Clamp(colorGrading.postExposure + 30f * positiveDelta, 0f, 10f);
            yield return new WaitForSeconds(rotationStepTime);
        }

        chromaticAberration.active = false;
        lensDistortion.active = false;
        colorGrading.active = false;
    }

    IEnumerator FlipLevel_OldVersion()
    {
        // // Logic for choosing a rotation target.
        // GameObject unselectedCharacter;
        // target = thirdPersonUserControl.GetSelectedCharacter();
        // if (target == player)
        //     unselectedCharacter = robot;
        // else
        //     unselectedCharacter = player;
        // Vector3 targetPosFixed = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);

        // Turn off gravity while flipping
        Vector3 savedGravity = Physics.gravity;
        Physics.gravity = Vector3.zero;

        // Do the actual flipping
        if (flippableContent.transform.rotation.eulerAngles.z < 180.0f)
        {
            while (flippableContent.transform.rotation.eulerAngles.z < 180.0f)
            {
                // player.transform.RotateAround(targetPosFixed, Vector3.forward, (float)degreesPerRotationStep);
                // robot.transform.RotateAround(targetPosFixed, Vector3.forward, (float)degreesPerRotationStep);
                // flippableContent.transform.RotateAround(targetPosFixed, Vector3.forward, (float)degreesPerRotationStep);
                flippableContent.transform.Rotate(new Vector3(0f, 0f, (float)degreesPerRotationStep));
                yield return new WaitForSeconds(rotationStepTime);
            }
        }
        else
        {
            float currentLevelZRotation = flippableContent.transform.rotation.eulerAngles.z;
            float zDifference = currentLevelZRotation - 180.0f;
            while (flippableContent.transform.rotation.eulerAngles.z > zDifference)
            {
                // player.transform.RotateAround(targetPosFixed, Vector3.forward, -(float)degreesPerRotationStep);
                // robot.transform.RotateAround(targetPosFixed, Vector3.forward, -(float)degreesPerRotationStep);
                // flippableContent.transform.RotateAround(targetPosFixed, Vector3.forward, -(float)degreesPerRotationStep);
                flippableContent.transform.Rotate(new Vector3(0f, 0f, -(float)degreesPerRotationStep));
                yield return new WaitForSeconds(rotationStepTime);
            }
        }

        // ... then turn gravity back on again.
        Physics.gravity = savedGravity;

        // And finally allow folks the freedom to move!
        isFlipping = false;

        // Now reorient the player & robot rotation while they fall to the "new" ground.
        yield return new WaitForSeconds(0.25f);
        int sum = 0;

        while (sum < 180)
        {
            player.transform.Rotate(new Vector3(0f, 0f, (float)characterDegreesPerRotationStep), Space.Self);
            robot.transform.Rotate(new Vector3(0f, 0f, (float)characterDegreesPerRotationStep), Space.Self);
            sum += characterDegreesPerRotationStep;
            yield return new WaitForSeconds(rotationStepTime);
        }
    }

}

