using System.Collections;
using UnityEngine;

public class CullTransparentTile : MonoBehaviour, CullTile
{
    Renderer r;
    Color startColor;
    Color endColor;
    float cullAlpha = 0.25f; // Keep private for universal value on all tiles using this script.
    float cullTime = 0.2f;  // Keep private for universal value on all tiles using this script.
    bool changing = false;
    bool resetting = false;
    bool hitThisFrame = false;

    void Start()
    {
        r = GetComponent<Renderer>();
        startColor = new Color(r.material.color.r, r.material.color.g, r.material.color.b, 1f);
        endColor = new Color(r.material.color.r, r.material.color.g, r.material.color.b, cullAlpha);
    }

    void LateUpdate()
    {
        if (!hitThisFrame && !resetting)
        {
            changing = false;
            resetting = true;
            StopCoroutine("ChangeAlpha");
            StartCoroutine("ChangeAlpha", false);
        }
        hitThisFrame = false;
    }

    public void CullThisFrame()
    {
        hitThisFrame = true;
        if (!changing)
        {
            changing = true;
            resetting = false;
            StopCoroutine("ChangeAlpha");
            StartCoroutine("ChangeAlpha", true);
        }
    }

    IEnumerator ChangeAlpha(bool cull)
    {
        Color end;
        if (cull)
            end = endColor;
        else
            end = startColor;

        Color currentColor = r.material.color;

        float elapsed = 0f;
        while (elapsed < cullTime)
        {
            r.material.color = Color.Lerp(currentColor, end, elapsed / cullTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        r.material.color = end;
    }
}
