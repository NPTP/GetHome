using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

// Beast class that takes care of just about everything for the PAUSE menu.
// Because much of this logic is not going to be reusable elsewhere (EXCEPT FOR THE PAUSE MENU), it's
// a fairly coupled and hardcoded approach right now (YOU"RE TELLING ME).
// Everything gets sent to this class.
public class PauseMenuManager : MonoBehaviour
{
    public GameObject buttonsParent;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    private bool isInteractable = false;
    private Button[] buttons;
    private Button selected;

    public AudioSource clickSound;
    public AudioSource hoverSound;

    private int SceneToLoad;
    private int CheckpointScene;

    private void Awake()
    {
        buttons = new Button[3];
        buttons[0] = resumeButton;
        buttons[1] = restartButton;
        buttons[2] = quitButton;
    }
    void Start()
    {
        // Set up buttons and subscribe to their events

        //= { resumeButton, restartButton, quitButton };
        // = b;
        for (int i = 0; i < buttons.Length; i++)
        {
            PauseButtonEvents events = buttons[i].GetComponent<PauseButtonEvents>();
            events.OnButtonEvent += HandleButtonEvent;
        }
        SetButtonsInteractable(true);
        buttons[0].Select();
    }

    public void SelectFirstButton()
    {
        buttons[0].Select();
    }

    void HandleButtonEvent(object sender, PauseButtonEvents.OnButtonEventArgs e)
    {
        if (isInteractable)
        {
            hoverSound.Play();
            PauseButtonEvents mmbe = (PauseButtonEvents)sender;
            Button b = mmbe.button;
            if (e.eventType == "Highlighted")
            {
                b.Select();
            }
            if (e.eventType == "Clicked")
            {
                if (e.buttonIndex == 0)
                    ResumeGame();
                if (e.buttonIndex == 1)
                    RestartLevel();
                if (e.buttonIndex == 2)
                    QuitGame();
            }
        }
    }

    public void QuitGame()
    {
        //Time.timeScale = 1; // reset timeScale before we leave scene!
        // Create return from level indicator object
        GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonUserControl>().unpause();
        GameObject.Instantiate(Resources.Load("ReturnFromLevel"), Vector3.zero, Quaternion.identity);
        SceneManager.LoadScene(0);
    }

    public void RestartLevel()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonUserControl>().unpause();
        GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonUserControl>().ResetScene();
    }

    public void ResumeGame()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonUserControl>().unpause();
    }

    void SetButtonsInteractable(bool setting)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[0].interactable = setting;
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

