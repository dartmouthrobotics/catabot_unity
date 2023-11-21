// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using Esri.ArcGISMapsSDK.Security;
using Esri.GameEngine.Security;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Security
{
	public class SampleMobileOAuthAuthenticationChallengeHandler : SampleOAuthAuthenticationChallengeHandler
	{
		protected override Task<string> HandleChallengeInternal(string authorizeURI)
		{
			var matches = Regex.Matches(authorizeURI, @"redirect_uri=([^&]*)", RegexOptions.IgnoreCase);

			if (matches.Count != 1)
			{
				return Task.FromException<string>(new ArgumentException("Invalid authorize URI"));
			}

			var redirectURI = matches[0].Groups[1].Value;

			if (redirectURI == "urn:ietf:wg:oauth:2.0:oob")
			{
				return Task.FromException<string>(new ArgumentException("\"urn:ietf:wg:oauth:2.0:oob\" is not a supported redirect URI"));
			}

			try
			{
				var uri = new Uri(redirectURI);

				if (uri.Scheme == "http" || uri.Scheme == "https")
				{
					return Task.FromException<string>(new ArgumentException("Invalid redirect URI"));
				}
				else
				{
					redirectURI = uri.ToString();
				}
			}
			catch
			{
				return Task.FromException<string>(new ArgumentException("Invalid redirect URI"));
			}

			var taskCompletionSource = new TaskCompletionSource<string>();

			Application.deepLinkActivated += delegate (string url)
			{
				matches = Regex.Matches(url, @"\?code=([^&]*)", RegexOptions.IgnoreCase);

				if (matches.Count != 1)
				{
					taskCompletionSource.SetException(new ArgumentException("Invalid response from the authentication server"));
				}
				else
				{
					var code = matches[0].Groups[1].Value;

					taskCompletionSource.SetResult(code);
				}
			};

			Application.OpenURL(authorizeURI);

			return taskCompletionSource.Task;
		}
	}
}
