﻿using UnityEngine;

public class GravityWatch : MonoBehaviour
{
    GravityManager gravityManager;
    GameObject gwatch;

    void Awake()
    {
        gravityManager = FindObjectOfType<GravityManager>();
        gwatch = GameObject.FindWithTag("KiraWatch");
        gwatch.SetActive(false);
    }

    void Start()
    {
        gravityManager.haveGravWatch = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            gravityManager.StopLookingOnPickup("GravityWatch");

            gwatch.SetActive(true);
            gravityManager.haveGravWatch = true;
            GameObject.Find("TutorialScreen").GetComponent<TutorialScreen>().StartTutorial();
            // Destroy(this.gameObject);

            // Trying this in case the destroy of the prefab hurts the tutorial
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<SphereCollider>().enabled = false;
        }
    }
}
