using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialScreen : MonoBehaviour
{
    StateManager stateManager;

    GameObject topCanvas;
    CanvasGroup topCanvasGroup;
    TMP_Text header;
    TMP_Text tutorialText1;
    CanvasGroup tutorialText2;
    CanvasGroup tutorialText3;
    RectTransform itemContainer;
    Vector3 savedItemScale;
    GameObject prompt;
    Animator promptAnimator;

    AudioSource audioSource;
    public AudioClip start;
    public AudioClip end;

    bool allowContinue = false;
    bool rotating = false; // flag for when animation should be allowed to keep playing

    public string tutorialName = "Tutorial";

    void Awake()
    {
        stateManager = FindObjectOfType<StateManager>();

        GameObject.Find(tutorialName + "Canvas").GetComponent<Canvas>().worldCamera = FindObjectOfType<Camera>();
        audioSource = GetComponent<AudioSource>();
        topCanvasGroup = GameObject.Find(tutorialName + "Canvas").GetComponent<CanvasGroup>();
        topCanvasGroup.alpha = 0;

        header = GameObject.Find(tutorialName + "Header").GetComponent<TMP_Text>();
        header.enabled = false;
        tutorialText1 = GameObject.Find(tutorialName + "Text1").GetComponent<TMP_Text>();
        tutorialText1.enabled = false;
        tutorialText2 = GameObject.Find(tutorialName + "Text2").GetComponent<CanvasGroup>();
        tutorialText2.alpha = 0;
        tutorialText3 = GameObject.Find(tutorialName + "Text3")?.GetComponent<CanvasGroup>();
        if (tutorialText3)
        {
            tutorialText3.alpha = 0;
        }
        itemContainer = GameObject.Find(tutorialName + "ItemContainer").GetComponent<RectTransform>();
        savedItemScale = itemContainer.localScale;
        itemContainer.localScale = Vector3.zero;
        prompt = GameObject.Find(tutorialName + "Prompt");
        promptAnimator = GameObject.Find(tutorialName + "Prompt").GetComponent<Animator>();
        prompt.SetActive(false);

        topCanvas = GameObject.Find(tutorialName + "Canvas");
        topCanvas.SetActive(false);
    }

    void Update()
    {
        if (allowContinue && Input.GetButtonDown("Interact"))
        {
            StartCoroutine(EndTutorial());
        }
    }

    public void StartTutorial()
    {
        StartCoroutine(TutorialProcess());
    }

    IEnumerator TutorialProcess()
    {
        stateManager?.SetState(StateManager.State.Inert);
        prompt.SetActive(true);
        promptAnimator.ResetTrigger("FadeIn");
        topCanvas.SetActive(true);

        audioSource.PlayOneShot(start);

        Tween t = topCanvasGroup.DOFade(1f, 1f);
        yield return new WaitWhile(() => t != null && t.IsPlaying());
        header.enabled = true;
        header.DOFade(1f, .5f).From(0f);
        yield return new WaitForSeconds(.75f);
        rotating = true;
        itemContainer.DOScale(savedItemScale, .5f);
        StartCoroutine(RotateItem());
        yield return new WaitForSeconds(.75f);
        tutorialText1.enabled = true;
        tutorialText1.DOFade(1f, .5f).From(0f);
        yield return new WaitForSeconds(.75f);
        tutorialText2.DOFade(1f, .5f);
        yield return new WaitForSeconds(1.5f);
        if (tutorialText3)
        {
            tutorialText3.DOFade(1f, .5f);
            yield return new WaitForSeconds(1.5f);
        }

        promptAnimator.SetTrigger("FadeIn");
        allowContinue = true;
    }

    IEnumerator EndTutorial()
    {
        allowContinue = false; // Stop from pressing prompt multiple time
        rotating = false;
        promptAnimator.ResetTrigger("FadeIn");
        prompt.SetActive(false);

        audioSource.PlayOneShot(end);

        itemContainer.DOScale(0f, .25f);
        yield return new WaitForSeconds(.25f);
        header.DOFade(0f, .25f);
        tutorialText1.DOFade(0f, .25f);
        tutorialText2.DOFade(0f, .25f);
        if (tutorialText3) tutorialText3.DOFade(0f, .25f);
        yield return new WaitForSeconds(.5f);
        Tween t = topCanvasGroup.DOFade(0f, .5f);
        yield return new WaitWhile(() => t != null && t.IsPlaying());
        rotating = false;
        topCanvas.SetActive(false);
        stateManager?.SetState(StateManager.State.Normal);
    }

    IEnumerator RotateItem()
    {
        while (rotating)
        {
            itemContainer.Rotate(0f, 60 * Time.deltaTime, 0f, Space.Self);
            yield return null;
        }
    }

}
