using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCulling : MonoBehaviour
{
    Transform playerTransform;
    float playerHeight;
    public LayerMask mask;
    public float playerMidpointAdjustment = 0f;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        playerHeight = player.GetComponent<CapsuleCollider>().height;
    }

    private void Update()
    {
        Vector3 playerMidpoint = playerTransform.position + new Vector3(0f, (playerHeight / 2) + playerMidpointAdjustment, 0f);
        Vector3 origin = transform.position;
        Vector3 direction = (playerMidpoint - transform.position).normalized;
        float maxDistance = (playerMidpoint - transform.position).magnitude;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            maxDistance,
            mask
        );
        Debug.DrawLine(
            origin,
            playerMidpoint,
            Color.magenta,
            .2f
        );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider.tag == "LevelInterior")
                {
                    Transform wallParent = collider.gameObject.transform.parent.gameObject.transform;
                    foreach (Transform child in wallParent)
                    {
                        child.gameObject.GetComponent<LevelInteriorTransparency>().intersecting = true;
                    }
                    // Material transparentMat = Resources.Load<Material>("TransparentMat");
                    // Renderer renderer = collider.gameObject.GetComponent<Renderer>();
                    // // Get the current material applied on the GameObject
                    // Material oldMat = renderer.material;
                    // Debug.Log("Applied Material: " + oldMat.name);
                    // // Set the new material on the GameObject
                    // renderer.material = transparentMat;
                }
                else if (collider.tag == "LevelWall")
                {
                    Transform wallParent = collider.gameObject.transform.parent.gameObject.transform;
                    foreach (Transform child in wallParent)
                    {
                        child.gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
            }
        }
    }


}
