using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettableObject : MonoBehaviour
{
    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter
    private GameObject childFx;
    private Renderer thisrenderer;
    public string ItemName;
    private AudioSource audios;
    // check to make sure we only play the keycard sound once
    private bool hasPlayed;
    private void Start()
    {
        // get the third person character ( this should never be null due to require component )
        m_Character = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonCharacter>();
        childFx = this.transform.GetChild(0).gameObject;
        audios = GetComponent<AudioSource>();
        thisrenderer = GetComponent<Renderer>();
        hasPlayed = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            thisrenderer.enabled = false;
            childFx.SetActive(false);
            // TODO: player has now collected something!
            m_Character.GetItem(ItemName);
            StartCoroutine("WaitDestroy");
        }
    }

    IEnumerator WaitDestroy()
    {
        float tlength = audios.clip.length;
        if (!hasPlayed)
        {
            audios.Play();
            hasPlayed = true;
        }
        yield return new WaitForSeconds(tlength);
        Destroy(gameObject);
    }
}
