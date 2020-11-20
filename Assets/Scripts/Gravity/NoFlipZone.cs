using UnityEngine;

// Note that NoFlipZones are on their own layer which ONLY collides with player & robot - nothing else.
public class NoFlipZone : MonoBehaviour
{
    BoxCollider boxCollider;

    // Fields used by the gravity manager to check if it's safe to flip & what UI to use.
    [HideInInspector] public bool characterInZone = false;
    [HideInInspector] public bool playerInZone = false;
    [HideInInspector] public bool robotInZone = false;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        characterInZone = true;

        if (other.tag == "Player") playerInZone = true;
        else if (other.tag == "robot") robotInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") playerInZone = false;
        else if (other.tag == "robot") robotInZone = false;

        UpdateInZoneStatus();
    }

    void UpdateInZoneStatus()
    {
        if (playerInZone || robotInZone) characterInZone = true;
        else characterInZone = false;
    }
}
