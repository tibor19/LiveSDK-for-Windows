using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Windows.Forms;

namespace OneDriveIntegration
{
    public partial class LiveAuthForm : Form
    {
        private const string RequestFormatString = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=code&redirect_uri={2}";
        private const string EndUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string[] _scopes = { "wl.skydrive", "wl.skydrive_update", "wl.offline_access", "wl.signin" };

        private readonly string _startUrl;
        public string ErrorDescription { get; set; }
        public string ErrorCode { get; set; }
        public string AuthorizeCode { get; set; }

        public LiveAuthForm(string clientId)
        {
            _startUrl = string.Format(RequestFormatString, clientId, HttpUtility.HtmlEncode(string.Join(" ", _scopes)), HttpUtility.UrlEncode(EndUrl));
            InitializeComponent();
        }

        private void LiveAuthForm_Load(object sender, EventArgs e)
        {
            webBrowser.Navigated += WebBrowser_Navigated;
            webBrowser.Navigate(_startUrl);
            webBrowser.IsWebBrowserContextMenuEnabled = false;
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.AbsoluteUri.StartsWith(EndUrl))
            {
                string[] queryParams = webBrowser.Url.Query.TrimStart('?').Split('&');
                foreach (string param in queryParams)
                {
                    string[] kvp = param.Split('=');
                    switch (kvp[0])
                    {
                        case "code":
                            AuthorizeCode = kvp[1];
                            break;
                        case "error":
                            ErrorCode = kvp[1];
                            break;
                        case "error_description":
                            ErrorDescription = Uri.UnescapeDataString(kvp[1]);
                            break;
                    }
                }
                DialogResult = string.IsNullOrEmpty(ErrorCode) ? DialogResult.OK : DialogResult.Cancel;

                this.Close();
            }
        }
    }
}
