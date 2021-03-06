﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class DialogBox
{
    public CanvasGroup canvasGroup;
    public TMP_Text header;
    public TMP_Text subtitle1;
    public TMP_Text subtitle2;
    public TMP_Text paragraphs;
    public Image prompt;
    public Animator animator;
    float fadeUpTime = 1f;
    float fadeDownTime = 1f;

    public Tween SetUp()
    {
        header.color = Helper.ChangedAlpha(header.color, 0f);
        subtitle1.color = Helper.ChangedAlpha(header.color, 0f);
        subtitle2.color = Helper.ChangedAlpha(header.color, 0f);
        paragraphs.maxVisibleCharacters = 0;
        prompt.enabled = false;
        prompt.color = Helper.ChangedAlpha(prompt.color, 0);
        return canvasGroup.DOFade(1f, fadeUpTime).From(0f);
    }

    public Tween TearDown()
    {
        header.color = Helper.ChangedAlpha(header.color, 0f);
        subtitle1.color = Helper.ChangedAlpha(header.color, 0f);
        subtitle2.color = Helper.ChangedAlpha(header.color, 0f);
        paragraphs.maxVisibleCharacters = 0;
        prompt.enabled = false;
        prompt.color = Helper.ChangedAlpha(prompt.color, 0);
        return canvasGroup.DOFade(0f, fadeDownTime);
    }

    public void Disable()
    {
        header.color = Helper.ChangedAlpha(header.color, 0f);
        subtitle1.color = Helper.ChangedAlpha(header.color, 0f);
        subtitle2.color = Helper.ChangedAlpha(header.color, 0f);
        paragraphs.maxVisibleCharacters = 0;
        prompt.enabled = false;
        prompt.color = Helper.ChangedAlpha(prompt.color, 0);
        canvasGroup.alpha = 0f;
        animator.SetBool("animate", false);
    }

    public void AddParagraph(Dialog dialog, int paragraphIndex)
    {
        dialog.paragraphs[paragraphIndex] += "\n\n";
        paragraphs.text += dialog.paragraphs[paragraphIndex];
    }

    public void ChangeParagraph(Dialog dialog, int paragraphIndex)
    {
        paragraphs.text = dialog.paragraphs[paragraphIndex];
    }

    public void ShowPrompt()
    {
        prompt.enabled = true;
        StartPromptWaitAnim();
    }

    public void HidePrompt()
    {
        prompt.color = Helper.ChangedAlpha(prompt.color, 0f);
        StopPromptWaitAnim();
        prompt.enabled = false;
    }

    public void StartPromptWaitAnim()
    {
        animator.SetBool("animate", true);
    }

    public void StopPromptWaitAnim()
    {
        animator.SetBool("animate", false);
    }
}


// Class for encapsulating dialogs sent from other classes to be played back.
public class Dialog
{
    public string header;
    public string subtitle1;
    public string subtitle2;
    public string[] paragraphs;
}

// The dialog manager handles all dialogs and sends info to the UI to control it.
// It is called from the Day by any type of trigger, or manual call (e.g. for narration).
public class DialogManager : MonoBehaviour
{
    StateManager stateManager;
    DialogBox dialogBox;
    DialogTextManager dialogTextManager;
    bool dialogNext = false;
    bool dialogFinished = true;
    int charsPerSecond = 60; // Text speed, number of characters that should render in a second

    public bool runTest = false; // DEBUG ONLY

    AudioSource audioSource;
    AudioSource hiss;
    public AudioClip startClip;
    public AudioClip nextClip;
    public AudioClip finishClip;

    void Start()
    {
        stateManager = GameObject.FindObjectOfType<StateManager>();
        audioSource = GetComponents<AudioSource>()[0];
        hiss = GetComponents<AudioSource>()[1];

        dialogBox = new DialogBox();
        dialogBox.canvasGroup = GameObject.Find("DialogBox").GetComponent<CanvasGroup>();
        dialogBox.header = GameObject.Find("DialogBoxHeader").GetComponent<TMP_Text>();
        dialogBox.subtitle1 = GameObject.Find("DialogBoxSubtitle1").GetComponent<TMP_Text>();
        dialogBox.subtitle2 = GameObject.Find("DialogBoxSubtitle2").GetComponent<TMP_Text>();
        dialogBox.paragraphs = GameObject.Find("DialogBoxParagraphs").GetComponent<TMP_Text>();
        dialogBox.prompt = GameObject.Find("DialogBoxPrompt").GetComponent<Image>();
        dialogBox.animator = GameObject.Find("DialogBoxPrompt").GetComponent<Animator>();

        dialogBox.Disable();

        GameObject dialogText = GameObject.FindGameObjectWithTag("DialogText");
        if (dialogText != null)
            dialogTextManager = dialogText.GetComponent<DialogTextManager>();
        else
            dialogTextManager = null;

        if (runTest)
            StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        yield return new WaitForSeconds(2f);

        Dialog d = new Dialog();
        d.header = "Lorem ipsum dolor sit";
        d.subtitle1 = "Consectetur adipiscing elit";
        d.subtitle2 = "Sed do eiusmod tempor incididunt";
        d.paragraphs = new string[] {
            "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. ",
            "Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.",
            "<i>Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?</i>"
        };

        NewDialog(d);
    }

