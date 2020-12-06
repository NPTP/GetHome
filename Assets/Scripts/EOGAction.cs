using UnityEngine;
using DG.Tweening;
using System.Collections;

public class EOGAction : MonoBehaviour, IObjectAction
{
    bool robotInTrigger = false;
    private AudioSource audios;
    StateManager stateManager;

    [SerializeField] GameObject endEffect;

    void Start()
    {
        audios = GetComponent<AudioSource>();
        stateManager = FindObjectOfType<StateManager>();
    }

    public void action()
    {
        StartCoroutine("launchTrigger");
        //GetComponent<AudioSource>()?.Play();
        //FindObjectOfType<SceneLoader>().LoadNextScene();
    }

    IEnumerator launchTrigger()
    {
        GameObject.FindGameObjectWithTag("Music").GetComponent<MusicLayerBuilder>().playLastHit();
        float effectScale = 12.5f;
        GameObject effect = GameObject.Instantiate(endEffect, transform.position, transform.rotation);
        effect.transform.localScale = effect.transform.localScale * effectScale;

        if (audios) audios.Play();
        yield return null;
        // if (audios)
        //     yield return new WaitForSecondsRealtime(audios.clip.length);

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
