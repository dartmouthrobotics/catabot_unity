// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using Esri.ArcGISMapsSDK.Security;
using Esri.ArcGISMapsSDK.Utils;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Security
{
	[ExecuteAlways]
	public class OAuthChallengeHandlersInitializer : MonoBehaviour
	{
		private ArcGISAuthenticationChallengeHandler authenticationChallengeHandler;

		private void Awake()
		{
	#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA) && !UNITY_EDITOR
			authenticationChallengeHandler = new Security.SampleMobileOAuthAuthenticationChallengeHandler();
	#else
			authenticationChallengeHandler = new Security.SampleDesktopOAuthAuthenticationChallengeHandler();
	#endif

			Environment.SetAuthenticationChallengeHandler(authenticationChallengeHandler);
		}

		private void OnDestroy()
		{
			if (authenticationChallengeHandler != null)
			{
				authenticationChallengeHandler.Dispose();
			}
		}
	}
}
