using UnityEngine;

public class LookUpTutorial : MonoBehaviour
{
    GravityManager gravityManager;
    TutorialScreen lookUpTutorialScreen;

    void Start()
    {
        gravityManager = FindObjectOfType<GravityManager>();
        lookUpTutorialScreen = GameObject.Find("LookUpTutorialScreen").GetComponent<TutorialScreen>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            gravityManager.StopLookingOnPickup("LookUpTutorial");
            lookUpTutorialScreen.StartTutorial();
            Destroy(this.gameObject);
        }
    }
}
