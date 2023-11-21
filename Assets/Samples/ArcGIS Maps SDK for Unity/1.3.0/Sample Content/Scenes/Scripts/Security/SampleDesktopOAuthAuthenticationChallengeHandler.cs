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
	public class SampleDesktopOAuthAuthenticationChallengeHandler : SampleOAuthAuthenticationChallengeHandler
	{	
		private HttpListener httpListener;
		
		public new void Dispose()
		{
			if (httpListener != null)
			{
				httpListener.Stop();
			}
		}
		
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

				if (uri.Scheme == "http" && uri.Host == "localhost")
				{
					redirectURI = uri.ToString();
				}
				else
				{
					return Task.FromException<string>(new ArgumentException("Invalid redirect URI"));
				}
			}
			catch
			{
				return Task.FromException<string>(new ArgumentException("Invalid redirect URI"));
			}

			var httpListenerPrefix = redirectURI;

			if (!httpListenerPrefix.EndsWith("/"))
			{
				httpListenerPrefix += "/";
			}

			httpListener = new HttpListener();
			httpListener.Prefixes.Add(httpListenerPrefix);
			httpListener.Start();

			var taskCompletionSource = new TaskCompletionSource<string>();

			httpListener.GetContextAsync().ContinueWith(task =>
			{
				if (!task.IsCompleted)
				{
					return;
				}

				var context = task.Result;

				context.Response.Close();

				httpListener.Stop();

				if (context.Request.QueryString.Get("error") != null)
				{
					taskCompletionSource.SetException(new Exception(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error"))));
				}
				else
				{
					var code = context.Request.QueryString.Get("code");

					taskCompletionSource.SetResult(code);
				}
			});

			Application.OpenURL(authorizeURI);

			return taskCompletionSource.Task;
		}
	}
}
