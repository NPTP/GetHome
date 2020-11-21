using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class IntroScript : MonoBehaviour
{
    AudioSource audioSource;
    float textSpeed = 0.02f;
    TMP_Text text;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        text = FindObjectOfType<TMP_Text>();
        text.maxVisibleCharacters = 0;
    }

    void Start()
    {
        audioSource.volume = 0;
        StartCoroutine("Intro");
    }

    IEnumerator Intro()
    {
        audioSource.DOFade(1f, 5f);
        yield return new WaitForSecondsRealtime(5f);
        for (int i = 0; i <= text.text.Length; i++)
        {
            text.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        yield return new WaitForSecondsRealtime(1f);
        audioSource.DOFade(0f, 1.8f);
        FindObjectOfType<SceneLoader>().LoadNextScene(2f);
    }

}

// / >> WARNING                                
// / >> CRITICAL FAILURE IN:
//      CRYOPOD SYSTEMS CONTAINMENT 4-04                                
// / >> ATTEMPTING RECOVERY                                
// / >> PLEASE WAIT ...                                
// / ...                                
// / ...                                
// / ...                                
// / CSC 4-04 recovery procedure failed.                                
// / Life support inactive.                                
// / Ship guidance system disabled.                                
// / Automated repair unit disengaged.                                
// / Waking passenger...                                                                
