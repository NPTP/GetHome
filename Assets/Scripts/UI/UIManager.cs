using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Prompt
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Image imageBG;
    public Image image;
    public TMP_Text text;
    public Tween fadeTween;
    float fadeTime = 0.2f;

    public Tween Show()
    {
        imageBG.enabled = true;
        image.enabled = true;
        text.enabled = true;
        return canvasGroup.DOFade(1f, fadeTime);
    }

    public Tween FadeOut()
    {
        return canvasGroup.DOFade(0f, fadeTime);
    }

    public void Hide()
    {
        imageBG.enabled = false;
        image.enabled = false;
        text.enabled = false;
    }
}

// ************************************

public class UIManager : MonoBehaviour
{
    StateManager stateManager;

    Prompt interactPrompt = new Prompt();
    float promptVerticalOffset = 75;
    float promptFadeTime = 0.25f;
    bool interactInRange = false;

    ThirdPersonUserControl thirdPersonUserControl;
    GameObject selected;

    public UIResources uiResources;

    void Awake()
    {
        stateManager = FindObjectOfType<StateManager>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;
        InitializePrompt(interactPrompt, "InteractPrompt");
    }

    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        selected = args.selected;
        interactInRange = false;
    }

    // ████████████████████████████████████████████████████████████████████████
    // ███ ENTER/EXIT RANGE
    // ████████████████████████████████████████████████████████████████████████

    public void EnterRange(string prompText = "")
    {
        interactInRange = true;
        interactPrompt.text.enabled = true;
        interactPrompt.text.text = prompText;

        StopCoroutine("AlignPromptOutOfRange");
        StartCoroutine("AlignPromptInRange", interactPrompt);
    }

    IEnumerator AlignPromptInRange(Prompt prompt)
    {
        if (prompt.fadeTween != null) prompt.fadeTween.Kill();
        prompt.fadeTween = prompt.Show();

        selected = stateManager.GetSelected();
        while (interactInRange)
        {
            Vector3 pos = GetPromptPosition(selected);
            pos.y += promptVerticalOffset; // Adjust as necessary to get above player's head.
            prompt.rectTransform.position = pos;
            yield return new WaitForFixedUpdate();
        }
    }

    public void ExitRange()
    {
        interactInRange = false;

        StopCoroutine("AlignPromptInRange");
        StartCoroutine("AlignPromptOutOfRange", interactPrompt);
    }

    IEnumerator AlignPromptOutOfRange(Prompt prompt)
    {
        if (prompt.fadeTween != null) prompt.fadeTween.Kill();
        prompt.fadeTween = prompt.canvasGroup.DOFade(0f, promptFadeTime).From(prompt.canvasGroup.alpha);

        while (prompt.fadeTween != null & prompt.fadeTween.IsPlaying())
        {
            Vector3 pos = GetPromptPosition(selected);
            pos.y += promptVerticalOffset; // Adjust as necessary to get above player's head.
            prompt.rectTransform.position = pos;
            yield return new WaitForFixedUpdate();
        }

        prompt.Hide();
    }

    // Always just at the currently selected character's head
    Vector3 GetPromptPosition(GameObject selected)
    {
        return Camera.main.WorldToScreenPoint(selected.transform.position +
                selected.transform.TransformVector(new Vector3(
                    0f, selected.GetComponent<CapsuleCollider>().height, 0f)));
    }

    // ████████████████████████████████████████████████████████████████████████
    // ███ INITIALIZERS
    // ████████████████████████████████████████████████████████████████████████


    void InitializePrompt(Prompt prompt, string gameobjectName)
    {
        GameObject pgo = GameObject.Find(gameobjectName);
        prompt.rectTransform = pgo.GetComponent<RectTransform>();
        prompt.canvasGroup = pgo.GetComponent<CanvasGroup>();
        prompt.imageBG = pgo.transform.GetChild(0).gameObject.GetComponent<Image>();
        prompt.imageBG.enabled = false;
        prompt.image = pgo.transform.GetChild(1).gameObject.GetComponent<Image>();
        prompt.image.enabled = false;
        prompt.text = pgo.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        prompt.text.enabled = false;

        if (gameobjectName == "InteractPrompt") prompt.image.sprite = uiResources.A_Button;
        prompt.canvasGroup.alpha = 0f;
    }

}
