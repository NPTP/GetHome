using UnityEngine;

public class GravityWatch : MonoBehaviour
{
    GravityManager gravityManager;

    void Awake()
    {
        gravityManager = FindObjectOfType<GravityManager>();
    }

    void Start()
    {
        gravityManager.haveGravWatch = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            gravityManager.haveGravWatch = true;
            FindObjectOfType<TutorialScreen>().StartTutorial();
            Destroy(this.gameObject);
        }
    }
}