    void Update()
    {
        if (stateManager.GetState() == StateManager.State.Dialog &&
            Input.GetButtonDown("Interact"))
        {
            dialogNext = true;
        }
    }

    // Start a new dialog.
    public void NewDialog(Dialog dialog)
    {
        dialogFinished = false;
        stateManager.SetState(StateManager.State.Dialog);
        StartCoroutine(DialogPlay(dialog));
    }

    public void EndDialog()
    {
        if (stateManager.GetState() == StateManager.State.Dialog && !dialogFinished)
        {
            dialogBox.TearDown();
            dialogFinished = true;
        }
    }

    public void PlayDialog(string id)
    {
        Dialog d = dialogTextManager?.GetDialog(id);
        if (d == null)
        {
            return;
        }
        NewDialog(d);
    }

    IEnumerator DialogPlay(Dialog dialog)
    {
        // STEP 1 : Fade box in
        dialogFinished = false;
        audioSource.PlayOneShot(startClip);
        hiss.Play();
        Tween setup = dialogBox.SetUp();
        //yield return new WaitWhile(() => setup != null && setup.IsPlaying());
        yield return setup.WaitForCompletion();

        // STEP 2 : Fade in the headers one by one.
        dialogBox.header.text = dialog.header;
        dialogBox.header.DOFade(1f, .5f);
        yield return new WaitForSeconds(.25f);
        dialogBox.subtitle1.text = dialog.subtitle1;
        // dialogBox.subtitle1.DOFade(1f, .5f);
        // yield return new WaitForSeconds(.25f);
        dialogBox.subtitle2.text = dialog.subtitle2;
        // dialogBox.subtitle2.DOFade(1f, .5f);
        // yield return new WaitForSeconds(.5f);

        // STEP 3: Show the paragraphs one by one.
        dialogBox.paragraphs.text = "";
        for (int p = 0; p < dialog.paragraphs.Length; p++)
        {
            dialogNext = false;
            // dialogBox.AddParagraph(dialog, p);
            dialogBox.ChangeParagraph(dialog, p);

            dialogBox.paragraphs.maxVisibleCharacters = 0;
            int runningCharCount = 0;

            // for (int i = 0; i <= dialog.paragraphs[p].Length; i++)
            while (dialogBox.paragraphs.maxVisibleCharacters < dialog.paragraphs[p].Length)
            {
                int charAddition = (int)(charsPerSecond * Time.deltaTime);
                if (charAddition < 1)
                    charAddition = 1;
                runningCharCount = runningCharCount + charAddition;
                if (runningCharCount > dialog.paragraphs[p].Length)
                    runningCharCount = dialog.paragraphs[p].Length;

                dialogBox.paragraphs.maxVisibleCharacters = runningCharCount;

                if (dialogNext)
                {
                    dialogBox.paragraphs.maxVisibleCharacters = dialog.paragraphs[p].Length;
                    break;
                }
                yield return null;
            }

            dialogBox.ShowPrompt();
            dialogNext = false;
            yield return new WaitUntil(() => dialogNext);
            if (p < dialog.paragraphs.Length - 1)
                audioSource.PlayOneShot(nextClip, .5f);
            dialogBox.HidePrompt();
        }

        //  for (int line = 0; line < lines.Length; line++)
        // {
        //     dialogNext = false;
        //     string nextLine = lines[line];
        //     uiManager.dialogBox.SetLine(nextLine);
        //     int runningCharCount = 0;
        //     // for (int i = 0; i <= nextLine.Length; i++)
        //     uiManager.dialogBox.tmpText.maxVisibleCharacters = 0;
        //     while (uiManager.dialogBox.tmpText.maxVisibleCharacters < nextLine.Length)
        //     {
        //         int charAddition = (int)(charsPerSecond * Time.deltaTime);
        //         if (charAddition < 1) { charAddition = 1; }
        //         runningCharCount = runningCharCount + charAddition;
        //         if (runningCharCount > nextLine.Length) { runningCharCount = nextLine.Length; }

        //         uiManager.dialogBox.tmpText.maxVisibleCharacters = runningCharCount;

        //         if (skippable && dialogNext)
        //         {
        //             uiManager.dialogBox.tmpText.maxVisibleCharacters = nextLine.Length;
        //             break;
        //         }
        //         // if (i > 0 && nextLine[i - 1] != ' ') { yield return null; }
        //         yield return null;
        //     }
        //     uiManager.dialogBox.ShowPrompt();
        //     dialogNext = false;
        //     yield return new WaitUntil(() => dialogNext);
        //     // yield return null; // Must put a frame between inputs
        //     uiManager.dialogBox.HidePrompt();
        // }

        // STEP 4 : Finish, tear down dialog box, reset state to Normal.
        dialogBox.TearDown();
        dialogFinished = true;
        hiss.Stop();
        audioSource.PlayOneShot(finishClip);
        stateManager.SetState(StateManager.State.Normal);
    }

    public bool IsDialogFinished()
    {
        return dialogFinished;
    }
}