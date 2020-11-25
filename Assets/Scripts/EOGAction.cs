using UnityEngine;
using DG.Tweening;

public class EOGAction : MonoBehaviour, IObjectAction
{
    public AudioSource audioSource = null; // Add this in the inspector.

    bool robotInTrigger = false;

    public void action()
    {
        audioSource.Play();
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
