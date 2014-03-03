using System;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace OneDriveIntegration
{
    public partial class LiveAuthForm : Form
    {
        private readonly string _startUrl;
        private static readonly string _endUrl = "https://login.live.com/oauth20_desktop.srf";
        public string ErrorDescription { get; set; }
        public string ErrorCode { get; set; }
        public string AuthorizeCode { get; set; }

        public LiveAuthForm(string startUrl)
        {
            _startUrl = startUrl;
            InitializeComponent();
        }

        private void LiveAuthForm_Load(object sender, EventArgs e)
        {
            webBrowser.Navigated += WebBrowser_Navigated;
            webBrowser.Navigate(_startUrl);
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser.Url.AbsoluteUri.Equals(_startUrl))
            {
                return;
            }

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
