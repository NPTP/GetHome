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
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.transform.SetParent(elevator, true);
            GameObject robot = GameObject.FindWithTag("robot");
            robot.GetComponent<RobotBuddy>().StopMoving();
            if (robotInTrigger)
            {
                robot.GetComponent<Rigidbody>().isKinematic = true;
                robot.transform.SetParent(elevator, true);
            }
            audioSource.Play();
            elevator.DOMoveY(elevator.position.y - 50, 50);
        }
        FindObjectOfType<SceneLoader>().LoadNextScene();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "robot")
            robotInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "robot")
            robotInTrigger = false;
    }
}
