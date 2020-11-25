using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckKey : MonoBehaviour
{
    public ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter
    public GameObject LinkedObject;
    private void Start()
    {
        // get the third person character ( this should never be null due to require component )
        // m_Character = GetComponent<ThirdPersonCharacter>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // TODO: player has now collected something!
            if (m_Character.HasKey)
            {
                LinkedObject.SetActive(false);
                m_Character.useKey();
            }
        }
    }
}
