using UnityEngine;

// Note that NoFlipZones are on their own layer which ONLY collides with player & robot - nothing else.
public class NoFlipZone : MonoBehaviour
{
    BoxCollider boxCollider;
    bool playerInZone = false;
    bool robotInZone = false;

    // Used by the gravity manager to check if it's safe to flip where the character are now.
    [HideInInspector]
    public bool characterInZone = false;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        characterInZone = true;

        if (other.tag == "Player") playerInZone = true;
        if (other.tag == "robot") robotInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") playerInZone = false;
        if (other.tag == "robot") robotInZone = false;

        UpdateInZoneStatus();
    }

    void UpdateInZoneStatus()
    {
        if (playerInZone || robotInZone) characterInZone = true;
        else characterInZone = false;
    }
}
