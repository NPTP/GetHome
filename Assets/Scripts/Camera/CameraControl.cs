using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [HideInInspector] public static CameraControl CC;
    [Header("Camera default target")] public Transform target;

    [Header("Camera distances")]
    // Some default values to start, tweakable in Inspector
    public float x = 4.5f;
    public float y = 11.25f;
    public float z = -5.25f;

    private Vector3 offsetX;
    private bool changingTarget = false;

    void Awake()
    {
        if (CC != null)
            GameObject.Destroy(CC);
        else
            CC = this;

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        offsetX = new Vector3(x, y, z);
    }

    void LateUpdate()
    {
        if (!changingTarget)
        {
            transform.position = target.position + offsetX;
            transform.LookAt(target.position);
        }
    }

    public void ChangeTarget(Transform newTarget, float time = 0f)
    {
        if (!changingTarget)
        {
            StartCoroutine(TargetSwitch(newTarget, time));
            changingTarget = true;
        }
    }

    IEnumerator TargetSwitch(Transform newTarget, float time)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, newTarget.position + offsetX, t);
            elapsed += Time.deltaTime;
            yield return null;

        }
        transform.position = newTarget.position + offsetX;
        target = newTarget;
        changingTarget = false;
        yield return null;
    }
}
