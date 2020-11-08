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
        if (collision.gameObject.tag == "Player")
        {
            return; // Ignore the player! We don't ever want to stack them on the crate!
        }
        if (collision.transform.position.y > transform.position.y)
        {

            childobject = collision.gameObject.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            return; // Ignore the player! We don't ever want to unstack them from crates!
        }

        if (collision.transform.position.y > transform.position.y)
        {
            childobject = null;
        }
    }

    public void DoMove(Vector3 move)
    {
        if (childobject)
        {
            childobject.position += move;
        }
    }
}
