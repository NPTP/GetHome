using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// Sceneloader handles audio fade in/out
public class EndingScript : MonoBehaviour
{
    float textSpeed = 0.03f;//0.02f;
    CanvasGroup textCanvasGroup;
    TMP_Text text;
    AudioSource textAudio;
    SceneLoader sceneLoader;
    AudioSource music;
    float savedVolume;

    string[] endingText;
    bool creditsOver = false;

    public float textHoldTime = 4f;
    public float textFadeTime = 1f;
    public float waitBetweenTextTime = 0.25f;

    void Awake()
    {
        GameObject textObject = GameObject.Find("Text");
        textCanvasGroup = textObject.GetComponent<CanvasGroup>();
        text = textObject.GetComponent<TMP_Text>();
        textAudio = textObject.GetComponent<AudioSource>();
        sceneLoader = FindObjectOfType<SceneLoader>();
        music = GetComponent<AudioSource>();
        savedVolume = music.volume;
    }

    void Start()
    {
        InitializeEndingText();
        StartCoroutine("Ending");
    }

    IEnumerator Ending()
    {
        music.volume = 0f;
        music.DOFade(savedVolume, 8f).SetEase(Ease.InOutCubic);
        textCanvasGroup.alpha = 0f;
        yield return new WaitForSeconds(sceneLoader.startFadeDuration);

        for (int page = 0; page < endingText.Length; page++)
        {
            Tween t;
            text.text = endingText[page];
            t = textCanvasGroup.DOFade(1f, 1f);
            yield return new WaitWhile(() => t != null && t.IsPlaying());

            yield return new WaitForSecondsRealtime(textHoldTime);
            t = textCanvasGroup.DOFade(0f, 1f);

            yield return new WaitWhile(() => t != null && t.IsPlaying());

            yield return new WaitForSecondsRealtime(waitBetweenTextTime);
        }

        yield return new WaitForSecondsRealtime(3f);
        music.DOFade(0f, 5f).SetEase(Ease.InOutCubic);
        sceneLoader.LoadNextScene();
    }

    void InitializeEndingText()
    {
        endingText = new string[] {
            "Every action of our lives touches on some chord that will vibrate in eternity. \n\nEdwin Hubbel Chapin",
            "Some more text, perhaps?",
            "Wow, you guys really like this. Well, I'll keep going. \n\nFor now.",
            "Actually, I'm done. Onto the credits you go.",
            "Bye now."
        };
    }

}
