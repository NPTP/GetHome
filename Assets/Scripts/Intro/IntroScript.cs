using System.Collections;
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
    private bool breakFlag;

    void Awake()
    {
        GameObject textObject = GameObject.Find("Text");
        text = textObject.GetComponent<TMP_Text>();
        text.maxVisibleCharacters = 0;
        textAudio = textObject.GetComponent<AudioSource>();
        sceneLoader = FindObjectOfType<SceneLoader>();
        breakFlag = false;
    }

    void Start()
    {
        StartCoroutine("Intro");
    }

    private void Update()
    {
        if (Input.GetButtonDown("Start") || Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            breakFlag = true;
        }
    }

    IEnumerator Intro()
    {
        float timer = 0;
        while (timer < 3.0f)
        {
            timer += Time.deltaTime;
            yield return new WaitForSecondsRealtime(0.1f);
            if (breakFlag) break;
        }

        for (int i = 0; i <= text.text.Length; i++)
        {
            if (breakFlag) break;
            text.maxVisibleCharacters = i;
            if (i > 0 && text.text[i - 1] != ' ')
                textAudio.Play();
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        text.maxVisibleCharacters = text.text.Length;

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
