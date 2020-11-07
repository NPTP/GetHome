using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxStacking : MonoBehaviour
{
    private GameObject origparent;
    private Transform childobject;
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.position.y > transform.position.y)
        {

            childobject = collision.gameObject.transform;
            Debug.Log("I'm " + this.gameObject + " and my child is " + collision.gameObject);
            // Debug.Log("Collision is here!");
            //origparent = collision.gameObject.transform.parent.gameObject;
            //collision.gameObject.transform.parent = this.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.position.y > transform.position.y)
        {
            //collision.gameObject.transform.parent = origparent.transform;
            childobject = null;
        }
    }

    public void DoMove(Vector3 move)
    {
        Debug.Log("I'm in DOMOVE");
        if (childobject)
        {
            Debug.Log("And I have a child!");
            childobject.position += move;
        }
    }
}
