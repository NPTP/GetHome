using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Image background;
    public Image earth;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AnimateBackground());
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator AnimateBackground()
    {
        yield return null;
    }
}
