using UnityEngine;
using DG.Tweening;

public class EOLAction : MonoBehaviour, IObjectAction
{
    public Transform elevator = null; // Add this in the inspector.
    public AudioSource audioSource = null; // Add this in the inspector.

    bool robotInTrigger = false;

    public void action()
    {
        if (elevator != null && audioSource != null)
        {
            GameObject.FindWithTag("Player").transform.SetParent(elevator);
            if (robotInTrigger)
                GameObject.FindWithTag("robot").transform.SetParent(elevator);
            audioSource.Play();
            elevator.DOMoveY(elevator.position.y - 50, 50);
        }
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
