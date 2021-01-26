using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// Sceneloader handles audio fade in/out
public class CreditsScript : MonoBehaviour
{
    float textSpeed = 0.03f;//0.02f;
    TMP_Text text;
    AudioSource textAudio;
    AudioSource finishSoundSource;
    AudioSource musicSource;
    SceneLoader sceneLoader;

    string[] creditsText;
    bool creditsOver = false;
    bool leavingCredits = false;

    public AudioClip finishSound;
    public float textHoldTime = 3f;

    void Awake()
    {
        GameObject textObject = GameObject.Find("Text");
        text = textObject.GetComponent<TMP_Text>();
        text.maxVisibleCharacters = 0;
        textAudio = textObject.GetComponent<AudioSource>();
        finishSoundSource = GameObject.Find("FinishSound").GetComponent<AudioSource>();
        musicSource = GetComponent<AudioSource>();
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    void Start()
    {
        InitializeCreditsText();
        StartCoroutine("Intro");
    }

    void Update()
    {
        if ((Input.GetButtonDown("Interact") ||
            Input.GetButtonDown("Start") ||
            Input.GetKeyDown(KeyCode.Escape)) &&
            creditsOver && !leavingCredits)
        {
            leavingCredits = true;
            finishSoundSource.Play();
            musicSource.DOFade(0f, sceneLoader.endFadeDuration).SetEase(Ease.InOutQuad);
            GameObject.Instantiate(Resources.Load("ReturnFromLevel"), Vector3.zero, Quaternion.identity);
            GameObject.Find("Scanlines").GetComponent<Image>().DOColor(Color.yellow, sceneLoader.endFadeDuration + 1f);
            sceneLoader.LoadSceneByName("MainMenu");
        }
    }

    IEnumerator Intro()
    {
        yield return new WaitForSeconds(sceneLoader.startFadeDuration);

        for (int page = 0; page < creditsText.Length; page++)
        {
            for (int i = 0; i <= creditsText[page].Length; i++)
            {
                text.maxVisibleCharacters = i;
                text.text = creditsText[page];
                if (i > 0 && text.text[i - 1] != ' ')
                    textAudio.Play();
                yield return new WaitForSecondsRealtime(textSpeed);
            }
            if (page < creditsText.Length - 1) { yield return new WaitForSecondsRealtime(textHoldTime); }
        }

        creditsOver = true;
        sceneLoader.fadeAudioInOut = true;
    }

    void InitializeCreditsText()
    {
        creditsText = new string[] {
            "GET HOME \n\nA GAME BY <color=red>RED CASSETTE STUDIOS</color>",
            "<color=red>RED CASSETTE</color> IS:",
            "Development Team:\n Isabel Bowman \nAl Oatridge \nTyler Weston \nNick Perrin",
            "Art Team:\n Kayleigh Ward \nAndrew Duong \nRebekah Jaberifard \n Elena Solimine \nJaffer Grisko-Rashid \nAnkush Gogna",
            "Music/Sound Team:\n Katharine Petkovski \nKeshav Sharma-Jaitly",
            "THANKS FOR PLAYING                                                                                         \n\n\n\n<color=green>PRESS START</color>"
        };
    }

}
