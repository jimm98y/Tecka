using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tecka
{
    public class OckoAuthRequest
    {
        [Newtonsoft.Json.JsonProperty("deviceName")]
        public string DeviceName { get; set; }
        
        [Newtonsoft.Json.JsonProperty("installationId")]
        public string InstallationId { get; set; }
    }

    public class OckoAuthResponse
    {
        [Newtonsoft.Json.JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }

    public class OckoPerson
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("lastChangeGdc")]
        public DateTime LastChangeGdc { get; set; }

        [Newtonsoft.Json.JsonProperty("givenName")]
        public string GivenName { get; set; }

        [Newtonsoft.Json.JsonProperty("familyName")]
        public string FamilyName { get; set; }

        [Newtonsoft.Json.JsonProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }
    }

    public class OckoCertificate
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("qrData")]
        public string qrData { get; set; }
    }

    public class OckoUzisClient
    {
        private const string BASE_UZIS_URL = "https://ocko.uzis.cz/api/v2/";

        private const string BASE_NIA_URL = "https://ocko.uzis.cz/Account/Prihlaseni";

        private const string LOGIN_REDIRECT_URL = "https://ocko.uzis.cz/Home/Index";

        private const string AUTH_ENDPOINT = "auth/login";

        private const string PERSON_ENDPOINT = "person";

        private string DEVICE_NAME = "UWPcka";
        private const string DEVICE_ID = "DeviceID";

        private readonly IHttpClientWrapper _client;

        public OckoUzisClient(IHttpClientWrapper client)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private async Task<string> GetJWTAsync(string deviceName, string installationId)
        {
            string httpResponse = await _client.HttpPostAsync($"{BASE_UZIS_URL}{AUTH_ENDPOINT}", Newtonsoft.Json.JsonConvert.SerializeObject(
                new OckoAuthRequest() {
                    DeviceName = deviceName,
                    InstallationId = installationId
                }));

            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<OckoAuthResponse>(httpResponse);
            return response.AccessToken;
        }

        public async Task Login()
        {
            string deviceId = LoadDeviceID();
            string accessToken;

            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = CreateNewDeviceID();
                SaveDeviceID(deviceId);

                accessToken = await GetJWTAsync(DEVICE_NAME, deviceId);
                Uri loginUrl = new Uri(GetLoginUrl(accessToken));
                await BrowserLogin(loginUrl);
            }

            accessToken = await GetJWTAsync(DEVICE_NAME, deviceId);
            _client.SetDefaultHeader("Authorization", $"Bearer {accessToken}");
        }

        private static async Task BrowserLogin(Uri loginUrl)
        {
            try
            {
                System.Uri endUrl = new Uri(LOGIN_REDIRECT_URL);

                Windows.Security.Authentication.Web.WebAuthenticationResult result = await Windows.Security.Authentication.Web.WebAuthenticationBroker.AuthenticateAsync(
                                                        Windows.Security.Authentication.Web.WebAuthenticationOptions.None,
                                                        loginUrl,
                                                        endUrl);
                if (result.ResponseStatus == Windows.Security.Authentication.Web.WebAuthenticationStatus.Success)
                {
                    // continue
                }
                else if (result.ResponseStatus == Windows.Security.Authentication.Web.WebAuthenticationStatus.ErrorHttp)
                {
                    throw new Exception("HTTP Error returned by AuthenticateAsync() : " + result.ResponseErrorDetail.ToString());
                }
                else
                {
                    throw new Exception("Error returned by AuthenticateAsync() : " + result.ResponseStatus.ToString());
                }
            }
            catch (Exception e)
            {
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        private string GetLoginUrl(string accessToken, string authType = "AlternativeOnly")
        {
            return $"{BASE_NIA_URL}?AccessToken={accessToken}&Type={authType}";
        }

        public async Task<OckoPerson[]> GetPersonsAsync()
        {
            string httpResponse = await _client.HttpGetAsync($"{BASE_UZIS_URL}{PERSON_ENDPOINT}");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<OckoPerson[]>(httpResponse);
        }

        public async Task<OckoCertificate[]> GetCertificatesAsync(string personId)
        {
            string httpResponse = await _client.HttpGetAsync($"{BASE_UZIS_URL}{PERSON_ENDPOINT}/{personId}/dgc");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<OckoCertificate[]>(httpResponse);
        }

        private string CreateNewDeviceID()
        {
            return Guid.NewGuid().ToString("D").ToUpperInvariant();
        }

        protected virtual string SaveDeviceID(string deviceID)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[DEVICE_ID] = deviceID;
            return deviceID;
        }

        protected virtual string LoadDeviceID()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string deviceID = localSettings.Values[DEVICE_ID] as string;
            if (!string.IsNullOrWhiteSpace(deviceID))
                return deviceID;
            else
                return null;
        }
    }
}
