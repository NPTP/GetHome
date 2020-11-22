using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EOLAction : MonoBehaviour, IObjectAction
{
    public void action()
    {
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        FindObjectOfType<SceneLoader>().LoadNextScene();
    }
}
