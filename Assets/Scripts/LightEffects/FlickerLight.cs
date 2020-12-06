using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    Light thisLight;
    Color originalColor;
    bool lightOnState;
    bool lightOffFlicker;

    public int transitionToOffState = 200;
    public int lightOffFlickerAmount = 10;
    public int transitionToOnState = 35;
    // Start is called before the first frame update
    void Start()
    {
        thisLight = GetComponent<Light>();
        originalColor = thisLight.color;
        lightOnState = true;
        lightOffFlicker = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (lightOnState)
        {
            thisLight.color = originalColor;
            if (Random.Range(0, transitionToOffState) < 1)
            {
                lightOnState = false;
            }
        }
        else
        {
            if (lightOffFlicker)
            {
                thisLight.color = Color.black;
            }
            else
            {
                thisLight.color = originalColor;
            }
            if (Random.Range(0, lightOffFlickerAmount) < 1)
            {
                lightOffFlicker = !lightOffFlicker;
            }
            if (Random.Range(0, transitionToOnState) < 1)
            {
                lightOnState = true;
            }
        }
    }
}
