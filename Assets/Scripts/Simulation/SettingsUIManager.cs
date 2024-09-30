using DigitalRuby.RainMaker;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.UI;

public class SettingsUIManager : MonoBehaviour
{
    [SerializeField]
    private bool _configureUiFromJSON = true;
    [SerializeField]
    private string _pathToSettingsConfigJson = "SettingsConfig.json";

    [Header("Objects being modified")]
    [SerializeField]
    private Radar _radarController;

    [SerializeField]
    private LidarPerception _lidarController;

    [SerializeField]
    private IMUController _imuController;

    [SerializeField]
    private GameObject _minimapGraphics;

    [SerializeField]
    private GameObject _minimapCamera;

    [SerializeField]
    private RainScript _rainEffect;

    [SerializeField]
    private PerceptionCamera _perception;
    bool _saveScreenshots;
    bool _saveColor;
    bool _saveSegment;
    bool _saveDepth;
    bool _saveNormal;

    private CameraLabeler _depthLabeler;
    private CameraLabeler _normalLabeler;
    private CameraLabeler _segmentationLabeler;

    [SerializeField]
    private VehicleMultiMovementManager _robotMovementManager;
    [SerializeField]
    private VehicleMultiMovementManager _boatMovementManagerPrefab;
    private List<VehicleMultiMovementManager> _boatMovementManagers = new List<VehicleMultiMovementManager>();

    [SerializeField]
    private GPSMath _gpsManager;

    [SerializeField]
    private TerrainFromElevationImage _terrainFromElevationImage;
    [SerializeField]
    private GameObject _defaultWorld;
    private bool _generatedTerrainActive = false;

    [SerializeField]
    private RosBroadcastAllFrames _rosBroadcaster;

    [Header("Toggles")]
    [SerializeField]
    private Toggle _radarToggle;
    [SerializeField]
    private Toggle _lidarToggle;
    [SerializeField]
    private Toggle _imuToggle;
    [SerializeField]
    private Toggle _minimapToggle;
    [SerializeField]
    private Toggle _saveScreenshotsToggle;
    [SerializeField]
    private Toggle _colorToggle;
    [SerializeField]
    private Toggle _segmentToggle;
    [SerializeField]
    private Toggle _depthToggle;
    [SerializeField]
    private Toggle _normalToggle;
    [SerializeField]
    private Toggle _terrainFromFileToggle;

    [Header("Input Fields")]
    [SerializeField]
    private TMPro.TMP_InputField _rainInput;
    [SerializeField]
    private TMPro.TMP_InputField _numBoatsInput;
    [SerializeField]
    private TMPro.TMP_InputField _latInput;
    [SerializeField]
    private TMPro.TMP_InputField _lonInput;
    [SerializeField]
    private TMPro.TMP_InputField _elevationMinInput;
    [SerializeField]
    private TMPro.TMP_InputField _elevationMaxInput;
    [SerializeField]
    private TMPro.TMP_InputField _elevationScaleInput;
    [SerializeField]
    private TMPro.TMP_InputField _pathToImageInput;

    [Header("Dropdowns")]
    [SerializeField]
    private TMPro.TMP_Dropdown _robotMovementInput;
    [SerializeField]
    private TMPro.TMP_Dropdown _boatMovementInput;

    [Header("Menu Buttons")]
    [SerializeField]
    private GameObject _showMenuButton;

    private void Start() {
        for (int i = 0; i < _perception.labelers.Count; i++) {
            Debug.Log("Found Labeler: " + _perception.labelers[i].labelerId);
            switch (_perception.labelers[i].labelerId) {
                case "Depth":
                    _depthLabeler = _perception.labelers[i];
                    break;
                case "Normal":
                    _normalLabeler = _perception.labelers[i];
                    break;
                case "semantic segmentation":
                    _segmentationLabeler = _perception.labelers[i];
                    break;
            }
        }

        // Generate empty json string
        SettingsUiJson tempJson = new SettingsUiJson();
        Debug.Log("JSON | " + JsonUtility.ToJson(tempJson));

        if (_configureUiFromJSON) {
            ConfigureUiFromJSON();
        }

        //TODO: Set more defaults! Should be done at the same time as the JSON file is read.
        //TODO: If no JSON file is found, then use the defaults already in the UI.
        InitializeSensors();
        InitializeRain();
        InitializeScreenshots();
        InitializeRobotAndBoats();
        InitializeGPS();
        InitializeTerrain();
    }

