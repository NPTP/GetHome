using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettableTape : MonoBehaviour
{
    public string DialogID;
    private DialogManager dm;
    private StateManager sm;

    public void Start()
    {
        dm = FindObjectOfType<DialogManager>();
        sm = FindObjectOfType<StateManager>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FindObjectOfType<GravityManager>().StopLookingOnPickup("Tape");
            dm.PlayDialog(DialogID);
            Destroy(this.gameObject);
        }
    }
}
