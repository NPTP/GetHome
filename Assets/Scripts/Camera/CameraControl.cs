using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    StateManager stateManager;
    GravityManager gravityManager;

    [HideInInspector] public static CameraControl CC;
    [Header("Camera default target")] public Transform target;
    [Header("Audio Listener Object")]
    [SerializeField] Transform audioListenerObject;

    [Header("Camera offset")]
    // Some default values to start, tweakable in Inspector. Respects standard rotation on x/z
    public float angle = -60f;
    public float distance = 8f;
    public float height = 12f;

    private Vector3 defaultOffset;
    private Vector3 offset;
    private float lerpModifier = 25f;
    private bool changingTarget = false;
    private bool changingOffset = false;
    private bool screenShaking = false;

    void Awake()
    {
        CC = this;
        stateManager = GameObject.FindObjectOfType<StateManager>();
        gravityManager = GameObject.FindObjectOfType<GravityManager>();
        ChangeOffset(angle, height);
        defaultOffset = offset;
        if (!target) target = GameObject.FindWithTag("Player").transform;
    }

    // USE THE BELOW TO TEST SCREEN SHAKE
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.K))
    //     {
    //         ScreenShake();
    //     }
    // }

    // WARNING! Don't use this except for scripting.
    public void SetPosAndRot(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    void LateUpdate()
    {
        if (gravityManager.isFlipping && !changingTarget && !screenShaking)
        {
            SetDefaultPositionRotation();
            audioListenerObject.position = transform.position - offset;
        }
    }

    // Used to reset the camera on lateupdate, also called on look up/down.
    public void SetDefaultPositionRotation()
    {
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    void FixedUpdate()
    {
        if (!gravityManager.isFlipping && !changingTarget && !screenShaking)
        {
            Vector3 cameraPosition = Vector3.Lerp(
                transform.position,
                target.position + offset,
                Time.deltaTime * lerpModifier
            );
            transform.position = cameraPosition;

            Quaternion cameraRotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(target.position - transform.position),
                Time.deltaTime
            );
            transform.rotation = cameraRotation;

            audioListenerObject.position = transform.position - offset;
        }
    }

    public void ScreenShake(float intensity = 1, float time = 1f)
    {
        if (time <= 0)
            return; // Div by 0, don't do it!
        StopCoroutine("ScreenShakeProcess");
        StartCoroutine("ScreenShakeProcess", new Tuple<float, float>(intensity, time));
    }

    IEnumerator ScreenShakeProcess(Tuple<float, float> intensityTimePair)
    {
        screenShaking = true;
        float intensity = intensityTimePair.Item1;
        float time = intensityTimePair.Item2;
        float elapsed = 0;
        float origMin = -intensity;
        float origMax = +intensity;

        while (elapsed < time)
        {
            float min = origMin + (elapsed / time) * intensity;
            float max = origMax - (elapsed / time) * intensity;
            transform.position = target.position + offset + new Vector3(
                UnityEngine.Random.Range(min, max),
                UnityEngine.Random.Range(min, max),
                UnityEngine.Random.Range(min, max)
            );
            transform.LookAt(target.position);

            yield return null;
            elapsed += Time.deltaTime;
        }
        screenShaking = false;
    }

    public void ChangeOffset(float newAngle, float newHeight, float time = 0f)
    {
        if (!changingOffset && !changingTarget)
        {
            changingOffset = true;
            float x = distance * Mathf.Cos(newAngle * Mathf.Deg2Rad);
            float z = distance * Mathf.Sin(newAngle * Mathf.Deg2Rad);
            float y = newHeight;
            Vector3 newOffset = new Vector3(x, y, z);
            StartCoroutine(ChangeOffsetProcess(newOffset, time));
        }
    }

    public void SetDefaultOffset(float time = 0f)
    {
        StartCoroutine(ChangeOffsetProcess(defaultOffset, time));
    }

    // Updating the offset WHILE lateupdate still runs, so we're still looking at the target.
    IEnumerator ChangeOffsetProcess(Vector3 newOffset, float time)
    {
        Vector3 startOffset = offset;
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            t = t * t * (3f - 2f * t);
            offset = Vector3.Lerp(startOffset, newOffset, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        offset = newOffset;
        changingOffset = false;
    }

    public bool IsChangingTarget()
    {
        return changingTarget;
    }

    // TODO: make it possible to change target + offset together
    public void ChangeTarget(Transform newTarget, float time = 0f, bool lookAtTarget = true)
    {
        if (!changingTarget && !changingOffset)
        {
            StopCoroutine("ScreenShakeProcess");
            changingTarget = true;
            stateManager.SetState(StateManager.State.Inert);
            StartCoroutine(ChangeTargetProcess(newTarget, time, lookAtTarget));
        }
    }

    IEnumerator ChangeTargetProcess(Transform newTarget, float time, bool lookAtTarget)
    {
        // Get starting position and rotation
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Get ending rotation
        transform.position = target.position + offset;
        Quaternion endRot;
        if (lookAtTarget)
        {
            transform.LookAt(target.position);
            endRot = transform.rotation;
        }
        else
        {
            endRot = newTarget.rotation;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            t = t * t * (3f - 2f * t);

            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            if (lookAtTarget)
                transform.position = Vector3.Lerp(startPos, newTarget.position + offset, t);
            else
                transform.position = Vector3.Lerp(startPos, newTarget.position, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (lookAtTarget)
        {
            transform.LookAt(newTarget.position);
            transform.position = newTarget.position + offset;
        }
        else
        {
            transform.rotation = newTarget.rotation;
            transform.position = newTarget.position;
        }

        target = newTarget;
        changingTarget = false;
        stateManager.EndInert();
    }

}
