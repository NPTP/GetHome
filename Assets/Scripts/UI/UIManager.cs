using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Prompt
{
    public RectTransform rectTransform;
    public Image image;
    public TMP_Text text;
    public Tween currentTween;
    float fadeTime = 0.1f;

    public Tween Show()
    {
        image.enabled = true;
        text.enabled = true;
        image.color = Helper.ChangedAlpha(image.color, 0);
        text.DOFade(1f, fadeTime);
        return image.DOFade(1f, fadeTime);
    }

    public Tween FadeOut()
    {
        text.DOFade(0f, fadeTime);
        return image.DOFade(0f, fadeTime);
    }

    public void Hide()
    {
        image.enabled = false;
        text.enabled = false;
    }
}

public class UIManager : MonoBehaviour
{
    Prompt interactPrompt = new Prompt();

    void Start()
    {
        // interactPrompt.rectTransform = 
        // interactPrompt.image = 
        // interactPrompt.text = 
    }

    public void ShowInteractPrompt()
    {

    }

    public void HideInteractPrompt()
    {

    }
    
}
