using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Live;
using OneDriveIntegration.Properties;

namespace OneDriveIntegration
{
        class FakeHttpContextBase : HttpContextBase
        {
            private HttpRequestBase _request = new FakeRequestBase();

            class FakeRequestBase : HttpRequestBase
            {
                private HttpCookieCollection _cookies = new HttpCookieCollection();
                private NameValueCollection _queryString = new NameValueCollection();
                private NameValueCollection _headers = new NameValueCollection();
                public override HttpCookieCollection Cookies
                {
                    get { return _cookies; }
                }

                public override NameValueCollection QueryString
                {
                    get { return _queryString; }
                }

                public override NameValueCollection Headers
                {
                    get { return _headers; }
                }
            }
            public override HttpRequestBase Request
            {
                get { return this._request; }
            }
        }

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
                    return Task.FromResult<RefreshTokenInfo>(new RefreshTokenInfo(Settings.Default.RefreshToken, Settings.Default.UserId));
                else
                {
                    return Task.FromResult<RefreshTokenInfo>(null);
                }
            }
            //private readonly FakeHttpContextBase _httpContext = new FakeHttpContextBase();

            private void wip()
            {
                //var authClient = new LiveAuthClient(clientId, clientSecret, endUrl, new RefreshHandler());
                //var result1 = await authClient.ExchangeAuthCodeAsync(_httpContext);
                //// var session = await authClient.InitializeWebSessionAsync(httpContext);
                //if (result1.Status != LiveConnectSessionStatus.Connected)
                //{
                    //var authClient = new LiveAuthClient(clientId, clientSecret, endUrl);
                    //var loginUrl = authClient.GetLoginUrl(new[] {"wl.skydrive", "wl.offline_access", "wl.skydrive_update"});
                    //var result = await authClient.InitializeSessionAsync(loginUrl);
                    //await authClient.ExchangeAuthCodeAsync();
                //}
                // var startUrl =  authClient.GetLoginUrl(new []{"wl.signin", "wl.skydrive", "wl.offline_access", "wl.skydrive_update"});
                //var connectClient = new LiveConnectClient(result.Session);
                //authClient.GetUserId(authForm.AuthorizeCode);
                // MessageBox.Show(authForm.AuthorizeCode);
                //var result = await authClient.ExchangeAuthCodeAsync(_httpContext);
            }
        }

}
