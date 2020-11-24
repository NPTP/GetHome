using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class CheckPoint : MonoBehaviour
{
    GameObject canvas;
    TMP_Text text;

    void Awake()
    {
        canvas = transform.GetChild(0).gameObject;
        text = canvas.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        text.alpha = 0f;
        text.maxVisibleCharacters = 6;
    }

    void Start()
    {
        int index = SceneManager.GetActiveScene().buildIndex;   // Get the index of this current scene
        PlayerPrefs.SetInt("checkpoint", index);                // save it to our player prefs
        StartCoroutine(SavingAnimation());
    }

    IEnumerator SavingAnimation()
    {
        float fadeInDuration = FindObjectOfType<SceneLoader>().startFadeDuration;
        Tween t = text.DOFade(1f, fadeInDuration).SetEase(Ease.InExpo);
        yield return new WaitWhile(() => t != null && t.IsPlaying());
        for (int i = 0; i < 3; i++)
        {
            text.maxVisibleCharacters = 6;
            yield return new WaitForSecondsRealtime(0.08f);
            text.maxVisibleCharacters = 8;
            yield return new WaitForSecondsRealtime(0.08f);
            text.maxVisibleCharacters = 10;
            yield return new WaitForSecondsRealtime(0.08f);
            text.maxVisibleCharacters = 12;
            yield return new WaitForSecondsRealtime(0.08f);
        }
        t = text.DOFade(0f, 1f);
        yield return new WaitWhile(() => t != null && t.IsPlaying());

        // Disable the whole canvas object at the end.
        canvas.SetActive(false);
    }
}