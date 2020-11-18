using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keytrigger : MonoBehaviour
{
    public GameObject toChangeObject;

    public GameObject prompt;

    public ThirdPersonCharacter key;

    bool inTrigger;
    // Start is called before the first frame update
    void Start()
    {
        prompt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(inTrigger){
            //take keypress
            if (Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.E)){
                MonoBehaviour[] list = toChangeObject.gameObject.GetComponents<MonoBehaviour>();
                foreach(MonoBehaviour mb in list)
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

    void OnTriggerEnter(Collider player){
        //put prompt on screen
        if(key.HasKey)
            inTrigger = true;
            prompt.SetActive(true);
        
    }

    void OnTriggerExit(Collider player){
        inTrigger = false;
        prompt.SetActive(false);
    }
}
