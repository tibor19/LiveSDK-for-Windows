using System;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace OneDriveIntegration
{
    public partial class LiveAuthForm : Form
    {
        private static readonly string _endUrl = "https://login.live.com/oauth20_desktop.srf";
        private static readonly string _startUrl = "https://login.live.com/oauth20_authorize.srf?client_id=0000000044007CD5&scope=wl.skydrive%20wl.skydrive_update%20wl.offline_access&response_type=code&redirect_uri=" + _endUrl;
        public string ErrorDescription { get; set; }
        public string ErrorCode { get; set; }
        public string AuthorizeCode { get; set; }

        public LiveAuthForm(string startUrl)
        {
            //_startUrl = startUrl;
            InitializeComponent();
        }

        private void LiveAuthForm_Load(object sender, EventArgs e)
        {
            webBrowser.Navigated += WebBrowser_Navigated;
            webBrowser.Navigate(_startUrl);
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser.Url.AbsoluteUri.StartsWith(_endUrl))
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
                if (String.IsNullOrEmpty(ErrorCode))
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
                this.Close();
            }
        }
    }
}
