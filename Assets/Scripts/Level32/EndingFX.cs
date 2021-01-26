using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EndingFX : MonoBehaviour
{
    PostProcessVolume postProcessVolume;
    Animator animator;
    float animatorSpeed = .25f;

    [HideInInspector] public float fxScale;
    [HideInInspector] public bool isAnimating;

    void Awake()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();
        animator = GetComponent<Animator>();
        animator.speed = animatorSpeed;
        InitializePostProcessFX();
    }

    void InitializePostProcessFX()
    {
        postProcessVolume.profile.TryGetSettings(out lensDistortion);
        lensDistortion.intensity.value = 0f;
        lensDistortion.scale.value = 1f;

        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        chromaticAberration.intensity.value = 0f;

        postProcessVolume.profile.TryGetSettings(out bloom);
        bloom.intensity.value = 0f;

        postProcessVolume.profile.TryGetSettings(out autoExposure);
        autoExposure.keyValue.value = 1f;

        ChangePostProcessFXActive(false);
    }

    void ChangePostProcessFXActive(bool setting)
    {
        lensDistortion.active = setting;
        chromaticAberration.active = setting;
        bloom.active = setting;
        autoExposure.active = setting;
    }

    public void SetFlag()
    {
        isAnimating = true;
    }

    public void ClearFlag()
    {
        isAnimating = false;
    }

    // ************************************************************************
    // **** Parameters and animation
    // ************************************************************************

    LensDistortion lensDistortion; // = null; ???????
    float lensDistortionIntensityEnd = -100f;
    float lensDistortionScaleModifier = -0.99f;

    ChromaticAberration chromaticAberration;
    float chromaticAberrationIntensityEnd = 1f;

    Bloom bloom;
    float bloomIntensityEnd = 25f;

    AutoExposure autoExposure;
    float autoExposureKeyValueEnd = 100f;

    public void EngageFX()
    {
        isAnimating = true;
        StartCoroutine(FXAnimation());
    }

    IEnumerator FXAnimation()
    {
        // TODO: particle stuff!

        ChangePostProcessFXActive(true);
        animator.Play("EndingFX", 0);

        while (isAnimating)
        {
            lensDistortion.intensity.value = fxScale * lensDistortionIntensityEnd;
            lensDistortion.scale.value = (fxScale * lensDistortionScaleModifier) + 1f;

            chromaticAberration.intensity.value = fxScale * chromaticAberrationIntensityEnd;

            bloom.intensity.value = fxScale * bloomIntensityEnd;

            autoExposure.keyValue.value = fxScale * autoExposureKeyValueEnd;

            yield return null;
        }

    }

}
