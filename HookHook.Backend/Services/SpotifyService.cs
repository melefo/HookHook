﻿using HookHook.Backend.Entities;
using HookHook.Backend.Exceptions;
using HookHook.Backend.Models;
using HookHook.Backend.Utilities;
using SpotifyAPI.Web;

namespace HookHook.Backend.Services
{
    /// <summary>
    /// Service used by areaservice
    /// </summary>
	public class SpotifyService
	{
        /// <summary>
        /// Client ID
        /// </summary>
		private readonly string _id;
        /// <summary>
        /// Client secret
        /// </summary>
		private readonly string _secret;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Environment variables</param>
		public SpotifyService(IConfiguration config)
		{
			_id = config["Spotify:ClientId"];
			_secret = config["Spotify:ClientSecret"];
		}

        /// <summary>
        /// OAuth
        /// </summary>
        /// <param name="code">OAuth code</param>
        /// <returns>SpotifyClient, AuthorizationCodeToken</returns>
		public async Task<(SpotifyClient, AuthorizationCodeTokenResponse)> OAuth(string code, string redirect)
		{
			var client = new OAuthClient();
			var response = await client.RequestToken(
				new AuthorizationCodeTokenRequest(_id, _secret, code, new Uri(redirect))
			);

			if (response == null)
				throw new ApiException("Failed to call API");
			var spotify = new SpotifyClient(response.AccessToken);

			return (spotify, response);
		}

        /// <summary>
        /// Add new service account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns>New ServiceAccount</returns>
		public async Task<ServiceAccount?> AddAccount(User user, string code, string redirect)
        {
			(var client, var token) = await OAuth(code, redirect);
			var spotifyUser = await client.UserProfile.Current();
			var id = spotifyUser.Id;

			user.ServicesAccounts.TryAdd(Providers.Spotify, new());
			if (user.ServicesAccounts[Providers.Spotify].Any(x => x.UserId == id))
				return null;

			user.ServicesAccounts[Providers.Spotify].Add(new(id, token.AccessToken, TimeSpan.FromSeconds(token.ExpiresIn), token.RefreshToken));
			var current = await client.UserProfile.Current();
			return new(id, current.DisplayName);
		}

        /// <summary>
        /// Refresh spotify account tokens
        /// </summary>
        /// <param name="account"></param>
		public async Task Refresh(OAuthAccount account)
        {
			if (account.ExpiresIn == null || account.ExpiresIn > DateTime.UtcNow)
				return;
			if (account.RefreshToken == null)
				return;
			var client = new OAuthClient();
			var res = await client.RequestToken(
				new AuthorizationCodeRefreshRequest(_id, _secret, account.RefreshToken)
			);

			account.AccessToken = res.AccessToken;
			account.ExpiresIn = DateTime.UtcNow.Add(TimeSpan.FromSeconds(res.ExpiresIn));
		}
	}
}

