using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xamarin.Forms;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for EditorLoginWindow.xaml
    /// </summary>
    public partial class EditorLoginWindow : Window
    {
        //Set the API Endpoint to Graph 'me' endpoint
        string graphAPIEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read" };

        public EditorLoginWindow()
        {
            InitializeComponent();
            AttemptSilentLogin();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            await LoginAndOpenEditor();

        }

        private async Task<bool> AttemptSilentLogin()
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;

            var accounts = await app.GetAccountsAsync();
            if (accounts.Count() > 0)
            {
                var firstAccount = accounts.FirstOrDefault();
                try
                {
                    authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilent.
                    // This indicates you need to call AcquireTokenInteractive to acquire a token
                    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
                }
            }
            return authResult != null;
        }

        private async Task LoginAndOpenEditor()
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;

            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenInteractive(scopes)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
            }
            catch (MsalClientException)
            {

            }
            catch (MsalException)
            {

            }

            if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
            {
                OpenEditor(true);
            }
        }

        //public async Task<string> GetHttpContentWithToken(string url, string token)
        //{

        //    var json = JsonConvert.SerializeObject(new { client_id = App.PublicClientApp.AppConfig.ClientId });
        //    var data = new StringContent(json, Encoding.UTF8, "application/json");

        //    var formdata = new List<KeyValuePair<string, string>>();
        //    formdata.Add(new KeyValuePair<string, string>("client_id", App.PublicClientApp.AppConfig.ClientId));
        //    formdata.Add(new KeyValuePair<string, string>("scope", "user.read"));
        //    var client = new HttpClient();
        //    var response = await client.PostAsync(url, new FormUrlEncodedContent(formdata));

        //    if(response != null)
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        return content;
        //    }

        //    return null;
        //    //var httpClient = new System.Net.Http.HttpClient();
        //    //System.Net.Http.HttpResponseMessage response;
        //    //try
        //    //{


        //    //    var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);

        //    //    //Add the token in Authorization header
        //    //    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //    //    response = await httpClient.SendAsync(request);
        //    //    var content = await response.Content.ReadAsStringAsync();
        //    //    return content;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return ex.ToString();
        //    //}
        //}

        public void OpenEditor(bool powerUser = false)
        {
            var editorWindow = new EditorWindow();
            if(powerUser)
                editorWindow.LoggedInAsPowerUser();
            editorWindow.Show();
            this.Close();
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            var editorWindow = new EditorWindow();
            editorWindow.Show();
            this.Close();
        }
    }
}
