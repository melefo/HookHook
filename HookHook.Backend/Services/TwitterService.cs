﻿using System;
using CoreTweet;
using HookHook.Backend.Exceptions;

namespace HookHook.Backend.Services
{
	public class TwitterService
	{
		private readonly IConfiguration _configuration;
		private readonly MongoService _db;

		private readonly string _id;
		private readonly string _secret;
		private readonly string _redirect;

		private Dictionary<string, OAuth.OAuthSession> _OAuthSessions = new();

		public TwitterService(MongoService db, IConfiguration configuration)
		{
			_configuration = configuration;
			_db = db;

			_id = _configuration["Twitter:ClientId"];
			_secret = _configuration["Twitter:ClientSecret"];
			_redirect = _configuration["Twitter:Redirect"];
		}

		public string Authorize()
		{
			var session = CoreTweet.OAuth.Authorize(_id, _secret, _redirect);

			_OAuthSessions.Add(session.RequestToken, session);
			return session.AuthorizeUri.ToString();
		}

		public async Task<(UserResponse, Tokens)> OAuth(string code, string verifier)
		{
			if (!_OAuthSessions.TryGetValue(code, out var session))
				throw new ApiException("Failed to call API");
			var tokens = await CoreTweet.OAuth.GetTokensAsync(session, verifier);
			_OAuthSessions.Remove(code);

			if (tokens == null)
				throw new ApiException("Failed to call API");

			var twitter = await tokens.Users.ShowAsync(tokens.UserId);

			return (twitter, tokens);
		}

		public async Task AddAccount(Entities.User user, string code, string verifier)
        {
			(UserResponse twitter, Tokens tokens) = await OAuth(code, verifier);
			var id = twitter.Id.ToString();

			user.TwitterServices ??= new();
			if (user.TwitterServices.Any(x => x.UserId == id))
				return;

			user.TwitterServices.Add(new(id, tokens.AccessToken, tokens.AccessTokenSecret));
			_db.SaveUser(user);
			
		}
	}
}

