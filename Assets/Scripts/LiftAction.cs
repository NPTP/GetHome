using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LiftAction : MonoBehaviour, IObjectAction
{
    StateManager stateManager;
    public bool lifted;
    public Transform ogParent;

    public int liftAmount;

    // Vector3 originalPosition;
    bool allowUse = true;

    void Start()
    {
        stateManager = FindObjectOfType<StateManager>();
        // originalPosition = transform.position;

    }

    public void action()
    {
        if (allowUse)
        {
            GetComponent<AudioSource>()?.Play();

            if (lifted)
            {
                // transform.position+= new Vector3(0,-2,0);
                allowUse = false;
                lifted = false;
                StartCoroutine(WaitForLift(transform.DOLocalMove(transform.localPosition + new Vector3(0, -liftAmount, 0), 1f).SetEase(Ease.OutCubic)));

            }
            else
            {
                // transform.position += new Vector3(0,2,0);
                allowUse = false;
                lifted = true;
                StartCoroutine(WaitForLift(transform.DOLocalMove(transform.localPosition + new Vector3(0, liftAmount, 0), 1f).SetEase(Ease.OutCubic)));
            }
        }
    }

    IEnumerator WaitForLift(Tween liftMovement)
    {
        //yield return new WaitWhile(() => liftMovement != null && liftMovement.IsPlaying());
        yield return liftMovement.WaitForCompletion();
        allowUse = true;
    }

    /*
        void OnCollisionEnter(Collision collision)
        {
            collision.transform.SetParent(transform);
        }

        void OnCollisionExit(Collision collision)
        {
            collision.transform.parent = ogParent;
        }
    */

}
