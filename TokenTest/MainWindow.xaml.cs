using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Windows;

namespace TokenTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient _client;
        private MyCustomSettings mysettings = new MyCustomSettings(); 

        public MainWindow()
        {
            InitializeComponent();
            ReadCustomSettings();
        }

        private void ReadCustomSettings()
        {
            txtTokenEndpoint.Text = mysettings.TokenEndpoint;
            txtClientId.Text = mysettings.ClientId;
            txtClientSecret.Text = mysettings.ClientSecret;
        }

        //Decode Token, check payload data
        private TokenPayloadData DecodeJWT(string jwt)
        {
            try
            {
                string strdata = JWT.JsonWebToken.Decode(jwt, string.Empty, false);
                return JsonConvert.DeserializeObject<TokenPayloadData>(strdata);
            }
            catch (JWT.SignatureVerificationException)
            {
                return null;
            }
        }

        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            string tokenendpoint = txtTokenEndpoint.Text;
            string client_id = txtClientId.Text;
            string client_secret = txtClientSecret.Text;

            

            if (string.IsNullOrEmpty(tokenendpoint)) throw new ArgumentNullException("TokenEndpoint");
            if (string.IsNullOrEmpty(client_id)) throw new ArgumentNullException("ClientId");
            if (string.IsNullOrEmpty(client_secret)) throw new ArgumentNullException("ClientSecret");

            //save data to store
            mysettings.ClientId = client_id;
            mysettings.ClientSecret = client_secret;
            mysettings.TokenEndpoint = tokenendpoint;
            mysettings.Save();


            _client = new HttpClient()
            {
                BaseAddress = new Uri(tokenendpoint)
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var cancellationToken = default(CancellationToken);

            button.IsEnabled = false;

            var fields = new Dictionary<string, string>
            {
              { "grant_type", "client_credentials" },
              { "client_id", client_id },
              { "client_secret", client_secret }
            };

            var response = await _client.PostAsync(string.Empty, new FormUrlEncodedContent(fields), cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var _response = JsonConvert.DeserializeObject<TokenResponse>(content);
                var _tokenpayloaddata =  DecodeJWT(_response.access_token);



                await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    txtJWT.Text = _response.access_token;

                    txtTokenType.Text = _response.token_type;

                    button.IsEnabled = true;

                    if (_tokenpayloaddata != null)
                    {
                        txtExpiresIn.Text =
                            $"{_tokenpayloaddata.exp} ({FromUnixTime(_tokenpayloaddata.exp).ToString()} GMT)";
                        txtNotBefore.Text =
                            $"{_tokenpayloaddata.exp} ({FromUnixTime(_tokenpayloaddata.nbf).ToString()} GMT)";
                    }
                    else
                    {
                        txtError.Text += "Unable decode JWT";
                    }


                }));

                
            }
            else
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    txtError.Text = $"Status code: {response.StatusCode}, reason: {response.ReasonPhrase}";
                    button.IsEnabled = true;
                }));
            }
        }
    }
}
