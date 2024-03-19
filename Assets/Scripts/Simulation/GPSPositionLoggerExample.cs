using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSPositionLogger : MonoBehaviour
{
    public GPSMath _gps;

    public void Update() {
        Debug.Log("GPS (Latitude, Longitude): " + _gps.CalculateLatLonFromObjectPosition(transform.position).ToString("F5"));
    }
}
