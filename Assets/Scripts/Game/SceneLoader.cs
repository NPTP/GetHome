using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    StateManager stateManager;
    CanvasGroup canvasGroup;

    [Header("Fade in from black on scene start?")]
    public bool fadeInOnSceneStart = true;
    public float fadeInDuration = 1f;

    void Awake()
    {
        stateManager = FindObjectOfType<StateManager>();
        canvasGroup = transform.GetChild(0).gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        if (fadeInOnSceneStart)
            canvasGroup.DOFade(0f, fadeInDuration).From(1f);
    }

    /// <summary>
    /// Calling this will take control away from player, fade to black and
    /// then load the next scene in the build order.
    /// </summary>
    public void LoadNextScene(float fadeDuration = 1f)
    {
        stateManager.SetState(StateManager.State.Inert);

        StartCoroutine(LoadNextSceneProcess(fadeDuration));
    }

    IEnumerator LoadNextSceneProcess(float fadeDuration)
    {
        Tween t = canvasGroup.DOFade(1f, fadeDuration);
        yield return new WaitWhile(() => t != null && t.IsPlaying());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