    private void ConfigureUiFromJSON() {
        try {
            Debug.Log("Json config start");
            SettingsUiJson settingsConfig = JsonUtility.FromJson<SettingsUiJson>(System.IO.File.ReadAllLines(_pathToSettingsConfigJson)[0]);
            Debug.Log("Settings Config loaded");
            if (settingsConfig != null) {
                Debug.Log("Json was not null");
                _radarToggle.isOn = settingsConfig.radarActive;
                _lidarToggle.isOn = settingsConfig.lidarActive;
                _imuToggle.isOn = settingsConfig.imuActive;
                _minimapToggle.isOn = settingsConfig.minimapActive;
                _rainInput.text = settingsConfig.rainIntensity.ToString("F2");
                _colorToggle.isOn = settingsConfig.screenshotsColorActive;
                _segmentToggle.isOn = settingsConfig.screenshotsSegmentationActive;
                _depthToggle.isOn = settingsConfig.screenshotsDepthActive;
                _normalToggle.isOn = settingsConfig.screenshotsNormalActive;
                _saveScreenshotsToggle.isOn = settingsConfig.screenshotsSaveToFile;

                _robotMovementInput.value = 0;
                string[] names = _robotMovementManager.MovementNames();
                for (int i = 0; i < names.Length; i++) {
                    if (names[i].Equals(settingsConfig.robotControlType)) {
                        _robotMovementInput.value = i;
                    }
                }

                _boatMovementInput.value = 0;
                names = _boatMovementManagerPrefab.MovementNames();
                for (int i = 0; i < names.Length; i++) {
                    if (names[i].Equals(settingsConfig.boatControlType)) {
                        _boatMovementInput.value = i;
                    }
                }

                _numBoatsInput.text = settingsConfig.boatsNumberActive.ToString("F0");
                _latInput.text = settingsConfig.gpsLatitude.ToString("F2");
                _lonInput.text = settingsConfig.gpsLongitude.ToString("F2");
                _terrainFromFileToggle.isOn = settingsConfig.terrainFromFile;
                _elevationMinInput.text = settingsConfig.terrainHeightMin.ToString("F2");
                _elevationMaxInput.text = settingsConfig.terrainHeightMax.ToString("F2");
                _elevationScaleInput.text = settingsConfig.terrainPixelScale.ToString("F2");
                _pathToImageInput.text = settingsConfig.terrainPathToElevationFile;
            }
        } catch (System.Exception e) { }
    }

    // Menu visibility

    public void ToggleMenuVisibility() {
        gameObject.SetActive(!gameObject.activeInHierarchy);
        _showMenuButton.SetActive(!_showMenuButton.activeInHierarchy);
    }

    // Sensors
    private void InitializeSensors() {
        OnRadarToggleChange(_radarToggle.isOn);
        OnLidarToggleChange(_lidarToggle.isOn);
        OnImuToggleChange(_imuToggle.isOn);
        OnMinimapToggleChange(_minimapToggle.isOn);
    }

    public void OnRadarToggleChange(bool value) {
        if (_radarController != null) {
            _radarController.RadarActive = value;
        }
    }

    public void OnLidarToggleChange(bool value) {
        if (_lidarController != null) {
            _lidarController.LidarActive = value;
        }
    }

    public void OnImuToggleChange(bool value) {
        if (_imuController != null) {
            _imuController.ImuActive = value;
        }
    }

