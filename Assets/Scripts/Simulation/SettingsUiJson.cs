using UnityEngine;

[System.Serializable]
public class SettingsUiJson
{
    public bool radarActive;
    public bool lidarActive;
    public bool imuActive;
    public bool minimapActive;
    public float rainIntensity;
    public bool screenshotsSaveToFile;
    public bool screenshotsColorActive;
    public bool screenshotsSegmentationActive;
    public bool screenshotsDepthActive;
    public bool screenshotsNormalActive;
    public string robotControlType;
    public string boatControlType;
    public int boatsNumberActive;
    public float gpsLatitude;
    public float gpsLongitude;
    public bool terrainFromFile;
    public float terrainHeightMin;
    public float terrainHeightMax;
    public float terrainPixelScale;
    public string terrainPathToElevationFile;
}
