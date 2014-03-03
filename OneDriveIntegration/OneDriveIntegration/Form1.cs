using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Live;
using Newtonsoft.Json;
using OneDriveIntegration.JsonData;
using OneDriveIntegration.Properties;

namespace OneDriveIntegration
{
    public partial class Form1 : Form
    {
        // private OneDriveClient _client;

        class RefreshHandler : IRefreshTokenHandler
        {
            public Task SaveRefreshTokenAsync(RefreshTokenInfo tokenInfo)
            {
                Settings.Default.RefreshToken = tokenInfo.RefreshToken;
                Settings.Default.UserId = tokenInfo.UserId;
                return Task.Delay(0);
            }

            public Task<RefreshTokenInfo> RetrieveRefreshTokenAsync()
            {
                if (!string.IsNullOrEmpty(Settings.Default.RefreshToken))
                    return Task.FromResult<RefreshTokenInfo>(new RefreshTokenInfo(Settings.Default.RefreshToken));
                else
                {
                    return Task.FromResult<RefreshTokenInfo>(null);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var lclient = new LiveAuthClient("0000000044007CD5", new RefreshHandler());
            var session = lclient.IntializeAsync();

            //const string clientId = @"0000000044007CD5";
            //const string clientSecret = "CxQOxCRbxzDOnRiSinyTcexWayWE9Q7m";
            //const string endUrl = "https://login.live.com/oauth20_token.srf";
            //var authClient = new LiveAuthClient(clientId, clientSecret, endUrl);
            //var loginUrl = authClient.GetLoginUrl(new[] {"wl.skydrive", "wl.offline_access", "wl.skydrive_update"});
            //var result = await authClient.InitializeSessionAsync(loginUrl);
            //await authClient.ExchangeAuthCodeAsync();
            
            // authClient.
            // var startUrl =  authClient.GetLoginUrl(new []{"wl.signin", "wl.skydrive", "wl.offline_access", "wl.skydrive_update"});
            //var connectClient = new LiveConnectClient(result.Session);
            //_client = new OneDriveClient(connectClient);

            var authForm = new LiveAuthForm(null);
            if (authForm.ShowDialog(this) == DialogResult.OK)
            {
                // MessageBox.Show(authForm.AuthorizeCode);
                // var session = await authClient.ExchangeAuthCodeAsync(authForm.AuthorizeCode);
                //var restClient = new HttpClient();
                //var response = await restClient.GetAsync("https://apis.live.net/v5.0/folder.d0396257098c3b52/files?access_token=" +
                //                    session.AccessToken);
                var client = new LiveRestClient();
                var jsonData = await client.GetTokensAsync(authForm.AuthorizeCode);
                var serializer = JsonSerializer.Create();

                var reader = new JsonTextReader(new StringReader(jsonData));
                var data = serializer.Deserialize<TokenResult>(reader);
                MessageBox.Show(data.RefreshToken);
            }
        }

        private async void btnGetId_Click(object sender, EventArgs e)
        {
            //if (_client != null)
            //{
            //    MessageBox.Show(await _client.GetFileIdAsync(txtSource.Text));
            //}
        }

        private async void btnCopy_Click(object sender, EventArgs e)
        {
            //if (_client != null)
            //{
            //    await _client.CopyFileAsync(txtSource.Text, txtDestination.Text);
            //}
        }
    }
}
