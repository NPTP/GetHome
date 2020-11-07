using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int index = SceneManager.GetActiveScene().buildIndex;   // Get the index of this current scene
        PlayerPrefs.SetInt("checkpoint", index);                // save it to our player prefs
    }
}
