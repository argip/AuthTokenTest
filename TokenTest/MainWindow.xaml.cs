using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            txtAudience.Text = mysettings.Audience;
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
            string audience = txtAudience.Text;

            

            if (string.IsNullOrEmpty(tokenendpoint)) throw new ArgumentNullException("TokenEndpoint");
            if (string.IsNullOrEmpty(client_id)) throw new ArgumentNullException("ClientId");
            if (string.IsNullOrEmpty(client_secret)) throw new ArgumentNullException("ClientSecret");
            if (string.IsNullOrEmpty(audience)) throw new ArgumentNullException("Audience");

            //save data to store
            mysettings.Audience = audience;
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

            CancellationToken cancellationToken = default(CancellationToken);

            button.IsEnabled = false;

            var fields = new Dictionary<string, string>
            {
              { "grant_type", "client_credentials" },
              { "audience", audience },
              { "client_id", client_id },
              { "client_secret", client_secret }
            };

            var response = await _client.PostAsync(string.Empty, new FormUrlEncodedContent(fields), cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                TokenResponse _response = JsonConvert.DeserializeObject<TokenResponse>(content);
                TokenPayloadData _tokenpayloaddata =  DecodeJWT(_response.access_token);



                await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    txtJWT.Text = _response.access_token;

                    txtTokenType.Text = _response.token_type;

                    button.IsEnabled = true;

                    if (_tokenpayloaddata != null)
                    {
                            txtExpiresIn.Text = string.Format("{0} ({1} GMT)", _tokenpayloaddata.exp,  FromUnixTime(_tokenpayloaddata.exp).ToString());
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
                    txtError.Text = string.Format("Status code: {0}, reason: {1}", response.StatusCode, response.ReasonPhrase);
                    button.IsEnabled = true;
                }));
            }
        }
    }
}
