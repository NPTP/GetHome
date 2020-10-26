using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button firstSelectedButton;
    public GameObject buttons;
    public float buttonsRotRange = 10f;

    // Start is called before the first frame update
    void Start()
    {
        firstSelectedButton.Select();
    }

    void Update()
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 10.0f; //distance of the plane from the camera
        Quaternion lookRotation = Quaternion.LookRotation(Camera.main.ScreenToWorldPoint(screenPoint), Vector3.up);
        Quaternion inverse = Quaternion.Inverse(lookRotation);

        buttons.transform.rotation = Quaternion.Euler(
            Mathf.Clamp(inverse.x, -buttonsRotRange, buttonsRotRange),
            Mathf.Clamp(inverse.y, -buttonsRotRange, buttonsRotRange),
            Mathf.Clamp(inverse.z, -buttonsRotRange, buttonsRotRange)
        );
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}

