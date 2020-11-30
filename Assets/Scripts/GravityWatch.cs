using UnityEngine;

public class GravityWatch : MonoBehaviour
{
    GravityManager gravityManager;
    GameObject gwatch;

    void Awake()
    {
        gravityManager = FindObjectOfType<GravityManager>();
        gwatch = GameObject.Find("GetHomewatch");
        gwatch.SetActive(false);
    }

    void Start()
    {
        gravityManager.haveGravWatch = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            gwatch.SetActive(true);
            gravityManager.haveGravWatch = true;
            FindObjectOfType<TutorialScreen>().StartTutorial();
            // Destroy(this.gameObject);

            // Trying this in case the destroy of the prefab hurts the tutorial
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<SphereCollider>().enabled = false;
        }
    }
}
