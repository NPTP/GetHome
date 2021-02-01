using System.Collections;
using UnityEngine;

public class CRTWall : MonoBehaviour
{

    [SerializeField] Texture[] staticFrames;
    Renderer rdr;
    int index = 0;
    bool animating = true;
    float frameTime = 0.042f; // 24 fps

    void Start()
    {
        rdr = GetComponent<Renderer>();
        StartCoroutine(Animation());
    }

    IEnumerator Animation()
    {
        while(animating)
        {
            if (index >= staticFrames.Length)
                index = 0;
            
            rdr.material.SetTexture ("_EmissionMap", staticFrames[index]);
            index++;

            float elapsed = 0;
            while (elapsed < frameTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
