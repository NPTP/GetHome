using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keytrigger : MonoBehaviour
{
    StateManager stateManager;
    UIManager uiManager;
    ThirdPersonUserControl thirdPersonUserControl;
    Collider thisCollider;

    public GameObject toChangeObject;
    [HideInInspector]
    public ThirdPersonCharacter keyHolder;
    // public GameObject prompt;

    public string noKeyPromptText = "Need Keycard";
    public string hasKeyPromptText = "Use Keycard";

    bool inTrigger = false;

    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        uiManager = FindObjectOfType<UIManager>();
        thirdPersonUserControl = FindObjectOfType<ThirdPersonUserControl>();
        thirdPersonUserControl.OnSwitchChar += HandleSwitchChar;
        thisCollider = GetComponent<Collider>();

        keyHolder = FindObjectOfType<ThirdPersonCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inTrigger && keyHolder.HasKey && stateManager.GetState() == StateManager.State.Normal)
        {
            //take keypress
            if (Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.E))
            {
                GetComponent<AudioSource>()?.Play();    // Play a sound if one has been added.

                MonoBehaviour[] list = toChangeObject.gameObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mb in list)
                {
                    if (mb is IObjectAction)
                    {
                        IObjectAction actor = (IObjectAction)mb;
                        actor.action();
                        keyHolder.useKey();
                        ExitRange("Player");
                        Destroy(this); // kill the script
                    }
                }
            }
        }
    }

    // On a char switch, manually check intersection with the trigger.
    void HandleSwitchChar(object sender, ThirdPersonUserControl.SwitchCharArgs args)
    {
        GameObject selected = args.selected;
        if (selected.tag != "Player") ExitRange("Player");

        if (selected.tag == "Player" &&
        selected.GetComponent<CapsuleCollider>().bounds.Intersects(thisCollider.bounds))
        {
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

    // void OnTriggerEnter(Collider player)
    // {
    //     //put prompt on screen
    //     if (keyHolder.HasKey)
    //         inTrigger = true;
    //     prompt.SetActive(true);

    // }

    // void OnTriggerExit(Collider player)
    // {
    //     inTrigger = false;
    //     prompt.SetActive(false);
    // }

    void EnterRange(string tag)
    {
        inTrigger = true;
        if (keyHolder.HasKey)
        {
            uiManager.EnterRange(tag, hasKeyPromptText);
        }
        else
        {
            uiManager.EnterRange(tag, noKeyPromptText);
        }
        // prompt.SetActive(true);
    }

    void ExitRange(string tag)
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

