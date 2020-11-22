﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// Sceneloader handles audio fade in/out
public class IntroScript : MonoBehaviour
{
    float textSpeed = 0.03f;//0.02f;
    TMP_Text text;
    AudioSource textAudio;
    SceneLoader sceneLoader;

    void Awake()
    {
        GameObject textObject = GameObject.Find("Text");
        text = textObject.GetComponent<TMP_Text>();
        text.maxVisibleCharacters = 0;
        textAudio = textObject.GetComponent<AudioSource>();
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    void Start()
    {
        StartCoroutine("Intro");
    }

    IEnumerator Intro()
    {
        yield return new WaitForSecondsRealtime(5f);
        for (int i = 0; i <= text.text.Length; i++)
        {
            text.maxVisibleCharacters = i;
            if (i > 0 && text.text[i - 1] != ' ')
                textAudio.Play();
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        GameObject.Find("Scanlines").GetComponent<Image>().DOColor(Color.yellow, sceneLoader.endFadeDuration + 1f);
        yield return new WaitForSecondsRealtime(1f);
        sceneLoader.LoadNextScene();
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
