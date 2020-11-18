using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum InteractableCharacter
{
    Human,
    Robot
}

public class Trigger : MonoBehaviour
{
    StateManager stateManager;
    UIManager uiManager;

    public GameObject toChangeObject;
    bool inTrigger;
    // public GameObject prompt;

    public InteractableCharacter interactableCharacter;
    public string interactText = "Interact";
    string interactableTag;

    void Start()
    {
        // prompt.SetActive(false);

        stateManager = FindObjectOfType<StateManager>();
        uiManager = FindObjectOfType<UIManager>();

        if (interactableCharacter == InteractableCharacter.Human)
            interactableTag = "Player";
        else
            interactableTag = "robot";
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

                MonoBehaviour[] list = toChangeObject.gameObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mb in list)
                {
                    if (mb is IObjectAction)
                    {
                        IObjectAction actor = (IObjectAction)mb;
                        actor.action();
                    }
                }
            }
        }
    }

    // TODO: catch the char switch event and check who's in the bounds of the trigger
    // at the time of the switch.

    void OnTriggerEnter(Collider other)
    {
        //put prompt on screen
        if (other.tag == interactableTag &&
            stateManager.GetSelected() == other.gameObject)
        {
            inTrigger = true;
            uiManager.EnterRange(interactText);
            // prompt.SetActive(true);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == interactableTag &&
            stateManager.GetSelected() == other.gameObject)
        {
            inTrigger = false;
            uiManager.ExitRange();
            // prompt.SetActive(false);
        }
    }
}
