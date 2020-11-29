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
    SceneLoader sceneLoader;

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
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    void Start()
    {
        InitializeEndingText();
        StartCoroutine("Ending");
    }

    IEnumerator Ending()
    {
        textCanvasGroup.alpha = 0f;
        yield return new WaitForSeconds(sceneLoader.startFadeDuration);

        for (int page = 0; page < endingText.Length; page++)
        {
            Tween t;
            text.text = endingText[page];
            t = textCanvasGroup.DOFade(1f, 1f);
            yield return t.WaitForCompletion();

            yield return new WaitForSecondsRealtime(textHoldTime);
            t = textCanvasGroup.DOFade(0f, 1f);

            yield return t.WaitForCompletion();

            yield return new WaitForSecondsRealtime(waitBetweenTextTime);
        }

        sceneLoader.LoadNextScene();
    }

    void InitializeEndingText()
    {
        endingText = new string[] {
            "Every action of our lives touches on some chord that will vibrate in eternity. \n\nEdwin Hubbell Chapin"
        };
    }

}
