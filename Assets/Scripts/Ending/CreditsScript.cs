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
    SceneLoader sceneLoader;

    string[] creditsText;
    bool creditsOver = false;

    public AudioClip finishSound;
    public float textHoldTime = 3f;

    void Awake()
    {
        GameObject textObject = GameObject.Find("Text");
        text = textObject.GetComponent<TMP_Text>();
        text.maxVisibleCharacters = 0;
        textAudio = textObject.GetComponent<AudioSource>();
        finishSoundSource = GameObject.Find("FinishSound").GetComponent<AudioSource>();
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
            creditsOver)
        {
            finishSoundSource.Play();
            GameObject.Instantiate(Resources.Load("ReturnFromLevel"), Vector3.zero, Quaternion.identity);
            GameObject.Find("Scanlines").GetComponent<Image>().DOColor(Color.yellow, sceneLoader.endFadeDuration + 1f);
            sceneLoader.LoadScene(0);
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
    }

    void InitializeCreditsText()
    {
        creditsText = new string[] {
            "GET HOME \n\nA GAME BY RED CASSETTE STUDIOS",
            "RED CASSETTE IS:",
            "Isabel Bowman \nAl Oatridge \nTyler Weston \nNick Perrin",
            "Kayleigh Ward \nAndrew Duong \nRebekah Jaberifard \n Elena Solimine \nJaffer Grisko-Rashid \nAnkush Gogna",
            "Katharine Petkovski \nKeshav Sharma-Jaitly",
            "THANKS FOR PLAYING                                                                                         \n\n\n\nPRESS START"
        };
    }

}
