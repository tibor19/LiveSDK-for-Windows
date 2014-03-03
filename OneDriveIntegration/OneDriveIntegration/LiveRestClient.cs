using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneDriveIntegration
{
    public class LiveRestClient
    {
        private string _authorizeToken;
        private string _refreshToken;

        public LiveRestClient()
        {
            
        }

        public LiveRestClient(string authorizeToken, string refreshToken)
        {
            _authorizeToken = authorizeToken;
            _refreshToken = refreshToken;
        }

        public async Task<string> GetTokensAsync(string authorizationCode)
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://login.live.com/oauth20_token.srf?redirect_uri=https%3a%2f%2flogin.live.com%2foauth20_desktop.srf&grant_type=authorization_code&client_id=0000000044007CD5&client_secret=CxQOxCRbxzDOnRiSinyTcexWayWE9Q7m&code=" + authorizationCode);
            var data = await response.Content.ReadAsStringAsync();
            //var serializer = new JsonSerializer();
            //serializer.
            return data;
        }



    }
}
