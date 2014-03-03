using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneDriveIntegration
{
    public partial class Form1 : Form
    {
        private OneDriveClient _client;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            const string clientId = @"0000000044007CD5";
            var authClient = new LiveAuthClient(clientId);
            var startUrl =  authClient.GetLoginUrl(new []{"wl.signin", "wl.skydrive", "wl.offline_access", "wl.skydrive_update"});

            var authForm = new LiveAuthForm(startUrl);
            if (authForm.ShowDialog(this) == DialogResult.OK)
            {
                var session = await authClient.ExchangeAuthCodeAsync(authForm.AuthorizeCode);
                var connectClient = new LiveConnectClient(session);
                _client = new OneDriveClient(connectClient);
                //var restClient = new HttpClient();
                //var response = await restClient.GetAsync("https://apis.live.net/v5.0/folder.d0396257098c3b52/files?access_token=" +
                //                    session.AccessToken);
            }
        }

        private async void btnGetId_Click(object sender, EventArgs e)
        {
            if (_client != null)
            {
                MessageBox.Show(await _client.GetFileIdAsync(txtSource.Text));
            }
        }

        private async void btnCopy_Click(object sender, EventArgs e)
        {
            if (_client != null)
            {
                await _client.CopyFileAsync(txtSource.Text, txtDestination.Text);
            }
        }
    }
}
