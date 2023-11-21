// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using Esri.ArcGISMapsSDK.Security;
using Esri.GameEngine.Security;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Security
{
	public abstract class SampleOAuthAuthenticationChallengeHandler : ArcGISAuthenticationChallengeHandler
	{	
		public override ArcGISAuthenticationChallengeType GetAuthenticationChallengeType()
		{
			return ArcGISAuthenticationChallengeType.ArcGISOAuthAuthenticationChallenge;
		}

		public override void HandleChallenge(ArcGISAuthenticationChallenge authenticationChallenge)
		{
			var oauthAuthenticationChallenge = authenticationChallenge as ArcGISOAuthAuthenticationChallenge;

			var authorizeURI = oauthAuthenticationChallenge.AuthorizeURI;
			
			HandleChallengeInternal(oauthAuthenticationChallenge.AuthorizeURI).ContinueWith(authorizationCodeTask =>
			{
				if (authorizationCodeTask.IsFaulted)
				{
					Debug.LogError(authorizationCodeTask.Exception.Message);

					oauthAuthenticationChallenge.Cancel();
				}
				else if (authorizationCodeTask.IsCanceled)
				{
					oauthAuthenticationChallenge.Cancel();
				}
				else
				{
					var authorizationCode = authorizationCodeTask.Result;

					if (authorizationCode != null)
					{
						oauthAuthenticationChallenge.Respond(authorizationCode);
					}
					else
					{
						oauthAuthenticationChallenge.Cancel();
					}
				}
			});
		}
		
		protected abstract Task<string> HandleChallengeInternal(string authorizeURI);
	}
}
