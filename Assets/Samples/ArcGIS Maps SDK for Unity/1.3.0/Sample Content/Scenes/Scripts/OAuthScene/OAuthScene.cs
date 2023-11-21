// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using Esri.ArcGISMapsSDK.Security;
using Esri.ArcGISMapsSDK.Utils;
using Esri.GameEngine.Security;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples
{
	[ExecuteAlways]
	public class OAuthScene : MonoBehaviour
	{
#if !UNITY_EDITOR
		private ArcGISAuthenticationChallengeHandler authenticationChallengeHandler;
#endif

		public string clientID = "Enter Client ID";
		public string redirectURIDesktop = "Enter Desktop Redirect URI";
		public string redirectURIMobile = "Enter Mobile Redirect URI";
		public string serviceURL = "EnterServiceURL";

		void Start()
		{
#if !UNITY_EDITOR
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
			authenticationChallengeHandler = new Security.SampleMobileOAuthAuthenticationChallengeHandler();
#else
			authenticationChallengeHandler = new Security.SampleDesktopOAuthAuthenticationChallengeHandler();
#endif

			Environment.SetAuthenticationChallengeHandler(authenticationChallengeHandler);
#endif

			ArcGISAuthenticationManager.AuthenticationConfigurations.Clear();

			// Named user login
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
			var authenticationConfiguration = new ArcGISOAuthAuthenticationConfiguration(clientID.Trim(), "", redirectURIMobile.Trim());
#else
			var authenticationConfiguration = new ArcGISOAuthAuthenticationConfiguration(clientID.Trim(), "", redirectURIDesktop.Trim());
#endif

			ArcGISAuthenticationManager.AuthenticationConfigurations.Add(serviceURL, authenticationConfiguration);
		}

#if !UNITY_EDITOR
		void OnDestroy()
		{
			if (authenticationChallengeHandler != null)
			{
				authenticationChallengeHandler.Dispose();
			}
		}
#endif
	}
}
