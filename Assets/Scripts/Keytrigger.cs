using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keytrigger : MonoBehaviour
{
    public GameObject toChangeObject;

    public GameObject prompt;

    public bool key = false;

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
            if (Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.E)){
                MonoBehaviour[] list = toChangeObject.gameObject.GetComponents<MonoBehaviour>();
                foreach(MonoBehaviour mb in list)
                {
                    if (mb is IObjectAction)
                    {
                        if(key){
                            IObjectAction actor = (IObjectAction)mb;
                            actor.action();
                        }
                    }
                 }
            }
        }
    }

    void OnTriggerEnter(Collider player){
        //put prompt on screen
        if(player.name.Equals("Robot"))
            inTrigger = true;
            prompt.SetActive(true);
        
    }

    void OnTriggerExit(Collider player){
        inTrigger = false;
        prompt.SetActive(false);
    }

    public void setKey(){
        key = true;
    }
}
