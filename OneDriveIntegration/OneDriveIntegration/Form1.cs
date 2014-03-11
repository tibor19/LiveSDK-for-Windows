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
        private OneDriveClient _oneClient;
        private const string ClientId = @"0000000040116E7B";
        private const string ClientSecret = @"wywiW4hpUPk1pAaeJRE8jLXPGzG9vXax";
        private const string RootFolderId = @"folder.fb4f175a349dea2a.FB4F175A349DEA2A!107";

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.RefreshToken))
            {
                try
                {
                    var authForm = new LiveAuthForm(ClientId);
                    if (authForm.ShowDialog(this) == DialogResult.OK)
                    {
                        _liveRestClient = new LiveRestClient(ClientId, ClientSecret);
                        await _liveRestClient.InitAsync(authForm.AuthorizeCode);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                _liveRestClient = new LiveRestClient(ClientId, ClientSecret, Settings.Default.RefreshToken);
                await _liveRestClient.RefreshTokensAsync();
            }
            if (_liveRestClient != null)
            {
                if (Settings.Default.RefreshToken != _liveRestClient.RefreshToken)
                {
                    Settings.Default.RefreshToken = _liveRestClient.RefreshToken;
                    Settings.Default.Save();
                }
                _oneClient = new OneDriveClient(_liveRestClient, RootFolderId);
            }
        }

        private async void btnGetId_Click(object sender, EventArgs e)
        {
            if (_oneClient != null)
            {
                MessageBox.Show(await _oneClient.GetFileOrFolderIdAsync(txtSource.Text));
            }
        }

        private async void btnCopy_Click(object sender, EventArgs e)
        {
            if (_oneClient != null)
            {
                await _oneClient.CopyFileAsync(txtSource.Text, txtDestination.Text);
            }
        }

        private async void btnMove_Click(object sender, EventArgs e)
        {
            if (_oneClient != null)
            {
                var newPath = await _oneClient.MoveFileAsync(txtSource.Text, txtDestination.Text);
                Clipboard.SetText(newPath);
                MessageBox.Show(newPath);
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (_oneClient != null)
            {
                var newPath = await _oneClient.UploadFileAsync(txtSource.Text, txtDestination.Text);
                MessageBox.Show(newPath);
            }
        }
    }
}
