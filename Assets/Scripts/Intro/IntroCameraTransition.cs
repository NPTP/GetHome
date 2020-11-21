using System.Collections;
using UnityEngine;

public class IntroCameraTransition : MonoBehaviour
{
    Transform cameraStartTransform;

    void Awake()
    {
        cameraStartTransform = transform.GetChild(0).transform;
    }

    void Start()
    {
        StartCoroutine("IntroCamera");
    }

    IEnumerator IntroCamera()
    {
        yield return null;
        CameraControl.CC.SetPosAndRot(cameraStartTransform.position, cameraStartTransform.rotation);
        CameraControl.CC.ChangeTarget(GameObject.FindWithTag("Player").transform, 5f);
        Destroy(this.gameObject);
    }
}
