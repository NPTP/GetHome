using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
    public GameObject prompt;
    public Animator transitionAnimator;
    public float dipToBlackTransitionTime = 1.5f;

    private bool isInteractable = false;
    private Button[] buttons;
    private Button selected;

    public AudioSource clickSound;
    public AudioSource hoverSound;
    public AudioSource introTextSound;
    public AudioSource showButtonsSound;

    private int SceneToLoad;
    private int CheckpointScene;
    private bool skipIntro = false;
    private bool musicStarted = false;

    void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
        prompt.SetActive(false);

        // check if we have a saved checkpoint, so that we make "resume" button active
        if (PlayerPrefs.HasKey("checkpoint"))
        {
            buttons[1].interactable = true;
            buttons[1].transform.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, 1);
            CheckpointScene = PlayerPrefs.GetInt("checkpoint");
        }

        // Check if we are returning from a level to this menu.
        ReturnFromLevel returnFromLevel = GameObject.FindObjectOfType<ReturnFromLevel>();
        if (returnFromLevel != null)
        {
            skipIntro = true;
            Destroy(returnFromLevel.gameObject);
            StartCoroutine(QuickStart());
        }

    }

    void Update()
    {
        // If this is true, it means all our intro start-up animation stuff is done
        // and we can start interacting with the menu.
        if (isInteractable)
            RotateButtons();

        // Check for skipping
        if ((Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Escape)) && !skipIntro)
        {
            skipIntro = true;
            StartCoroutine(QuickStart());
        }


        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find("NEWGAMEButton"));
        }

        // TODO: Secret debug level select on the 1,2,3,... keys.
    }

    void HandleButtonEvent(object sender, MainMenuButtonEvents.OnButtonEventArgs e)
    {
        if (isInteractable)
        {
            hoverSound.Play();
            MainMenuButtonEvents mmbe = (MainMenuButtonEvents)sender;
            Button b = mmbe.button;
            if (e.eventType == "Highlighted")
            {
                b.Select();
            }
            if (e.eventType == "Clicked")
            {
                if (e.buttonIndex == 0)
                    StartNewGame();
                if (e.buttonIndex == 1)
                    ResumeGame();
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
            // clear save game flag if we have one
            if (PlayerPrefs.HasKey("checkpoint"))
            {
                // TODO: Maybe add "This will replace your saved game, are you sure?!"
                PlayerPrefs.DeleteKey("checkpoint");
            }
            SceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
            clickSound.Play();          // play game starting sound
            isInteractable = false;
            SetButtonsInteractable(false);
            StartCoroutine(StartNewGameTransition());
        }
    }

    public void ResumeGame()
    {
        if (isInteractable)
        {
            SceneToLoad = CheckpointScene;
            clickSound.Play();          // play game starting sound
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
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadScene(SceneToLoad);
    }

    IEnumerator ZoomOutLens()
    {
        LensDistortion lensDistortion = null;
        postProcessVolume.profile.TryGetSettings(out lensDistortion);
        while (lensDistortion.intensity.value > -100f)
        {
            lensDistortion.intensity.value -= 0.5f;
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
        GetComponent<AudioSource>().Play(); // TODO: Make sure this is the right AudioSource!
        musicStarted = true;
    }

    public void ButtonsAppear()
    {
        StartCoroutine("ButtonsAppearAnimation");
    }

    public void FocusDOF()
    {
        StartCoroutine("FocusDOFAnimation");
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
            introTextSound.Play();
            introTextSound.pitch += (Random.Range(0, 0.01f) - 0.005f);
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

        // Make the options buttons appear. NOTE: Does not make the "Accept" prompt appear.

        StartCoroutine("BloomFlash");

        foreach (Transform child in buttonsParent.transform)
        {
            yield return new WaitForSeconds(.15f);
            child.gameObject.SetActive(true);
        }
        showButtonsSound.Play();
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
            bloom.intensity.value += 1.5f;
            yield return new WaitForSeconds(waitStep);
        }

        // Peak of bloom flash. Let the player interact now, auto-select the first button, prompt appears
        isInteractable = true;
        buttons[0].Select();
        prompt.SetActive(true);
        foreach (Transform child in buttonsParent.transform)
        {
            child.gameObject.SetActive(true);
        }

        while (bloom.intensity.value > 8f)
        {
            bloom.intensity.value -= 1.5f;
            yield return new WaitForSeconds(waitStep);
        }
    }

    IEnumerator QuickStart()
    {
        if (!musicStarted)
            StartMusic();

        transitionAnimator.SetTrigger("SkipIntro");
        StopCoroutine("ButtonsAppearAnimation");
        StopCoroutine("FocusDOF");
        StopCoroutine("BloomFlash");

        titleText.gameObject.SetActive(true);
        titleText.maxVisibleCharacters = titleText.text.Length;

        postProcessVolume.enabled = true;

        // Turn off DOF
        DepthOfField dof = null;
        postProcessVolume.profile.TryGetSettings(out dof);
        dof.active = false;

        // Turn off chromatic aberration
        ChromaticAberration chromaticAberration = null;
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        chromaticAberration.active = false;

        // Make menu work
        isInteractable = true;
        prompt.SetActive(true);
        foreach (Transform child in buttonsParent.transform)
        {
            child.gameObject.SetActive(true);
        }
        buttons[0].Select();

        // Make bloom flash happen
        Bloom bloom = null;
        postProcessVolume.profile.TryGetSettings(out bloom);
        bloom.active = true;
        float waitStep = 0.005f;
        bloom.intensity.value = 30f;
        while (bloom.intensity.value > 8f)
        {
            bloom.intensity.value -= 1.5f;
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

