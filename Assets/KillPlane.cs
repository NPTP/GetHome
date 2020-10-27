using UnityEngine;
using UnityEngine.SceneManagement;

// Ensure mesh collider is convex for plane
public class KillPlane : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
