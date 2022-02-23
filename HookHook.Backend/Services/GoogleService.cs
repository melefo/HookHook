using HookHook.Backend.Entities;
using HookHook.Backend.Exceptions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using static Google.Apis.YouTube.v3.YouTubeService;
using HookHook.Backend.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using HookHook.Backend.Models;

namespace HookHook.Backend.Services
{
    public class GoogleAuth
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expire_in")]
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }

    public class GoogleProfile
    {
        public string Email { get; set; }
        public string Id { get; set; }

        public GoogleProfile(string id, string email)
        {
            Id = id;
            Email = email;
        }
    }

    public class GoogleService
    {
        private readonly MongoService _db;

        private readonly string _id;
        private readonly string _secret;
        private readonly string _key;
        private readonly string _redirect;

        private readonly HttpClient _client = new();

        /// <summary>
        /// Youtube Service Constructor
        /// </summary>
        /// <param name="config"></param>
        public GoogleService(MongoService db, IConfiguration config)
        {
            _db = db;

            _id = config["Google:ClientId"];
            _secret = config["Google:ClientSecret"];
            _key = config["Google:ApiKey"];
            _redirect = config["Google:Redirect"];
        }

        /// <summary>
        /// Create a Youtube Widget from a Google account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public YouTubeService CreateYouTube(OAuthAccount account)
        {
            return new(new BaseClientService.Initializer()
            {
                ApiKey = _key,
                HttpClientInitializer = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _id,
                        ClientSecret = _secret
                    },
                    // ! jsp quel scope permet de commenter quelque chose
                    Scopes = new[] { Scope.YoutubeReadonly, Scope.Youtube, Scope.Youtubepartner, Scope.YoutubeUpload, Scope.YoutubeForceSsl }
                }), account.UserId, new TokenResponse()
                {
                    RefreshToken = account.RefreshToken,
                })
            });
        }

        public async Task<(GoogleProfile, GoogleAuth)> OAuth(string code)
        {
            var res = await _client.PostAsync<GoogleAuth>($"https://oauth2.googleapis.com/token?code={code}&client_id={_id}&client_secret={_secret}&redirect_uri={_redirect}&grant_type=authorization_code");

            if (res == null)
                throw new ApiException("Failed to call API");

            JwtSecurityTokenHandler tokenHandler = new();
            string? id = tokenHandler.ReadJwtToken(res.IdToken).Payload["sub"].ToString();
            string? email = tokenHandler.ReadJwtToken(res.IdToken).Payload["email"].ToString();

            if (id == null || email == null)
                throw new ApiException("API did not return the necessary arguments");

            return (new GoogleProfile(id, email), res);
        }

        public async Task<ServiceAccount?> AddAccount(User user, string code)
        {
            (var profile, var res) = await OAuth(code);

            user.ServicesAccounts.TryAdd(Providers.Google, new());
            if (user.ServicesAccounts[Providers.Google].Any(x => x.UserId == profile.Id))
                return null;

            OAuthAccount oauth = new(profile.Id, res.AccessToken, TimeSpan.FromSeconds(res.ExpiresIn), res.RefreshToken);
            user.ServicesAccounts[Providers.Google].Add(oauth);
            _db.SaveUser(user);

            var youtube = CreateYouTube(oauth);
            var req = youtube.Channels.List("snippet");
            req.Mine = true;
            var search = req.Execute();

            return new(profile.Id, search.Items[0].Snippet.Title);
        }

        public async Task Refresh(OAuthAccount account)
        {
            if (account.ExpiresIn == null || account.ExpiresIn > DateTime.UtcNow)
                return;
            if (account.RefreshToken == null)
                return;

            var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("client_id", _id),
                new("client_secret", _secret),
                new("grant_type", "refresh_token"),
                new("refresh_token", account.RefreshToken),
            });

            var res = await _client.PostAsync<GoogleAuth>($"https://oauth2.googleapis.com/token", content);
            account.AccessToken = res.AccessToken;
            account.ExpiresIn = DateTime.UtcNow.Add(TimeSpan.FromSeconds(res.ExpiresIn));
        }
    }
}