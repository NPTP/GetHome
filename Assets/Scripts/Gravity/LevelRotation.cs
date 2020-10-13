﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LevelRotation : MonoBehaviour
{
    // TODO: This still messes with X and Z coordinates! 
    // maybe it'll be easier to just flip everythings y scale or something?
    public GravityManager gravityManager;
    private GameObject target;
    private ThirdPersonUserControl thirdPersonUserControl;
    UnityEngine.Rendering.VolumeProfile volumeProfile;
    public int degreesPerStep = 2; // Must be a strict integer multiple of 90.
    public float stepTime = .000001f;
    public bool usePostProcessingEffects = true;
    public bool isFlipping = false;
    private GameObject player;
    private GameObject robot;
    private GameObject level;
    void Start()
    {
        // Warning if you've calibrated the rotation steps badly.
        if (90 % degreesPerStep != 0)
            Debug.Log("WARNING! Level rotation degreesPerStep is not a multiple of 90. Level rotation could break.");

        // Get a reference to the post-processing volume for gravity flipping.
        GameObject postProcessvolume = GameObject.Find("GravityManager"); // ("GravityPostProcessing");
        volumeProfile = postProcessvolume.GetComponent<UnityEngine.Rendering.Volume>()?.profile;
        if (!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));

        player = GameObject.FindGameObjectWithTag("Player");
        thirdPersonUserControl = player.GetComponent<ThirdPersonUserControl>();
        robot = GameObject.FindGameObjectWithTag("robot"); // TODO: can we capitalize this to make it consistent for debugging? Just didn't want to change it now in case it's referenced elsewhere
        level = GameObject.FindGameObjectWithTag("Level");

        // Our target of rotation starts with the player by default.
        target = player;
    }

    void Update()
    {
        if (gravityManager.gravityFlip)
        {
            StartCoroutine("FlipLevel");
            if (usePostProcessingEffects)
                StartCoroutine("PostProcessingEffects");
            gravityManager.gravityFlip = false;

        }
    }

    IEnumerator FlipLevel()
    {
        GameObject unselectedCharacter;
        target = thirdPersonUserControl.GetSelectedCharacter();
        if (target == player)
            unselectedCharacter = robot;
        else
            unselectedCharacter = player;

        Vector3 targetPosFixed = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
        float currentLevelZRotation = level.transform.rotation.eulerAngles.z;

        // Turn off gravity while flipping...
        Vector3 savedGravity = Physics.gravity;
        Physics.gravity = Vector3.zero;
        isFlipping = true;

        if (transform.rotation.eulerAngles.z < 180.0f)
        {
            while (transform.rotation.eulerAngles.z < 180.0f)
            {
                transform.RotateAround(targetPosFixed, Vector3.forward, (float)degreesPerStep);
                yield return new WaitForSeconds(stepTime);
            }
        }
        else
        {
            float zDifference = currentLevelZRotation - 180.0f;
            while (transform.rotation.eulerAngles.z > zDifference)
            {
                transform.RotateAround(targetPosFixed, Vector3.forward, -(float)degreesPerStep);
                yield return new WaitForSeconds(stepTime);
            }
        }

        // ... then turn gravity back on again.
        isFlipping = false;
        Physics.gravity = savedGravity;
    }

    IEnumerator PostProcessingEffects()
    {
        // Delay for a moment.
        yield return new WaitForSeconds(stepTime * 45f);

        // Set up deltas for each post-process effect's changes over time, calibrated to a 180-degree level rotation.
        float positiveDelta = (1f / 90f) * degreesPerStep;
        float negativeDelta = -positiveDelta;

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

        // Apply effects over the course of the level rotation.
        for (int degreesTurned = 0; degreesTurned < 180; degreesTurned += degreesPerStep)
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
            colorAdjustments.postExposure.Override((float)colorAdjustments.postExposure + positiveDelta * 2f);
            yield return new WaitForSeconds(stepTime);
        }

        chromaticAberration.active = false;
        lensDistortion.active = false;
        colorAdjustments.active = false;
    }

}
