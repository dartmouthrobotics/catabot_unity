using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float beamRadius = 0.005f; // 5 mm
    public float rateOfRainfall = 0f; // Units of mm / hr

    void Update()
    {
        RaycastHit rayHit;
        float dropDistribution = LidarMath.DropSizeDistribution(rateOfRainfall);
        float beamVolumeOneMeter = Mathf.PI * Mathf.Pow(beamRadius, 2f);
        for (float i = 0; i < 360; i++)
        {
            Ray ray = new Ray(transform.position, Quaternion.Euler(0, i, 0) * this.gameObject.transform.forward * 2);
            bool hitObject = Physics.Raycast(ray, out rayHit, LidarMath.MaxLidarDistance);
            float distance = LidarMath.CalculateLidarDistanceWithRain(rayHit.distance, hitObject, dropDistribution, beamVolumeOneMeter);
            Debug.DrawRay(transform.position, Quaternion.Euler(0, i, 0) * transform.forward.normalized * distance, distance == rayHit.distance ? Color.green : Color.red, 0.01f);
        }
    }
}
