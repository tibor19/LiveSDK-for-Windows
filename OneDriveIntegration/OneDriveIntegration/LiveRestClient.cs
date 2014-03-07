using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using OneDriveIntegration.JsonData;

namespace OneDriveIntegration
{
    public class LiveRestClient
    {
        private const string RedirectUri = "https://login.live.com/oauth20_desktop.srf";
        private const string AuthCodeFormatString = "https://login.live.com/oauth20_token.srf?grant_type=authorization_code&redirect_uri={0}&client_id={1}&client_secret={2}&code={3}";
        private const string RefreshTokenFormatString = "https://login.live.com/oauth20_token.srf?grant_type=refresh_token&redirect_uri={0}&client_id={1}&client_secret={2}&refresh_token={3}";

        private readonly string _clientId;
        private readonly string _clientSecret;

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }

        public LiveRestClient(string clientId, string clientSecret) : this(clientId, clientSecret, null)
        {
            
        }

        public LiveRestClient(string clientId, string clientSecret, string refreshToken)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            RefreshToken = refreshToken;
        }

        public async Task InitAsync(string authorizationCode)
        {
            var requestString = String.Format(AuthCodeFormatString, HttpUtility.UrlEncode(RedirectUri), _clientId, _clientSecret, authorizationCode);
            await GetTokenResult(requestString);
        }

        public async Task RefreshTokensAsync()
        {
            var requestString = String.Format(RefreshTokenFormatString, HttpUtility.UrlEncode(RedirectUri), _clientId, _clientSecret, RefreshToken);
            await GetTokenResult(requestString);
        }

        private async Task<TokenResult> GetTokenResult(string requestString)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(requestString);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var result = JsonConvert.DeserializeObject<TokenResult>(data);

            AccessToken = result.AccessToken;
            RefreshToken = result.RefreshToken;
            return result;
        }
    }
}
