using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Animation events during the startup logo and fade in.
public class StartGameAnimEvents : MonoBehaviour
{
    PostProcessVolume postProcessVolume;
    MainMenuManager mainMenuManager;

    void Start()
    {
        postProcessVolume = GameObject.Find("PostProcessVolume").GetComponent<PostProcessVolume>();
        mainMenuManager = GameObject.Find("MainMenuManager").GetComponent<MainMenuManager>();
    }

    public void DisablePostProcessing()
    {
        DepthOfField dof = null;
        postProcessVolume.profile.TryGetSettings(out dof);
        dof.active = true;
        postProcessVolume.enabled = false;
    }

    public void EnablePostProcessing()
    {
        postProcessVolume.enabled = true;
    }

    public void StartMusic()
    {
        mainMenuManager.StartMusic();
    }

    public void ButtonsAppear()
    {
        mainMenuManager.ButtonsAppear();
    }

    public void FocusDOF()
    {
        mainMenuManager.FocusDOF();
    }
}
