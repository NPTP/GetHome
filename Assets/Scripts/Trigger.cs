using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Trigger : MonoBehaviour
{
    StateManager stateManager;
    UIManager uiManager;

    public GameObject toChangeObject;

    // public GameObject prompt;

    public bool playerInteractable;

    bool inTrigger;
    // Start is called before the first frame update
    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        uiManager = FindObjectOfType<UIManager>();
        // prompt.SetActive(false);
    }

    // Update is called once per frame
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

    void OnTriggerEnter(Collider player)
    {
        //put prompt on screen
        if (player.tag == "robot" || playerInteractable)
        {
            inTrigger = true;
            uiManager.ShowInteractPrompt();
            // prompt.SetActive(true);
        }

    }

    void OnTriggerExit(Collider player)
    {
        inTrigger = false;
        uiManager.HideInteractPrompt();
        // prompt.SetActive(false);
    }
}
