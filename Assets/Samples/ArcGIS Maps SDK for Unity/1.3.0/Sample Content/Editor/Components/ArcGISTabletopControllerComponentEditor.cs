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
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Editor.Components
{
	[CustomEditor(typeof(ArcGISTabletopControllerComponent))]
	public class ArcGISTabletopControllerComponentEditor : UnityEditor.Editor
	{
		private static class Styles
		{
			public static readonly GUIContent CameraComponent = EditorGUIUtility.TrTextContent("Camera Component", "The Camera Component used to load data.");
			public static readonly GUIContent Center = EditorGUIUtility.TrTextContent("Center", "The center (in geographic coordinates) of the tabletop.");
			public static readonly GUIContent ElevationOffset = EditorGUIUtility.TrTextContent("Elevation Offset", "The vertical offset (in meters) applied to the tabletop.");
			public static readonly GUIContent MapComponent = EditorGUIUtility.TrTextContent("Map Component", "The ArcGIS Map.");
			public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "The radius (in meters) of the tabletop.");
			public static readonly GUIContent TransformWrapper = EditorGUIUtility.TrTextContent("Transform Wrapper", "The GameObject wrapping the ArcGIS Map used for scaling and offset.");
		}

		private void DrawPropertyField(string propertyName, GUIContent label)
		{
			var property = serializedObject.FindProperty(propertyName);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property, label);
			if (!EditorGUI.EndChangeCheck())
			{
				return;
			}

			property.serializedObject.ApplyModifiedProperties();

			var fieldInfo = serializedObject.targetObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(fieldInfo => fieldInfo.Name == propertyName).FirstOrDefault();

			Debug.Assert(fieldInfo != null);

			var onChangedCallAttribute = fieldInfo.GetCustomAttributes<OnChangedCallAttribute>().FirstOrDefault();

			if (onChangedCallAttribute == null)
			{
				return;
			}

			var method = serializedObject.targetObject.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(methodInfo => methodInfo.Name == onChangedCallAttribute.OnChangedHandler).FirstOrDefault();

			if (method == null)
			{
				return;
			}

			method.Invoke(property.serializedObject.targetObject, null);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawPropertyField("TransformWrapper", Styles.TransformWrapper);
			DrawPropertyField("MapComponent", Styles.MapComponent);
			DrawPropertyField("CameraComponent", Styles.CameraComponent);
			DrawPropertyField("center", Styles.Center);
			DrawPropertyField("radius", Styles.Radius);
			DrawPropertyField("elevationOffset", Styles.ElevationOffset);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
