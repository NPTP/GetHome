using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note that NoFlipZones are on their own layer which ONLY collides with player & robot - nothing else.
// So we do not have to check tags on entering triggers, nor worry about other stuff tripping it up.
public class NoFlipZone : MonoBehaviour
{
    BoxCollider boxCollider;
    Transform playerTransform;
    Transform robotTransform;

    // Used by the gravity manager to check if it's safe to flip where the character are now.
    [HideInInspector]
    public bool characterInZone = false;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        playerTransform = GameObject.FindWithTag("Player").transform;
        robotTransform = GameObject.FindWithTag("robot").transform;
    }

    void OnTriggerEnter(Collider other)
    {
        characterInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!CheckCharacterInBounds())
        {
            characterInZone = false;
        }
    }

    // TODO: verify that this is okay, or we might need to check colliders instead of positions.
    bool CheckCharacterInBounds()
    {
        return boxCollider.bounds.Contains(playerTransform.position) || boxCollider.bounds.Contains(robotTransform.position);
    }

}
