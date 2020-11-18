using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettableTape : MonoBehaviour
{
    public string DialogID;
    private DialogManager dm;

    public void Start()
    {
        dm = GameObject.FindObjectOfType<DialogManager>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            dm.PlayDialog(DialogID);
            Destroy(this.gameObject);
        }
    }
}
