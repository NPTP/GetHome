using UnityEngine;
using UnityEngine.UI;

public class IntroScanlines : MonoBehaviour
{
    Image image;
    float alphaShift = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        alphaShift *= -1f;
        image.color = Helper.ChangedAlpha(image.color, image.color.a + alphaShift);
    }
}
