using System.Collections;
using UnityEngine;

public class ReactorCameraTransition : MonoBehaviour
{
    Transform position1;
    Transform position2;
    SceneLoader sceneLoader;

    public OcclusionVolume volume1;

    void Awake()
    {
        position1 = transform.GetChild(0).transform;
        position2 = transform.GetChild(1).transform;
        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    void Start()
    {
        StartCoroutine("ReactorCamera");
    }

    IEnumerator ReactorCamera()
    {
        yield return null;

        volume1.ShowRoom();

        CameraControl.CC.SetPosAndRot(position1.position, position1.rotation);
        // yield return new WaitForSeconds(sceneLoader.startFadeDuration);

        CameraControl.CC.ChangeTarget(position2, 4f, false);
        yield return new WaitWhile(CameraControl.CC.IsChangingTarget);

        volume1.HideRoom();

        CameraControl.CC.ChangeTarget(GameObject.FindWithTag("Player").transform, 4f);
        yield return new WaitWhile(CameraControl.CC.IsChangingTarget);

        Destroy(this.gameObject);
    }
}
