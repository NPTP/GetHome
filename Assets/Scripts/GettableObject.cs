using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettableObject : MonoBehaviour
{
    private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter
    private void Start()
    {
        // get the third person character ( this should never be null due to require component )
        m_Character = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonCharacter>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // TODO: player has now collected something!
            m_Character.HasKey = true;
            Destroy(gameObject);

        }
    }
}
