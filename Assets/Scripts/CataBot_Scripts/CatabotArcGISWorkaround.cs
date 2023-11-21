using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CatabotArcGISWorkaround : MonoBehaviour
{
    [SerializeField]
    private ArcGISMapComponent _arcGisMap;
    [SerializeField]
    private ArcGISLocationComponent _arcGisLocation;
    [SerializeField]
    private GameObject _catabotPrefab;
    [SerializeField]
    private Vector3 _catabotSpawnLocation = new Vector3(0, -0.88f, 0);
    [SerializeField]
    private WaterSurface _water;
    [SerializeField]
    private WindEmitterForRigidbody _wind;

    private void Start() {
        _arcGisMap.OriginPosition = new ArcGISPoint(_arcGisMap.OriginPosition.X, _arcGisMap.OriginPosition.Y, 0);
        _arcGisLocation.Position = new ArcGISPoint(_arcGisLocation.Position.X, _arcGisLocation.Position.Y, 0);

        Transform catabot = Instantiate(_catabotPrefab, _catabotSpawnLocation, Quaternion.identity).transform;
        catabot.GetComponent<WindReceiverForRigidbody>().wind = _wind;
        DetectWaterHeight[] waterHeights = catabot.GetComponentsInChildren<DetectWaterHeight>();
        print("Floater count: " + waterHeights.Length);
        foreach(DetectWaterHeight height in waterHeights) {
            height.water = _water;
        }
    }
}
