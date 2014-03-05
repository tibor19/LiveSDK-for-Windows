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
        private readonly string _clientId = "0000000044007CD5";
        private readonly string _clientSecret = "CxQOxCRbxzDOnRiSinyTcexWayWE9Q7m";
        private readonly string _redirectUri = "https://login.live.com/oauth20_desktop.srf";

        private static readonly string authCodeFormatString =
            "https://login.live.com/oauth20_token.srf?grant_type=authorization_code&redirect_uri={0}&client_id={1}&client_secret={2}&code={3}";

        private static readonly string refreshTokenFormatString =
            "https://login.live.com/oauth20_token.srf?grant_type=refresh_token&redirect_uri={0}&client_id={1}&client_secret={2}&refresh_token={3}";

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }

        public LiveRestClient()
        {
            
        }

        public LiveRestClient(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public async Task InitAsync(string authorizationCode)
        {
            var requestString = String.Format(authCodeFormatString, HttpUtility.UrlEncode(_redirectUri), _clientId, _clientSecret, authorizationCode);
            await GetTokenResult(requestString);
        }

        public async Task RefreshTokensAsync()
        {
            var requestString = String.Format(refreshTokenFormatString, HttpUtility.UrlEncode(_redirectUri), _clientId, _clientSecret, RefreshToken);
            await GetTokenResult(requestString);
        }

        private async Task<TokenResult> GetTokenResult(string requestString)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(requestString);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TokenResult>(data);

            AccessToken = result.AccessToken;
            RefreshToken = result.RefreshToken;
            return result;
        }
    }
}
