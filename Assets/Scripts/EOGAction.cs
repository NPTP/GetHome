using UnityEngine;
using DG.Tweening;
using System.Collections;

public class EOGAction : MonoBehaviour, IObjectAction
{
    bool robotInTrigger = false;
    private AudioSource audios;
    StateManager stateManager;
    SceneLoader sceneLoader;

    EndingFX endingFX;

    void Start()
    {
        audios = GetComponent<AudioSource>();
        stateManager = FindObjectOfType<StateManager>();
        endingFX = FindObjectOfType<EndingFX>();
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    public void action()
    {
        StartCoroutine("launchTrigger");
        //GetComponent<AudioSource>()?.Play();
        //FindObjectOfType<SceneLoader>().LoadNextScene();
    }

    IEnumerator launchTrigger()
    {
        stateManager.SetState(StateManager.State.Inert);

        GameObject.FindGameObjectWithTag("Music").GetComponent<MusicLayerBuilder>().playLastHit();

        if (audios) audios.Play();
        yield return null;

        endingFX.EngageFX();
        yield return new WaitForSeconds(sceneLoader.endFadeDuration);

        sceneLoader.LoadNextScene();
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
