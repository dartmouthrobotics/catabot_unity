// COPYRIGHT 1995-2023 ESRI
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
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Components
{
	[DisallowMultipleComponent]
	[AddComponentMenu("ArcGIS Maps SDK/Samples/ArcGIS Tabletop Input Controller")]
	public class ArcGISTabletopInputControllerComponent : MonoBehaviour
	{
		public ArcGISTabletopControllerComponent tabletopControllerComponent;

		private Vector3 dragStartPoint = Vector3.zero;
		private double4x4 dragStartWorldMatrix;
		private bool isDragging = false;
		private bool isZooming = false;
		private HPRoot hpRoot;
		private ArcGISMapComponent mapComponent;
		private float zoomStartDistance = 0;
		private const double zoomScalar = 20;

		public void EndPointDrag()
		{
			isDragging = false;
		}

		public void EndTwoPointZoom()
		{
			isZooming = false;
		}

		private void OnEnable()
		{
			hpRoot = FindObjectOfType<HPRoot>();
			mapComponent = FindObjectOfType<ArcGISMapComponent>();
		}

		public void StartPointDrag(Vector3 screenPoint)
		{
			Vector3 dragCurrentPoint;
			var dragStartRay = Camera.main.ScreenPointToRay(screenPoint);

			if (tabletopControllerComponent.Raycast(dragStartRay, out dragCurrentPoint))
			{
				isDragging = true;
				dragStartPoint = dragCurrentPoint;

				// Save the matrix to go from Local space to Universe space
				// As the origin location will be changing during drag, we keep the transform we had when the action started
				dragStartWorldMatrix = math.mul(math.inverse(hpRoot.WorldMatrix), tabletopControllerComponent.transform.localToWorldMatrix.ToDouble4x4());
			}
		}

		public void StartTwoPointZoom(Vector3 position0, Vector3 position1)
		{
			Vector3 zoomCurrentPoint0;
			Vector3 zoomCurrentPoint1;
			var zoomStartRay0 = Camera.main.ScreenPointToRay(position0);
			var zoomStartRay1 = Camera.main.ScreenPointToRay(position1);

			if (tabletopControllerComponent.Raycast(zoomStartRay0, out zoomCurrentPoint0) && tabletopControllerComponent.Raycast(zoomStartRay1, out zoomCurrentPoint1))
			{
				isZooming = true;

				zoomStartDistance = Vector3.Distance(zoomCurrentPoint0, zoomCurrentPoint1);
			}
		}

		private void Update()
		{
			// Handle mouse inputs.
			if (Input.touchCount < 2)
			{
				if (Input.GetMouseButtonDown(0))
				{
					StartPointDrag(Input.mousePosition);
				}
				else if (Input.GetMouseButton(0))
				{
					UpdatePointDrag(Input.mousePosition);
				}
				else if (Input.GetMouseButtonUp(0))
				{
					EndPointDrag();
				}

				if (Input.mouseScrollDelta.y != 0.0)
				{
					var zoom = Mathf.Sign(Input.mouseScrollDelta.y);
					ZoomMap(zoom, Input.mousePosition);
				}
			}
			// Handle two-finger inputs.
			else if (Input.touchCount == 2)
			{
				var touch0 = Input.GetTouch(0);
				var touch1 = Input.GetTouch(1);

				if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
				{
					StartTwoPointZoom(touch0.position, touch1.position);
				}
				else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
				{
					UpdateTwoPointZoom(touch0.position, touch1.position);
				}
				else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
				{
					EndTwoPointZoom();
				}
			}
		}

		public void UpdatePointDrag(Vector3 screenPoint)
		{
			if (isDragging)
			{
				var updateRay = Camera.main.ScreenPointToRay(screenPoint);

				Vector3 dragCurrentPoint;
				tabletopControllerComponent.Raycast(updateRay, out dragCurrentPoint);

				var diff = dragStartPoint - dragCurrentPoint;
				var newExtentCenterCartesian = dragStartWorldMatrix.HomogeneousTransformPoint(diff.ToDouble3());
				var newExtentCenterGeographic = mapComponent.View.WorldToGeographic(new double3(newExtentCenterCartesian.x, newExtentCenterCartesian.y, newExtentCenterCartesian.z));

				tabletopControllerComponent.Center = newExtentCenterGeographic;
			}
		}

		public void UpdateTwoPointZoom(Vector3 position0, Vector3 position1)
		{
			if (isZooming)
			{
				Vector3 zoomCurrentPoint0;
				Vector3 zoomCurrentPoint1;
				var zoomRay0 = Camera.main.ScreenPointToRay(position0);
				var zoomRay1 = Camera.main.ScreenPointToRay(position1);

				if (tabletopControllerComponent.Raycast(zoomRay0, out zoomCurrentPoint0) && tabletopControllerComponent.Raycast(zoomRay1, out zoomCurrentPoint1))
				{
					var zoomCurrentDistance = Vector3.Distance(zoomCurrentPoint0, zoomCurrentPoint1);
					var diff = zoomCurrentDistance - zoomStartDistance;

					// More zoom means smaller extent
					tabletopControllerComponent.Radius -= diff * tabletopControllerComponent.Radius / 10;
				}
			}
		}

		public void ZoomMap(float zoom, Vector3 screenPoint)
		{
			if (zoom == 0)
			{
				return;
			}

			Vector3 outPoint;
			var zoomRay = Camera.main.ScreenPointToRay(screenPoint);

			if (tabletopControllerComponent.Raycast(zoomRay, out outPoint))
			{
				var speed = tabletopControllerComponent.Radius / zoomScalar;

				// More zoom means smaller extent
				tabletopControllerComponent.Radius -= zoom * speed;
			}
		}
	}
}
