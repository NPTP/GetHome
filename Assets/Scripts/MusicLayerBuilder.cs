using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLayerBuilder : MonoBehaviour
{
    public AudioClip[] layers;
    public AudioClip lastHit;
    private AudioSource audios;
    private int currentLayer = 0;

    void Start()
    {
        audios = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void playNextLayer()
    {
        currentLayer++;
        float playLoc = audios.time;
        audios.Stop();
        audios.clip = layers[currentLayer];
        audios.time = playLoc;
        audios.Play();
        
    }

    public void playLastHit()
    {
        audios.Stop();
        audios.clip = lastHit;
        audios.time = 9.0f;
        audios.Play();
    }
}