    public void OnMinimapToggleChange(bool value) {
        _minimapGraphics.SetActive(value);
        _minimapCamera.SetActive(value);
    }

    // Rain intensity

    private void InitializeRain() {
        OnRainIntensityInputChange(_rainInput.text);
    }

    public void OnRainIntensityInputChange(string value) {
        float rateOfRainfall;
        if (float.TryParse(value, out rateOfRainfall)) {
            float clampedRain = Mathf.Clamp(rateOfRainfall, 0, 50);
            _lidarController.RateOfRainfall = clampedRain;
            _rainEffect.RainIntensity = Mathf.Clamp01(clampedRain / 50f);
            if (clampedRain != rateOfRainfall) {
                _rainInput.text = clampedRain.ToString();
            }
        }
    }

    // Saving screenshots
    private void InitializeScreenshots() {
        OnSaveColorImagesToggleChange(_colorToggle.isOn);
        OnSaveDepthImagesToggleChange(_depthToggle.isOn);
        OnSaveNormalImagesToggleChange(_normalToggle.isOn);
        OnSaveSegmentationImagesToggleChange(_segmentToggle.isOn);
        // Make sure to turn the perception on/off last just in case it starts trying to take photos before the types of photos it should take are configured
        OnSaveScreenshotToggleChange(_saveScreenshotsToggle.isOn);
    }

    public void OnSaveScreenshotToggleChange(bool value) {
        // Make sure that the Perception Camera is ENABLED!
        // NEVER EVER DISABLE THE PERCEPTION CAMERA!
        _saveScreenshots = value;
        if(value) {
            _perception.CaptureRgbImages = _saveColor;
            _depthLabeler.enabled = _saveDepth;
            _normalLabeler.enabled = _saveNormal;
            _segmentationLabeler.enabled = _saveSegment;
        } else {
            _perception.CaptureRgbImages = false;
            _depthLabeler.enabled = false;
            _normalLabeler.enabled = false;
            _segmentationLabeler.enabled = false;
        }
    }

    public void OnSaveColorImagesToggleChange(bool value) {
        _saveColor = value;
        if (_saveScreenshots) {
            _perception.CaptureRgbImages = value;
        }
    }

    public void OnSaveDepthImagesToggleChange(bool value) {
        _saveDepth = value;
        if (_saveScreenshots) {
            _depthLabeler.enabled = value;
        }
    }

    public void OnSaveNormalImagesToggleChange(bool value) {
        _saveNormal = value;
        if (_saveScreenshots) {
            _normalLabeler.enabled = value;
        }
    }

    public void OnSaveSegmentationImagesToggleChange(bool value) {
        _saveSegment = value;
        if (_saveScreenshots) {
            _segmentationLabeler.enabled = value;
        }
    }

    // The robot and boat stuff

