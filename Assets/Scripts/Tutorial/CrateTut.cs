using UnityEngine;

public class CrateTut : MonoBehaviour
{
    GravityManager gravityManager;
    TutorialScreen crateTutorialScreen;

    void Start()
    {
        gravityManager = FindObjectOfType<GravityManager>();
        crateTutorialScreen = GameObject.Find("CrateTutorial").GetComponent<TutorialScreen>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            gravityManager.StopLookingOnPickup("LookUpTutorial");
            crateTutorialScreen.StartTutorial();
            Destroy(this.gameObject);
        }
    }
}