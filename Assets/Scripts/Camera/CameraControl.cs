using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    StateManager stateManager;

    [HideInInspector] public static CameraControl CC;
    [Header("Camera default target")] public Transform target;

    [Header("Camera offset")]
    // Some default values to start, tweakable in Inspector. Respects standard rotation on x/z
    public float angle = -60f;
    public float distance = 8f;
    public float height = 12f;

    private Vector3 defaultOffset;
    private Vector3 offset;
    private bool changingTarget = false;
    private bool changingOffset = false;

    void Awake()
    {
        CC = this;
        stateManager = GameObject.FindObjectOfType<StateManager>();
    }

    void Start()
    {
        ChangeOffset(angle, height);
        defaultOffset = offset;
        if (!target) target = GameObject.FindWithTag("Player").transform;
    }

    void LateUpdate()
    {
        if (!changingTarget)
        {
            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }
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

    // TODO: make it possible to change target + offset together
    public void ChangeTarget(Transform newTarget, float time = 0f)
    {
        if (!changingTarget && !changingOffset)
        {
            changingTarget = true;
            stateManager.SetState(StateManager.State.Inert);
            StartCoroutine(ChangeTargetProcess(newTarget, time));
        }
    }

    IEnumerator ChangeTargetProcess(Transform newTarget, float time)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, newTarget.position + offset, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = newTarget.position + offset;
        target = newTarget;
        changingTarget = false;
        stateManager.EndInert();
    }

}
