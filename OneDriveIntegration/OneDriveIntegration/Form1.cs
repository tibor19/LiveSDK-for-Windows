using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
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

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            const string clientId = @"0000000044007CD5";
            const string clientSecret = "CxQOxCRbxzDOnRiSinyTcexWayWE9Q7m";
            const string endUrl = "https://login.live.com/oauth20_desktop.srf";
            //const string endUrl = "https://login.live.com/oauth20_token.srf";
            if (string.IsNullOrEmpty(Settings.Default.RefreshToken))
            {

                try
                {
                    var authForm = new LiveAuthForm(null);
                    if (authForm.ShowDialog(this) == DialogResult.OK)
                    {
                        var client = new LiveRestClient();
                        await client.InitAsync(authForm.AuthorizeCode);
                        AccessToken = client.AccessToken;
                        Settings.Default.RefreshToken = client.RefreshToken;
                        Settings.Default.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                var client = new LiveRestClient(Settings.Default.RefreshToken);
                await client.RefreshTokensAsync();
                if (Settings.Default.RefreshToken != client.RefreshToken)
                {
                    Settings.Default.RefreshToken = client.RefreshToken;
                    Settings.Default.Save();
                }
                AccessToken = client.AccessToken;
            }

        }

        public string AccessToken { get; set; }

        private async void btnGetId_Click(object sender, EventArgs e)
        {
            var client = new OneDriveClient(AccessToken, null);
            MessageBox.Show(await client.GetFileOrFolderIdAsync(txtSource.Text));
        }

        private async void btnCopy_Click(object sender, EventArgs e)
        {
            var client = new OneDriveClient(AccessToken, null);
            await client.CopyFileAsync(txtSource.Text, txtDestination.Text); //MessageBox.Show();
        }

        private async void btnMove_Click(object sender, EventArgs e)
        {
            var client = new OneDriveClient(AccessToken, null);
            var newPath = await client.MoveFileAsync(txtSource.Text, txtDestination.Text);
            Clipboard.SetText(newPath);
            MessageBox.Show(newPath);
        }
    }
}
