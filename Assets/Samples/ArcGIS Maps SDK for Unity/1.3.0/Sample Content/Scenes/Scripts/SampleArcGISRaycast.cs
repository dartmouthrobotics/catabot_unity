// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.UI;

public class SampleArcGISRaycast : MonoBehaviour
{
	public ArcGISMapComponent arcGISMapComponent;
	public ArcGISCameraComponent arcGISCamera;
	public Canvas canvas;
	public Text featureText;

	private void OnEnable()
	{
#if ENABLE_INPUT_SYSTEM
		Debug.Log("ArcGISRaycast sample is not configured to work with the new input manager package");
		enabled = false;
#endif
	}

	void Update()
    {
#if !ENABLE_INPUT_SYSTEM
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
				var layer = arcGISRaycastHit.layer;
				var featureId = arcGISRaycastHit.featureId;

				if (layer != null && featureId != -1)
				{
					featureText.text = featureId.ToString();

					var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
					var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + 200, geoPosition.SpatialReference);

					var rotation = arcGISCamera.GetComponent<ArcGISLocationComponent>().Rotation;
					var location = canvas.GetComponent<ArcGISLocationComponent>();
					location.Position = offsetPosition;
					location.Rotation = rotation;
				}
			}
		}
#endif
	}
}
