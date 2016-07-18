using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Extensions;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        private AuthType _authType = AuthType.Google;
        private string _accessToken;
        private string _apiUrl;
        private Request.Types.UnknownAuth _unknownAuth;

        public Client()
        {
            //Setup HttpClient and create default headers
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(new RetryHandler(handler));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");//"Dalvik/2.1.0 (Linux; U; Android 5.1.1; SM-G900F Build/LMY48G)");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        }

        public async Task LoginGoogle(string deviceId, string email, string refreshToken)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };

            using (var tempHttpClient = new HttpClient(handler))
            {
                tempHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "GoogleAuth/1.4 (kltexx LMY48G); gzip");
                tempHttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                tempHttpClient.DefaultRequestHeaders.Add("device", deviceId);
                tempHttpClient.DefaultRequestHeaders.Add("app", "com.nianticlabs.pokemongo");

                var response = await tempHttpClient.PostAsync(Resources.GoogleGrantRefreshAccessUrl,
                    new FormUrlEncodedContent(
                        new[]
                        {
                            new KeyValuePair<string, string>("androidId", deviceId),
                            new KeyValuePair<string, string>("lang", "nl_NL"),
                            new KeyValuePair<string, string>("google_play_services_version", "9256238"),
                            new KeyValuePair<string, string>("sdk_version", "22"),
                            new KeyValuePair<string, string>("device_country", "nl"),
                            new KeyValuePair<string, string>("client_sig", Settings.ClientSig),
                            new KeyValuePair<string, string>("caller_sig", Settings.ClientSig),
                            new KeyValuePair<string, string>("Email", email),
                            new KeyValuePair<string, string>("service", "audience:server:client_id:848232511240-7so421jotr2609rmqakceuu1luuq0ptb.apps.googleusercontent.com"),
                            new KeyValuePair<string, string>("app", "com.nianticlabs.pokemongo"),
                            new KeyValuePair<string, string>("check_email", "1"),
                            new KeyValuePair<string, string>("token_request_options", ""),
                            new KeyValuePair<string, string>("callerPkg", "com.nianticlabs.pokemongo"),
                            new KeyValuePair<string, string>("Token", refreshToken)
                        }));

                var content = await response.Content.ReadAsStringAsync();
                _accessToken = content.Split(new[] {"Auth=", "issueAdvice"}, StringSplitOptions.RemoveEmptyEntries)[0];
                _authType = AuthType.Google;
            }
        }

        public async Task LoginPtc(string username, string password)
        {
            //Get session cookie
            var sessionResp = await _httpClient.GetAsync(Resources.PtcLoginUrl);
            var data = await sessionResp.Content.ReadAsStringAsync();
            var lt = JsonHelper.GetValue(data, "lt");
            var executionId = JsonHelper.GetValue(data, "execution");

            //Login
            var loginResp = await _httpClient.PostAsync(Resources.PtcLoginUrl,
                new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("lt", lt),
                        new KeyValuePair<string, string>("execution", executionId),
                        new KeyValuePair<string, string>("_eventId", "submit"),
                        new KeyValuePair<string, string>("username", username),
                        new KeyValuePair<string, string>("password", password),
                    }));

            var ticketId = HttpUtility.ParseQueryString(loginResp.Headers.Location.Query)["ticket"];

            //Get tokenvar 
            var tokenResp = await _httpClient.PostAsync(Resources.PtcLoginOauth,
            new FormUrlEncodedContent(
                new[]
                {
                        new KeyValuePair<string, string>("client_id", "mobile-app_pokemon-go"),
                        new KeyValuePair<string, string>("redirect_uri", "https://www.nianticlabs.com/pokemongo/error"),
                        new KeyValuePair<string, string>("client_secret", "w8ScCUXJQc6kXKw8FiOhd8Fixzht18Dq3PEVkUCP5ZPxtgyWsbTvWHFLm2wNY0JR"),
                        new KeyValuePair<string, string>("grant_type", "grant_type"),
                        new KeyValuePair<string, string>("code", ticketId),
                }));

            var tokenData = await tokenResp.Content.ReadAsStringAsync();
            _accessToken = HttpUtility.ParseQueryString(tokenData)["access_token"];
            _authType = AuthType.Ptc;
        }

        public async Task<ProfileResponse> GetServer()
        {
            var serverRequest = RequestBuilder.GetInitialRequest(_accessToken, _authType, Settings.DefaultLatitude, Settings.DefaultLongitude, 30, RequestType.Profile, RequestType.Unknown126, RequestType.Time, RequestType.Unknown129, RequestType.Settings);
            var serverResponse = await _httpClient.PostProto<Request, ProfileResponse>(Resources.RpcUrl, serverRequest);
            _apiUrl = serverResponse.ApiUrl;
            return serverResponse;
        }

        public async Task<ProfileResponse> GetProfile()
        {
            var profileRequest = RequestBuilder.GetInitialRequest(_accessToken, _authType, Settings.DefaultLatitude, Settings.DefaultLongitude, 30, new Request.Types.Requests() { Type = (int)RequestType.Profile });
            var profileResponse = await _httpClient.PostProto<Request, ProfileResponse>($"https://{_apiUrl}/rpc", profileRequest);
            _unknownAuth = new Request.Types.UnknownAuth()
            {
                Unknown71 = profileResponse.Auth.Unknown71,
                Timestamp = profileResponse.Auth.Timestamp,
                Unknown73 = profileResponse.Auth.Unknown73,
            };
            return profileResponse;
        }

        public async Task<SettingsResponse> GetSettings()
        {
            var settingsRequest = RequestBuilder.GetRequest(_unknownAuth, Settings.DefaultLatitude, Settings.DefaultLongitude, 30, RequestType.Settings);
            return await _httpClient.PostProto<Request, SettingsResponse>($"https://{_apiUrl}/rpc", settingsRequest);
        }
        public async Task<EncounterResponse> GetEncounters()
        {
            var customRequest = new EncounterRequest.Types.RequestsMessage()
            {
                CellIds =
                    ByteString.CopyFrom(
                        ProtoHelper.EncodeUlongList(S2Helper.GetNearbyCellIds(Settings.DefaultLongitude,
                            Settings.DefaultLatitude))),
                Latitude = Utils.FloatAsUlong(Settings.DefaultLatitude),
                Longitude = Utils.FloatAsUlong(Settings.DefaultLongitude),
                Unknown14 = ByteString.CopyFromUtf8("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
            };

            var encounterRequest = RequestBuilder.GetRequest(_unknownAuth, Settings.DefaultLatitude, Settings.DefaultLongitude, 30, 
                new Request.Types.Requests() { Type = (int)RequestType.Encounters, Message = customRequest.ToByteString() },
                new Request.Types.Requests() { Type = (int)RequestType.Unknown126 },
                new Request.Types.Requests() { Type = (int)RequestType.Time, Message = new EncounterRequest.Types.Time() { Time_ = DateTime.UtcNow.ToUnixTime() }.ToByteString() },
                new Request.Types.Requests() { Type = (int)RequestType.Unknown129 },
                new Request.Types.Requests() { Type = (int)RequestType.Settings, Message = new EncounterRequest.Types.SettingsGuid() { Guid = ByteString.CopyFromUtf8("4a2e9bc330dae60e7b74fc85b98868ab4700802e")}.ToByteString() });

            return await _httpClient.PostProto<Request, EncounterResponse>($"https://{_apiUrl}/rpc", encounterRequest);
        }

    }
}
