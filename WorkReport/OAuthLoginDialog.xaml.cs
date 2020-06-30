using eWayCRM.API.Net;
using JWT;
using JWT.Builder;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimeClock.Core;
using TimeClock.Interfaces;

namespace TimeClock
{
    /// <summary>
    /// Interaction logic for OAuthLoginDialog.xaml
    /// </summary>
    public partial class OAuthLoginDialog : Window, ILoginDialog
    {
        public OAuthLoginDialog()
        {
            InitializeComponent();
        }

        public string UserName { get; set; }
        
        public string AccessToken { get; set; }

        public string Server { get; set; }

        public bool RememberPassword => false;

        public string Password { get => null; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string clientId = Settings.APPLICATION_NAME_SHORT.ToLower();

            this.webBrowser.Navigate($"https://login.eway-crm.com/?client_id={clientId}&scope=api&redirect_uri=http://localhost&response_type=token&login_hint={this.UserName}&ui_locales=en");
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (!e.Uri.AbsoluteUri.StartsWith("http://localhost/#"))
                return;

            e.Cancel = true;
            
            var parameters = new ParameterCollection(e.Uri.Fragment.Substring(1));
            this.AccessToken = parameters["access_token"];

            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            var decoder = new JwtDecoder(serializer, urlEncoder);

            var payload = decoder.DecodeToObject(this.AccessToken);

            this.UserName = payload["username"].ToString();
            this.Server = payload["ws"].ToString();

            this.DialogResult = true;
            this.Close();
        }

        public void HideRememberPasswordControl() { }
    }
}
