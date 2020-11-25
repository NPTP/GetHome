using UnityEngine;
using DG.Tweening;

public class EOGAction : MonoBehaviour, IObjectAction
{
    bool robotInTrigger = false;

    public void action()
    {
        GetComponent<AudioSource>()?.Play();
        FindObjectOfType<SceneLoader>().LoadNextScene();
    }

    void OnTriggerEnter(Collider other)
    {
        robotInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        robotInTrigger = false;
    }
}
