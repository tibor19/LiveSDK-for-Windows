using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using OneDriveIntegration.Properties;

namespace OneDriveIntegration
{
    public partial class Form1 : Form
    {
        private LiveRestClient _liveRestClient;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            const string clientId = @"0000000044007CD5";
            const string clientSecret = @"CxQOxCRbxzDOnRiSinyTcexWayWE9Q7m";

            if (string.IsNullOrEmpty(Settings.Default.RefreshToken))
            {
                try
                {
                    var authForm = new LiveAuthForm(clientId);
                    if (authForm.ShowDialog(this) == DialogResult.OK)
                    {
                        var client = new LiveRestClient(clientId, clientSecret);
                        await client.InitAsync(authForm.AuthorizeCode);
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
                _liveRestClient = new LiveRestClient(clientId, clientSecret, Settings.Default.RefreshToken);
                await _liveRestClient.RefreshTokensAsync();
                if (Settings.Default.RefreshToken != _liveRestClient.RefreshToken)
                {
                    Settings.Default.RefreshToken = _liveRestClient.RefreshToken;
                    Settings.Default.Save();
                }
            }
        }

        private async void btnGetId_Click(object sender, EventArgs e)
        {
            var client = new OneDriveClient(_liveRestClient, null);
            MessageBox.Show(await client.GetFileOrFolderIdAsync(txtSource.Text));
        }

        private async void btnCopy_Click(object sender, EventArgs e)
        {
            var client = new OneDriveClient(_liveRestClient, null);
            await client.CopyFileAsync(txtSource.Text, txtDestination.Text);
        }

        private async void btnMove_Click(object sender, EventArgs e)
        {
            var client = new OneDriveClient(_liveRestClient, null);
            var newPath = await client.MoveFileAsync(txtSource.Text, txtDestination.Text);
            Clipboard.SetText(newPath);
            MessageBox.Show(newPath);
        }
    }
}
