using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    StateManager stateManager;
    CanvasGroup canvasGroup;
    Image image;
    List<Tuple<AudioSource, float>> audioSourceVolumePairs;

    [Header("Fade options")]
    public bool fadeAudioInOut = true;
    public bool fadeOnSceneStart = true;
    public bool fadeOnSceneEnd = true;
    public float startFadeDuration = 2f;
    public float endFadeDuration = 2f;

    [Header("Color picker")]
    public Color startSceneColor = Color.black;
    public Color endSceneColor = Color.black;

    void Awake()
    {
        stateManager = FindObjectOfType<StateManager>();
        canvasGroup = transform.GetChild(0).gameObject.GetComponent<CanvasGroup>();
        image = transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>();
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        audioSourceVolumePairs = new List<Tuple<AudioSource, float>>();
        foreach (AudioSource audioSource in audioSources)
        {
            audioSourceVolumePairs.Add(new Tuple<AudioSource, float>(audioSource, audioSource.volume));
            print(audioSource.gameObject.name);
        }
    }

    void Start()
    {
        if (fadeOnSceneStart)
        {
            canvasGroup.alpha = 1f;
            image.color = startSceneColor;
            StartCoroutine(SceneStartProcess());
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }

    IEnumerator SceneStartProcess()
    {
        yield return null;
        canvasGroup.DOFade(0f, startFadeDuration);
        if (fadeAudioInOut)
        {
            foreach (Tuple<AudioSource, float> pair in audioSourceVolumePairs)
            {
                pair.Item1.volume = 0f;
                pair.Item1.DOFade(pair.Item2, startFadeDuration).SetEase(Ease.InQuad);
            }
        }
    }

    /// <summary>
    /// Calling this will take control away from player, fade to black and
    /// then load the next scene in the build order.
    /// </summary>
    public void LoadNextScene()
    {
        stateManager.SetState(StateManager.State.Inert);
        StartCoroutine(LoadNextSceneProcess());
    }

    public void LoadScene(int buildIndex)
    {
        stateManager.SetState(StateManager.State.Inert);
        StartCoroutine(LoadSceneProcess(buildIndex));
    }

    public void LoadSceneByName(string name)
    {
        stateManager.SetState(StateManager.State.Inert);
        StartCoroutine(LoadSceneProcess(name));
    }

    IEnumerator LoadNextSceneProcess()
    {
        if (fadeOnSceneEnd)
        {
            image.color = endSceneColor;
            Tween t = canvasGroup.DOFade(1f, endFadeDuration);
            if (fadeAudioInOut)
            {
                foreach (Tuple<AudioSource, float> pair in audioSourceVolumePairs)
                {
                    pair.Item1.DOFade(0f, endFadeDuration).SetEase(Ease.InOutQuad);
                }
            }
            yield return t.WaitForCompletion();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // BUILD INDEX VERSION
    IEnumerator LoadSceneProcess(int buildIndex)
    {
        if (fadeOnSceneEnd)
        {
            image.color = endSceneColor;
            Tween t = canvasGroup.DOFade(1f, endFadeDuration);
            if (fadeAudioInOut)
            {
                foreach (Tuple<AudioSource, float> pair in audioSourceVolumePairs)
                {
                    pair.Item1.DOFade(0f, endFadeDuration).SetEase(Ease.InOutQuad);
                }
            }
            yield return t.WaitForCompletion();
        }
        SceneManager.LoadScene(buildIndex);
    }

    // STRING NAME VERSION
    IEnumerator LoadSceneProcess(string sceneName)
    {
        if (fadeOnSceneEnd)
        {
            image.color = endSceneColor;
            Tween t = canvasGroup.DOFade(1f, endFadeDuration);
            if (fadeAudioInOut)
            {
                foreach (Tuple<AudioSource, float> pair in audioSourceVolumePairs)
                {
                    pair.Item1.DOFade(0f, endFadeDuration).SetEase(Ease.InOutQuad);
                }
            }
            yield return t.WaitForCompletion();
        }
        SceneManager.LoadScene(sceneName);
    }
}
