using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Steps;

public class BoxStacking : MonoBehaviour
{
    private GameObject origparent;
    private List<GameObject> childobjects;
    // Start is called before the first frame update
    private void Start()
    {
        childobjects = new List<GameObject> { };
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            return; // Ignore the player! We don't ever want to stack them on the crate!
        }

        if ((collision.gameObject.tag != "robot" || collision.gameObject.tag != "FloppyProps") 
            && collision.transform.position.y > transform.position.y)
        {
            childobjects.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            return; // Ignore the player! We don't ever want to unstack them from crates!
        }
        childobjects.Remove(collision.gameObject);
        
    }

    public void DoMove(Vector3 move)
    {
        foreach (GameObject childobject in childobjects)
        {
            childobject.transform.position += move;
        }
    }
}
