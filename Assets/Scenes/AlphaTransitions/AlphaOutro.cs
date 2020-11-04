using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AlphaOutro : MonoBehaviour
{
    int numTexts;
    Animator[] textAnimators;
    Animator promptAnimator;
    Animator transitionAnimator;
    AudioSource audioSource;
    bool readyToStart = false;

    // Start is called before the first frame update
    void Start()
    {
        Transform textParent = GameObject.Find("TextParent").transform;
        numTexts = textParent.childCount;
        textAnimators = new Animator[numTexts];
        for (int i = 0; i < numTexts; i++)
        {
            textAnimators[i] = textParent.GetChild(i).gameObject.GetComponent<Animator>();
        }
        promptAnimator = GameObject.Find("Prompt").GetComponent<Animator>();
        transitionAnimator = GameObject.Find("Transition").GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < numTexts; i++)
        {
            textAnimators[i].SetTrigger("FadeIn");
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(1f);

        promptAnimator.SetTrigger("FadeIn");
        readyToStart = true;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && readyToStart)
        {
            audioSource.Play();
            promptAnimator.gameObject.SetActive(false);
            StartCoroutine(ExitIntro());
        }
    }

    IEnumerator ExitIntro()
    {
        transitionAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
    }
}
