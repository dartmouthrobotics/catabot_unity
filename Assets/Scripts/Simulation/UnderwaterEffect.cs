using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnderwaterEffect : MonoBehaviour
{
    Transform CameraTransform;
    public GameObject AbovewaterVolume;
    public GameObject UnderwaterVolume;
    public float fogDensity = 0.005f;

    void Start()
    {
        CameraTransform = this.transform;
    }

    void Update()
    {
        if (CameraTransform.position.y < 0)
        {
            AbovewaterVolume.SetActive(false);
            UnderwaterVolume.SetActive(true);
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogMode = FogMode.Linear;
        } else
        {
            AbovewaterVolume.SetActive(true);
            UnderwaterVolume.SetActive(false);
            RenderSettings.fogDensity = 0.0025f;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
        }
    }

}
