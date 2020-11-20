using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keytrigger_Update : MonoBehaviour
{
    StateManager stateManager;
    UIManager uiManager;
    ThirdPersonUserControl thirdPersonUserControl;
    Collider thisCollider;

    public GameObject toChangeObject;
    public ThirdPersonCharacter key;
    bool inTrigger;
    // public GameObject prompt;

    [Header("What text (if any) will show on this key trigger's UI prompt?")]
    public string interactText = "";

    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        uiManager = FindObjectOfType<UIManager>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;
        thisCollider = GetComponent<Collider>();
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

    // On a char switch, manually check intersection with the trigger.
    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        GameObject selected = args.selected;
        if (selected.tag == "Player") ExitRange("robot");
        else ExitRange("Player");

        if (selected.tag == "Player" &&
        selected.GetComponent<CapsuleCollider>().bounds.Intersects(thisCollider.bounds))
        {
            print("Got next to enter range for: " + selected.tag);
            EnterRange(selected.tag);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!inTrigger && other.tag == "Player" &&
            stateManager.GetSelected() == other.gameObject)
        {
            EnterRange(other.tag);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (inTrigger && other.tag == "Player")
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

    void ExitRange(string tag)
    {
        inTrigger = false;
        uiManager.ExitRange(tag);
        // prompt.SetActive(false);
    }
}
