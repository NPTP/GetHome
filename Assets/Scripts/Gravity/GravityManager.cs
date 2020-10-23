using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class GravityManager : MonoBehaviour
{
    private GameObject target;
    private ThirdPersonUserControl thirdPersonUserControl;
    UnityEngine.Rendering.VolumeProfile volumeProfile;

    public float cooldownTime = 2f;
    public int degreesPerRotationStep = 2; // Must be a strict integer multiple of 90.
    int characterDegreesPerRotationStep = 5;
    public float rotationStepTime = .000001f;
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
        volumeProfile = GetComponent<UnityEngine.Rendering.Volume>()?.profile;
        if (!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));

        player = GameObject.FindGameObjectWithTag("Player");
        thirdPersonUserControl = player.GetComponent<ThirdPersonUserControl>();
        robot = GameObject.FindGameObjectWithTag("robot"); // TODO: fix tag capitalization
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
        // TODO: DOTween
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
        UnityEngine.Rendering.Universal.ChromaticAberration chromaticAberration;
        if (!volumeProfile.TryGet(out chromaticAberration)) throw new System.NullReferenceException(nameof(chromaticAberration));
        chromaticAberration.active = true;
        chromaticAberration.intensity.Override(0f);

        // Set up lens distortion.
        UnityEngine.Rendering.Universal.LensDistortion lensDistortion;
        if (!volumeProfile.TryGet(out lensDistortion)) throw new System.NullReferenceException(nameof(lensDistortion));
        lensDistortion.active = true;
        lensDistortion.intensity.Override(0f);

        // Set up color adjustments.
        UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
        if (!volumeProfile.TryGet(out colorAdjustments)) throw new System.NullReferenceException(nameof(colorAdjustments));
        colorAdjustments.active = true;
        colorAdjustments.saturation.Override(0f);
        colorAdjustments.postExposure.Override(0f);

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
            // chromaticAberration.intensity.Override((float)chromaticAberration.intensity + positiveDelta);
            chromaticAberration.intensity.Override(1f);
            lensDistortion.intensity.Override((float)lensDistortion.intensity + negativeDelta);
            colorAdjustments.saturation.Override((float)colorAdjustments.saturation + negativeDelta * 100f);
            if (60 <= degreesTurned && degreesTurned <= 120)
                colorAdjustments.postExposure.Override(Mathf.Clamp((float)colorAdjustments.postExposure + 30f * positiveDelta, 0f, 10f));
            yield return new WaitForSeconds(rotationStepTime);
        }

        chromaticAberration.active = false;
        lensDistortion.active = false;
        colorAdjustments.active = false;
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

