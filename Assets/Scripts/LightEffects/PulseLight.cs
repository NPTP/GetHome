using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseLight : MonoBehaviour
{
    Light thisLight;
    // we'll make the range pulse so it doesn't interfere with the intensity that is controlled
    // by the occlusion volumes
    float lightOriginalRange;
    public float pulseAmount = 3;
    public float pulseSpeed = 2;
    
    void Start()
    {
        thisLight = GetComponent<Light>();
        lightOriginalRange = thisLight.range;
    }

    // Update is called once per frame
    void Update()
    {
        thisLight.range = lightOriginalRange + pulseAmount * Mathf.Sin(Time.time * pulseSpeed);
    }
}
