using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class EndingFX : MonoBehaviour
{
    SceneLoader sceneLoader;
    PostProcessVolume postProcessVolume;
    Animator animator;
    AudioSource forcefieldSound;

    [SerializeField] GameObject existingParticles;
    [SerializeField] ParticleSystem extraParticles;

    [HideInInspector] public float fxScale;
    [HideInInspector] public bool isAnimating;

    void Awake()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
        postProcessVolume = GetComponent<PostProcessVolume>();
        animator = GetComponent<Animator>();
        forcefieldSound = transform.GetChild(1).GetComponent<AudioSource>();

        // The animator should take double the scene-loader fade time,
        // so that the fade can start halfway through the animation
        // and finish at the same time.
        animator.speed = 1 / (sceneLoader.endFadeDuration * 2);

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
    float bloomIntensityEnd = 5f;

    AutoExposure autoExposure;
    float autoExposureKeyValueEnd = 10f;

    public void EngageFX()
    {
        isAnimating = true;
        StartCoroutine(FXAnimation());
    }

    IEnumerator FXAnimation()
    {
        forcefieldSound.DOFade(0f, 0.25f);
        ChangePostProcessFXActive(true);
        animator.Play("EndingFX", 0);
        extraParticles.Play(true);

        while (isAnimating)
        {
            // Particles parent scale should be equal to "Vector3.one"
            existingParticles.transform.localScale = new Vector3(1 - fxScale, 1 - fxScale, 1 - fxScale);

            lensDistortion.intensity.value = fxScale * lensDistortionIntensityEnd;
            lensDistortion.scale.value = (fxScale * lensDistortionScaleModifier) + 1f;

            chromaticAberration.intensity.value = fxScale * chromaticAberrationIntensityEnd;

            bloom.intensity.value = fxScale * bloomIntensityEnd;

            autoExposure.keyValue.value = fxScale * autoExposureKeyValueEnd;

            yield return null;
        }

    }

}
