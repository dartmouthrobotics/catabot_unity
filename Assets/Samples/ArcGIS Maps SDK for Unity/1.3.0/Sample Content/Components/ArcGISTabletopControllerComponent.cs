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
using Esri.ArcGISMapsSDK.Utils;
using Esri.GameEngine.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Components
{
	[DisallowMultipleComponent]
	[ExecuteAlways]
	[AddComponentMenu("ArcGIS Maps SDK/Samples/ArcGIS Tabletop Controller")]
	public class ArcGISTabletopControllerComponent : MonoBehaviour
	{
		public Transform TransformWrapper;
		public ArcGISMapComponent MapComponent;
		public ArcGISLocationComponent CameraComponent;

		private ArcGISPoint lastCenter;
		private double? lastElevationOffset;
		private double? lastRadius;

		[SerializeField]
		[OnChangedCall("OnCenterChanged")]
		private ArcGISPoint center = new ArcGISPoint(0, 0, 0, ArcGISSpatialReference.WGS84());
		public ArcGISPoint Center
		{
			get => center;
			set
			{
				if (center != value)
				{
					center = value;
					OnCenterChanged();
				}
			}
		}

		[SerializeField]
		[OnChangedCall("OnElevationOffsetChanged")]
		private double elevationOffset;
		public double ElevationOffset
		{
			get => elevationOffset;
			set
			{
				if (elevationOffset != value)
				{
					elevationOffset = value;
					OnElevationOffsetChanged();
				}
			}
		}

		[SerializeField]
		[OnChangedCall("OnRadiusChanged")]
		private double radius;
		public double Radius
		{
			get => radius;
			set
			{
				if (radius != value)
				{
					radius = value;
					OnRadiusChanged();
				}
			}
		}

		internal void OnCenterChanged()
		{
			PreUpdateTabletop();
		}

		private void OnDisable()
		{
			MapComponent.ExtentUpdated -= new ArcGISExtentUpdatedEventHandler(PostUpdateTabletop);
		}

		internal void OnElevationOffsetChanged()
		{
			PreUpdateTabletop();
		}

		private void OnEnable()
		{
			MapComponent.ExtentUpdated += new ArcGISExtentUpdatedEventHandler(PostUpdateTabletop);

			lastCenter = null;
			lastElevationOffset = null;
			lastRadius = null;

			PreUpdateTabletop();
		}

		internal void OnRadiusChanged()
		{
			PreUpdateTabletop();
		}

		private void PostUpdateTabletop(ArcGISExtentUpdatedEventArgs e)
		{
			if (!e.Type.HasValue)
			{
				return;
			}

			var areaMin = e.AreaMin.Value;
			var areaMax = e.AreaMax.Value;

			// Adjust center and scale only after all tiles were updated
			var width = areaMax.x - areaMin.x;
			var height = areaMax.z - areaMin.z;
			var centerPosition = new double3(areaMin.x + width / 2.0, 0, areaMin.z + height / 2.0);

			MapComponent.OriginPosition = MapComponent.View.WorldToGeographic(centerPosition);

			var scale = 1.0 / width;

			TransformWrapper.localScale = new Vector3((float)scale, (float)scale, (float)scale);

			UpdateOffset();

			// We need to force an HP Root update to account for scale changes
			MapComponent.UpdateHPRoot();
		}

		private void PreUpdateTabletop()
		{
			bool needsOffsetUpdate = lastElevationOffset != ElevationOffset;
			bool needsExtentUpdate = lastCenter != Center || lastRadius != Radius;

			if (!needsExtentUpdate && !needsOffsetUpdate)
			{
				return;
			}

			if (needsExtentUpdate)
			{
				var newExtent = new ArcGISExtentInstanceData()
				{
					GeographicCenter = (ArcGISPoint)Center.Clone(),
					ExtentShape = MapExtentShapes.Circle,
					ShapeDimensions = new double2(Radius, Radius)
				};

				if (MapComponent.Extent == newExtent)
				{
					MapComponent.OriginPosition = newExtent.GeographicCenter;

					var scale = 1.0 / (2 * Radius);

					TransformWrapper.localScale = new Vector3((float)scale, (float)scale, (float)scale);
				}
				else
				{
					MapComponent.Extent = new ArcGISExtentInstanceData()
					{
						GeographicCenter = (ArcGISPoint)Center.Clone(),
						ExtentShape = MapExtentShapes.Circle,
						ShapeDimensions = new double2(Radius, Radius)
					};
				}

				CameraComponent.Position = new ArcGISPoint(Center.X, Center.Y, Radius, Center.SpatialReference);

				lastCenter = (ArcGISPoint)Center.Clone();
				lastRadius = Radius;
			}

			if (needsOffsetUpdate)
			{
				UpdateOffset();

				lastElevationOffset = ElevationOffset;
			}
		}

		/// <summary>
		/// Casts the ray against an horizontal plane plane centered at us and returns whether they intersected it.
		/// Returns in currentPoint the point relative to us in Universe space that intersected the plane centered at us
		/// </summary>
		/// <param name="ray">The ray to test.</param>
		/// <param name="currentPoint">The point relative to us in Universe space that intersected the plane centered at us.</param>
		/// <returns>True if the ray intersects the plane, false otherwise.</returns>
		/// <since>1.3.0</since>
		private bool RaycastRelativeToCenter(Ray ray, out Vector3 currentPoint)
		{
			var planeCenter = MapComponent.transform.localToWorldMatrix.MultiplyPoint(Vector3.zero);
			var dragStartPlane = new Plane(Vector3.up, planeCenter);

			float dragStartEntry;
			if (dragStartPlane.Raycast(ray, out dragStartEntry))
			{
				currentPoint = MapComponent.transform.worldToLocalMatrix.MultiplyPoint(ray.GetPoint(dragStartEntry));

				return true;
			}
			else
			{
				currentPoint = Vector3.positiveInfinity;
			}

			return false;
		}

		/// <summary>
		/// Casts the ray against an horizontal plane plane centered at us and returns whether they intersected it inside the extent.
		/// Returns in currentPoint the point relative to us in Universe space that intersected the plane centered at us
		/// </summary>
		/// <param name="ray">The ray to test.</param>
		/// <param name="currentPoint">The point relative to us in Universe space that intersected the plane centered at us.</param>
		/// <returns>True if the ray intersects the plane, false otherwise.</returns>
		/// <since>1.3.0</since>
		public bool Raycast(Ray ray, out Vector3 currentPoint)
		{
			RaycastRelativeToCenter(ray, out currentPoint);

			bool insideRadius = Vector3.Distance(currentPoint, Vector3.zero) <= Radius;

			currentPoint = MapComponent.transform.localToWorldMatrix.MultiplyPoint(currentPoint);
			currentPoint = transform.worldToLocalMatrix.MultiplyPoint(currentPoint);

			return insideRadius;
		}

		private void Update()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			PreUpdateTabletop();
		}

		private void UpdateOffset()
		{
			var newPosition = TransformWrapper.localPosition;

			newPosition.y = (float)(ElevationOffset * TransformWrapper.localScale.x);

			TransformWrapper.localPosition = newPosition;
		}
	}
}
