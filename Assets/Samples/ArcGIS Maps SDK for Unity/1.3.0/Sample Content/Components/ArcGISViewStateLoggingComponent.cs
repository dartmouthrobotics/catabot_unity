// COPYRIGHT 1995-2022 ESRI
// TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
// Unpublished material - all rights reserved under the
// Copyright Laws of the United States and applicable international
// laws, treaties, and conventions.
//
// For additional information, contact:
// Attn: Contracts and Legal Department
// Environmental Systems Research Institute, Inc.
// 380 New York Street
// Redlands, California 92373
// USA
//
// email: legal@esri.com
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.View.State;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Components
{
	[DisallowMultipleComponent]
	[ExecuteAlways]
	[AddComponentMenu("ArcGIS Maps SDK/Samples/ArcGIS View State Logging")]
	public class ArcGISViewStateLoggingComponent : MonoBehaviour
	{
		public bool enableLogging = true;

		private ArcGISMapComponent mapComponent;

		private void OnEnable()
		{
			mapComponent = GetComponentInParent<ArcGISMapComponent>();

			if (mapComponent == null)
			{
				Debug.LogError("Unable to find a parent ArcGISMapComponent.");

				enabled = false;
				return;
			}

			SubscribeToViewStateEvents();
		}

		// Remove subscribers to events so messages are no longer logged when the component is removed
		private void OnDisable()
		{
			mapComponent.View.ViewStateChanged = null;
			mapComponent.View.ElevationSourceViewStateChanged = null;
			mapComponent.View.LayerViewStateChanged = null;
		}

		// You can subscribe to these events to show information about the view state and log warnings in the console
		// Logs usually describe events such as if the data is loading, if the data's state is changed, or if there's an error processing the data
		// You only need to subscribe to them once as long as you don't unsubscribe
		private void SubscribeToViewStateEvents()
		{
			if (mapComponent == null || mapComponent.View == null)
			{
				return;
			}

			// This event logs updates on the Elevation source data
			mapComponent.View.ElevationSourceViewStateChanged += (Esri.GameEngine.Elevation.Base.ArcGISElevationSource layer, ArcGISElevationSourceViewState elevationSourceViewState) =>
			{
				var message = elevationSourceViewState.Message?.GetMessage();
				var status = elevationSourceViewState.Status;

				var statusString = "ArcGISElevationSourceViewState " + layer.Name + " changed to : " + status.ToString();

				if ((status.HasFlag(ArcGISElevationSourceViewStatus.Error) || status.HasFlag(ArcGISElevationSourceViewStatus.Warning)) && message != null)
				{
					statusString += " (" + message + ")";

					var additionalInfo = elevationSourceViewState.Message.GetAdditionalInformation();
					string additionalMessage = "";
					additionalInfo.TryGetValue("Additional Message", out additionalMessage);

					if (additionalMessage != null && additionalMessage != "")
					{
						statusString += "\nAdditional info: " + additionalMessage;
					}
				}

				if (enableLogging)
				{
					Debug.Log(statusString);
				}
			};

			// This event logs changes to the layers' statuses
			mapComponent.View.LayerViewStateChanged += (Esri.GameEngine.Layers.Base.ArcGISLayer layer, ArcGISLayerViewState layerViewState) =>
			{
				var message = layerViewState.Message?.GetMessage();
				var status = layerViewState.Status;

				var statusString = "ArcGISLayerViewState " + layer.Name + " changed to : " + status.ToString();

				if ((status.HasFlag(ArcGISLayerViewStatus.Error) || status.HasFlag(ArcGISLayerViewStatus.Warning)) && message != null)
				{
					statusString += " (" + message + ")";

					var additionalInfo = layerViewState.Message.GetAdditionalInformation();
					string additionalMessage = "";
					additionalInfo.TryGetValue("Additional Message", out additionalMessage);

					if (additionalMessage != null && additionalMessage != "")
					{
						statusString += "\nAdditional info: " + additionalMessage;
					}
				}

				if (enableLogging)
				{
					Debug.Log(statusString);
				}
			};

			// This event logs the View's overall status
			mapComponent.View.ViewStateChanged += (ArcGISViewState viewState) =>
			{
				var message = viewState.Message?.GetMessage();
				var status = viewState.Status;

				var statusString = "ArcGISViewState changed to : " + status.ToString();

				if ((status.HasFlag(ArcGISViewStatus.Error) || status.HasFlag(ArcGISViewStatus.Warning)) && message != null)
				{
					statusString += " (" + message + ")";

					var additionalInfo = viewState.Message.GetAdditionalInformation();
					string additionalMessage = "";
					additionalInfo.TryGetValue("Additional Message", out additionalMessage);

					if (additionalMessage != null && additionalMessage != "")
					{
						statusString += "\nAdditional info: " + additionalMessage;
					}
				}

				if (enableLogging)
				{
					Debug.Log(statusString);
				}
			};

			mapComponent.View.SpatialReferenceChanged += () =>
			{
				var spatialReference = mapComponent.View.SpatialReference;

				if (spatialReference != null)
				{
					if (enableLogging)
					{
						Debug.Log("SpatialReference changed to : " + spatialReference.WKID.ToString());
					}
				}
			};
		}

	}
}
