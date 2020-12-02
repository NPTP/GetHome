using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Trigger : MonoBehaviour
{
    StateManager stateManager;
    UIManager uiManager;
    ThirdPersonUserControl thirdPersonUserControl;
    Collider thisCollider;

    public GameObject toChangeObject;
    public GameObject triggerEffects;
    public bool persist = true;
    private AudioSource audios;

    bool inTrigger;
    // public GameObject prompt;

    [Header("Who can interact with this trigger? Pick one only.")]
    public bool humanCanInteract = false;
    public bool robotCanInteract = false;
    [Header("What text (if any) will show on this trigger's UI prompt?")]
    public string interactText = "";
    string interactableTag = "";

    void Start()
    {
        // prompt.SetActive(false);

        stateManager = FindObjectOfType<StateManager>();
        uiManager = FindObjectOfType<UIManager>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;

        thisCollider = GetComponent<Collider>();
        audios = GetComponent<AudioSource>();


        if (humanCanInteract && robotCanInteract)
        {
            interactableTag = "Player";
        }
        else if (humanCanInteract)
        {
            interactableTag = "Player";
        }
        else if (robotCanInteract)
        {
            interactableTag = "robot";
        }
        else
        {

        }
    }

    void Update()
    {
        if (inTrigger && stateManager.GetState() == StateManager.State.Normal)
        {
            //take keypress
            if (Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.E))
            {
                if (!toChangeObject)    // if we don't have an object, don't do anything
                {
                    return;
                }

                //GetComponent<AudioSource>()?.Play();    // Play a sound if one has been added.
                if (audios) audios.Play();

                MonoBehaviour[] list = toChangeObject.gameObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mb in list)
                {
                    if (mb is IObjectAction)
                    {
                        IObjectAction actor = (IObjectAction)mb;
                        actor.action();
                    }
                }

                if (!persist)
                {
                    StartCoroutine("destroyTrigger");
                }
            }
        }
    }

    IEnumerator destroyTrigger()
    {
        if (audios)
            yield return new WaitForSecondsRealtime(audios.clip.length);
        if (triggerEffects)
            Destroy(triggerEffects);
        ExitRange(interactableTag);
        Destroy(this.gameObject);
    }

    // On a char switch, manually check intersection with the trigger.
    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        GameObject selected = args.selected;
        if (selected.tag == "Player") ExitRange("robot");
        else ExitRange("Player");

        if (selected.tag == interactableTag &&
        selected.GetComponent<CapsuleCollider>().bounds.Intersects(thisCollider.bounds))
        {
            EnterRange(selected.tag);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!inTrigger && other.tag == interactableTag &&
            stateManager.GetSelected() == other.gameObject)
        {
            EnterRange(other.tag);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (inTrigger && other.tag == interactableTag)
        {
            ExitRange(other.tag);
        }
    }

    void EnterRange(string tag)
    {
        inTrigger = true;
        uiManager.EnterRange(tag, interactText);
        // prompt.SetActive(true);
    }

    public void ExitRange(string tag)
    {
        inTrigger = false;
        uiManager.ExitRange(tag);
        // prompt.SetActive(false);
    }

    void OnDestroy()
    {
        thirdPersonUserControl.OnSwitchChar -= HandleSwitchChar;
    }
}
