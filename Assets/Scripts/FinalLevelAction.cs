using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalLevelAction : MonoBehaviour, IObjectAction
{
    // Start is called before the first frame update

    //ATTACH TO THE TRIGGER
    public FinalForcefieldController forcefield;
    public Material attachedWire;
    private bool isActivated = false;
    private MusicLayerBuilder musicLayerManager;

    void Start(){
        attachedWire.DisableKeyword("_EMISSION");
        musicLayerManager = GameObject.FindGameObjectWithTag("Music").GetComponent<MusicLayerBuilder>();
    }


    public void action(){
        if(!isActivated){
            forcefield.addCompleted();
            attachedWire.EnableKeyword("_EMISSION");
            isActivated = true;
            musicLayerManager.playNextLayer();
        }
    }
}
