using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Prompt
{
    public GameObject character;
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Image imageBG;
    public Image image;
    public TMP_Text text;
    public Tween fadeTween;
    public bool inRange = false;
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

    Prompt playerPrompt = new Prompt();
    bool playerInRange = false;
    Prompt robotPrompt = new Prompt();
    bool robotInRange = false;

    float promptFadeTime = 0.25f;
    float promptLerpMultiplier = 15f;

    ThirdPersonUserControl thirdPersonUserControl;
    GameObject selected;

    public UIResources uiResources;

    void Awake()
    {
        stateManager = FindObjectOfType<StateManager>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;
        InitializePrompt(playerPrompt, "PlayerPrompt");
        InitializePrompt(robotPrompt, "RobotPrompt");
    }

    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        selected = args.selected;
        string unselectedTag = selected.tag == "Player" ? "robot" : "Player";
        SetInRange(unselectedTag, false);
    }

    // ████████████████████████████████████████████████████████████████████████
    // ███ ENTER/EXIT RANGE
    // ████████████████████████████████████████████████████████████████████████

    public void EnterRange(string tag, string prompText = "")
    {
        SetInRange(tag, true);

        Prompt prompt;
        if (tag == "Player") prompt = playerPrompt;
        else prompt = robotPrompt;

        prompt.text.text = prompText;

        if (!prompt.inRange)
        {
            prompt.inRange = true;
            StopCoroutine("AlighPromptOutOfRange");
            StartCoroutine("AlignPromptInRange", prompt);
        }
    }

    IEnumerator AlignPromptInRange(Prompt prompt)
    {
        if (prompt.fadeTween != null) prompt.fadeTween.Kill();
        prompt.fadeTween = prompt.Show();

        selected = stateManager.GetSelected();
        bool firstFrameAligned = false;
        while (GetInRange(prompt.character.tag))
        {
            Vector3 pos = GetPromptPosition(prompt.character);
            if (!firstFrameAligned)
            {
                prompt.rectTransform.position = pos;
                firstFrameAligned = true;
            }
            else
            {
                prompt.rectTransform.position = Vector3.Lerp(prompt.rectTransform.position, pos, promptLerpMultiplier * Time.deltaTime);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void ExitRange(string tag)
    {
        SetInRange(tag, false);

        Prompt prompt;
        if (tag == "Player") prompt = playerPrompt;
        else prompt = robotPrompt;

        prompt.inRange = false;

        StopCoroutine("AlignPromptInRange");
        StartCoroutine("AlignPromptOutOfRange", prompt);
    }

    IEnumerator AlignPromptOutOfRange(Prompt prompt)
    {
        if (prompt.fadeTween != null) prompt.fadeTween.Kill();
        prompt.fadeTween = prompt.canvasGroup.DOFade(0f, promptFadeTime).From(prompt.canvasGroup.alpha);

        while (prompt.fadeTween.IsActive())
        {
            Vector3 pos = GetPromptPosition(prompt.character);
            prompt.rectTransform.position = Vector3.Lerp(prompt.rectTransform.position, pos, promptLerpMultiplier * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        prompt.Hide();
    }

    // Get screenspace position at 2 x the height of the given character.
    Vector3 GetPromptPosition(GameObject character)
    {
        return Camera.main.WorldToScreenPoint(character.transform.position +
                character.transform.TransformVector(new Vector3(
                    0f, 1.5f * character.GetComponent<CapsuleCollider>().height, 0f)));
    }

    void SetInRange(string tag, bool value)
    {
        if (tag == "Player") playerInRange = value;
        else robotInRange = value;
    }

    bool GetInRange(string tag)
    {
        if (tag == "Player") return playerInRange;
        else return robotInRange;
    }

    // ████████████████████████████████████████████████████████████████████████
    // ███ INITIALIZERS / DESTROYERS
    // ████████████████████████████████████████████████████████████████████████

    void InitializePrompt(Prompt prompt, string gameobjectName)
    {
        if (gameobjectName == "PlayerPrompt") prompt.character = GameObject.FindWithTag("Player");
        else prompt.character = GameObject.FindWithTag("robot");

        GameObject pgo = GameObject.Find(gameobjectName);
        prompt.rectTransform = pgo.GetComponent<RectTransform>();
        prompt.canvasGroup = pgo.GetComponent<CanvasGroup>();
        prompt.imageBG = pgo.transform.GetChild(0).gameObject.GetComponent<Image>();
        prompt.imageBG.enabled = false;
        prompt.image = pgo.transform.GetChild(1).gameObject.GetComponent<Image>();
        prompt.image.enabled = false;
        prompt.text = pgo.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        prompt.text.enabled = false;

        prompt.image.sprite = uiResources.A_Button;
        prompt.canvasGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        thirdPersonUserControl.OnSwitchChar -= HandleSwitchChar;
    }

}