    private void InitializeRobotAndBoats() {
        // Robot Movement Dropdown setup
        string[] names = _robotMovementManager.MovementNames();
        _robotMovementInput.options.Clear();
        for (int i = 0; i < names.Length; i++) {
            _robotMovementInput.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = names[i] });
        }
        _robotMovementManager.SetRosId(0);

        // Make sure the robot is using the default control scheme
        OnRobotControlDropdownChange(_robotMovementInput.value);

        // Boats Movement Dropdown setup
        names = _boatMovementManagerPrefab.MovementNames();
        _boatMovementInput.options.Clear();
        for (int i = 0; i < names.Length; i++) {
            _boatMovementInput.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = names[i] });
        }

        // Spawn the default boats
        OnNumBoatsInputChange(_numBoatsInput.text);
    }

    public void OnRobotControlDropdownChange(int value) {
        _robotMovementManager.SetMovementActive(value);
    }

    public void OnBoatControlDropdownChange(int value) {
        for (int i = 0; i < _boatMovementManagers.Count; i++) {
            _boatMovementManagers[i].SetMovementActive(value);
        }
    }

    public void OnNumBoatsInputChange(string value) {
        int numBoats;
        if (int.TryParse(value, out numBoats)) {
            int clampedNumBoats = Mathf.Clamp(numBoats, 0, 100);
            
            // Actually adjust the number of boats out there!
            for(int i = 0; i < _boatMovementManagers.Count; i++) {
                _boatMovementManagers[i].gameObject.SetActive(i < clampedNumBoats);
            }

            for(int i = _boatMovementManagers.Count; i < clampedNumBoats; i++) {
                _boatMovementManagers.Add(Instantiate<VehicleMultiMovementManager>(_boatMovementManagerPrefab, new Vector3(Random.Range(250,750), 0, Random.Range(250, 750)), Quaternion.identity));
                _boatMovementManagers[i].SetRosId(i+1);
                _rosBroadcaster.AddTrafficBoat(_boatMovementManagers[i].transform, i+1);
            }

            if (clampedNumBoats != numBoats) {
                _numBoatsInput.text = clampedNumBoats.ToString();
            }

            // Make sure each boat is set to the right type of movement
            OnBoatControlDropdownChange(_boatMovementInput.value);
        }
    }

    // GPS

    private void InitializeGPS() {
        OnLatitudeInputChange(_latInput.text);
        OnLongitudeInputChange(_lonInput.text);
    }

    public void OnLatitudeInputChange(string value) {
        float latitude;
        if (float.TryParse(value, out latitude)) {
            float clampedLat = Mathf.Clamp(latitude, -90, 90);
            _gpsManager.WorldOriginLatLong = new Vector2(clampedLat, _gpsManager._worldOriginLatLong.y);
            if (clampedLat != latitude) {
                _latInput.text = clampedLat.ToString();
            }
        }
    }

    public void OnLongitudeInputChange(string value) {
        float longitude;
        if (float.TryParse(value, out longitude)) {
            float clampedLon = Mathf.Clamp(longitude, -180, 180);
            _gpsManager.WorldOriginLatLong = new Vector2(_gpsManager._worldOriginLatLong.x, longitude);
            if (clampedLon != longitude) {
                _lonInput.text = clampedLon.ToString();
            }
        }
    }

    // Terrain

    private void InitializeTerrain() {
        OnTerrainMinChanged(_elevationMinInput.text);
        OnTerrainMaxChanged(_elevationMaxInput.text);
        OnTerrainScaleChanged(_elevationScaleInput.text);

        // Make sure all of the terrain settings are configured before you attempt to create the terrain
        OnTerrainFromFileToggleChange(_terrainFromFileToggle.isOn);
    }

    public void OnTerrainFromFileToggleChange(bool value) {
        _generatedTerrainActive = value;
        if (value) {
            if (_defaultWorld != null) {
                _defaultWorld.SetActive(false);
            }
            _terrainFromElevationImage.GenerateTerrainFromImage(_pathToImageInput.text);
        } else {
            _terrainFromElevationImage.DestroyTerrainFromImage();
            if (_defaultWorld != null) {
                _defaultWorld.SetActive(true);
            }
        }
    }

    public void OnTerrainMinChanged(string value) {
        float min;
        if (float.TryParse(value, out min)) {
            _terrainFromElevationImage.minAltitude = min;
            _terrainFromElevationImage.UpdateSize();
        }
    }

    public void OnTerrainMaxChanged(string value) {
        float max;
        if (float.TryParse(value, out max)) {
            _terrainFromElevationImage.maxAltitude = max;
            _terrainFromElevationImage.UpdateSize();
        }
    }

    public void OnTerrainScaleChanged(string value) {
        float scale;
        if (float.TryParse(value, out scale)) {
            _terrainFromElevationImage.distanceBetweenPixels = scale;
            _terrainFromElevationImage.UpdateSize();
        }
    }

    public void OnTerrainGenerateButtonPressed() {
        OnTerrainFromFileToggleChange(true);
    }
}