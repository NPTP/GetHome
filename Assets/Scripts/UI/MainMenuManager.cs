using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

// Beast class that takes care of just about everything for the main menu.
// Because much of this logic is not going to be reusable elsewhere, it's
// a fairly coupled and hardcoded approach right now.
// Everything gets sent to this class.
public class MainMenuManager : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;
    public GameObject buttonsParent;
    [Range(0.0f, 1.0f)] public float buttonRotationScale = 0.05f;
    public TMP_Text titleText;
    public Button newGameButton;
    public Button resumeButton;
    public Button quitButton;
    public Animator transitionAnimator;
    public float dipToBlackTransitionTime = 1.5f;

    private bool isInteractable = false;
    private Button[] buttons;
    private Button selected;

    void Start()
    {
        // Set up for intro animations
        titleText.maxVisibleCharacters = 0;

        // Set up buttons and subscribe to their events
        Button[] b = { newGameButton, resumeButton, quitButton };
        buttons = b;
        for (int i = 0; i < buttons.Length; i++)
        {
            MainMenuButtonEvents events = buttons[i].GetComponent<MainMenuButtonEvents>();
            events.OnButtonEvent += HandleButtonEvent;
        }

        foreach (Transform child in buttonsParent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // If this is true, it means all our intro start-up animation stuff is done
        // and we can start interacting with the menu.
        if (isInteractable)
            RotateButtons();
    }

    void HandleButtonEvent(object sender, MainMenuButtonEvents.OnButtonEventArgs e)
    {
        if (isInteractable)
        {
            MainMenuButtonEvents mmbe = (MainMenuButtonEvents)sender;
            Button b = mmbe.button;
            if (e.eventType == "Highlighted")
                b.Select();
            if (e.eventType == "Clicked")
            {
                if (e.buttonIndex == 0)
                    StartNewGame();
            }
        }
    }

    void RotateButtons()
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 10.0f; // Virtual distance of plane from camera
        Quaternion lookRotation = Quaternion.LookRotation(Camera.main.ScreenToWorldPoint(screenPoint), Vector3.up);
        Quaternion inverseLookRotation = Quaternion.Inverse(lookRotation);
        buttonsParent.transform.rotation = Quaternion.Lerp(
            Quaternion.identity,
            inverseLookRotation,
            buttonRotationScale
        );
    }

    public void StartNewGame()
    {
        if (isInteractable)
        {
            isInteractable = false;
            SetButtonsInteractable(false);
            StartCoroutine(StartNewGameTransition());
        }
    }
    IEnumerator StartNewGameTransition()
    {
        StartCoroutine(ZoomOutLens());
        transitionAnimator.SetTrigger("StartNewGame");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator ZoomOutLens()
    {
        LensDistortion lensDistortion = null;
        postProcessVolume.profile.TryGetSettings(out lensDistortion);
        while (lensDistortion.intensity.value > -100f)
        {
            lensDistortion.intensity.value -= 0.25f;
            yield return null;
        }
    }

    void SetButtonsInteractable(bool setting)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[0].interactable = setting;
        }
    }

    public void StartMusic()
    {
        GetComponent<AudioSource>().Play();
    }

    public void ButtonsAppear()
    {
        StartCoroutine(ButtonsAppearAnimation());
    }

    public void FocusDOF()
    {
        StartCoroutine(FocusDOFAnimation());
    }

    IEnumerator FocusDOFAnimation()
    {
        DepthOfField dof = null;
        postProcessVolume.profile.TryGetSettings(out dof);
        dof.focusDistance.value = .1f;
        while (dof.focusDistance.value < 6f)
        {
            dof.focusDistance.value += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator ButtonsAppearAnimation()
    {
        titleText.gameObject.SetActive(true);
        for (int i = 0; i <= titleText.text.Length; i++)
        {
            titleText.maxVisibleCharacters = i;
            if (i < titleText.text.Length && titleText.text[i] != ' ')
                yield return new WaitForSeconds(.2f);
        }

        // TODO: tie all this to the opening animator with keyframed events
        ChromaticAberration chromaticAberration = null;
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        chromaticAberration.active = true;

        chromaticAberration.intensity.value = .75f;
        yield return new WaitForSeconds(.4f);
        chromaticAberration.intensity.value = 0f;
        yield return new WaitForSeconds(.2f);
        chromaticAberration.intensity.value = .75f;
        yield return new WaitForSeconds(.05f);
        chromaticAberration.intensity.value = 0f;
        yield return new WaitForSeconds(.05f);
        chromaticAberration.intensity.value = .75f;
        yield return new WaitForSeconds(.05f);
        chromaticAberration.intensity.value = 0f;

        chromaticAberration.active = false;

        // Slight delay before next step
        yield return new WaitForSeconds(.35f);

        // Make the options buttons appear
        StartCoroutine(BloomFlash());
        foreach (Transform child in buttonsParent.transform)
        {
            yield return new WaitForSeconds(.15f);
            child.gameObject.SetActive(true);
        }

        // Buttons become interactable in BloomFlash coroutine.
    }

    IEnumerator BloomFlash()
    {
        Bloom bloom = null;
        postProcessVolume.profile.TryGetSettings(out bloom);

        yield return new WaitForSeconds(1f);
        float waitStep = 0.005f;

        while (bloom.intensity.value < 30f)
        {
            bloom.intensity.value += 1f;
            yield return new WaitForSeconds(waitStep);
        }

        // Let the player interact now, and auto-select the first button
        isInteractable = true;
        buttons[0].Select();

        while (bloom.intensity.value > 8f)
        {
            bloom.intensity.value -= 1f;
            yield return new WaitForSeconds(waitStep);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}

