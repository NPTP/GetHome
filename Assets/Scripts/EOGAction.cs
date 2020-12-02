using UnityEngine;
using DG.Tweening;
using System.Collections;

public class EOGAction : MonoBehaviour, IObjectAction
{
    bool robotInTrigger = false;
    private AudioSource audios;

    void Start()
    {
        audios = GetComponent<AudioSource>();
    }

    public void action()
    {
        StartCoroutine("launchTrigger");
        //GetComponent<AudioSource>()?.Play();
        //FindObjectOfType<SceneLoader>().LoadNextScene();
    }

    IEnumerator launchTrigger()
    {
        if (audios) audios.Play();

        if (audios)
            yield return new WaitForSecondsRealtime(audios.clip.length);
        FindObjectOfType<SceneLoader>().LoadNextScene();

    }

    void OnTriggerEnter(Collider other)
    {
        robotInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        robotInTrigger = false;
    }
}
