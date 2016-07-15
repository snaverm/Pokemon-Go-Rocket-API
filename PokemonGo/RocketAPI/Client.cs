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
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Extensions;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        private readonly HttpClient _httpClient;

        public Client()
        {
            //Setup HttpClient and create default headers
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 5.1.1; SM-G900F Build/LMY48G)");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        }

        public async Task<string> GetGoogleAccessToken(string deviceId, string clientSig, string email,
            string token)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };

            using (var tempHttpClient = new HttpClient(handler))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "GoogleAuth/1.4 (kltexx LMY48G); gzip");
                _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                tempHttpClient.DefaultRequestHeaders.Add("device", deviceId);
                tempHttpClient.DefaultRequestHeaders.Add("app", "com.nianticlabs.pokemongo");
                tempHttpClient.DefaultRequestHeaders.Add("device", deviceId);
                tempHttpClient.DefaultRequestHeaders.Add("device", deviceId);

                var response = await tempHttpClient.PostAsync(Resources.GOOGLE_GRANT_REFRESH_ACCESS_URL,
                    new FormUrlEncodedContent(
                        new[]
                        {
                            new KeyValuePair<string, string>("androidId", deviceId),
                            new KeyValuePair<string, string>("lang", "nl_NL"),
                            new KeyValuePair<string, string>("google_play_services_version", "9256238"),
                            new KeyValuePair<string, string>("sdk_version", "22"),
                            new KeyValuePair<string, string>("device_country", "nl"),
                            new KeyValuePair<string, string>("client_sig", "321187995bc7cdc2b5fc91b11a96e2baa8602c62"),
                            new KeyValuePair<string, string>("caller_sig", "321187995bc7cdc2b5fc91b11a96e2baa8602c62"),
                            new KeyValuePair<string, string>("Email", email),
                            new KeyValuePair<string, string>("service", "audience:server:client_id:848232511240-7so421jotr2609rmqakceuu1luuq0ptb.apps.googleusercontent.com"),
                            new KeyValuePair<string, string>("app", "com.nianticlabs.pokemongo"),
                            new KeyValuePair<string, string>("check_email", "1"),
                            new KeyValuePair<string, string>("token_request_options", ""),
                            new KeyValuePair<string, string>("callerPkg", "com.nianticlabs.pokemongo"),
                            new KeyValuePair<string, string>("Token", token)
                        }));

                var content = await response.Content.ReadAsStringAsync();
                return content.Split(new[] {"Auth=", "issueAdvice"}, StringSplitOptions.RemoveEmptyEntries)[0];
            }
        }

        public async Task<ProfileResponse> GetServer(Request profileRequest)
        {
            return await _httpClient.PostProto<Request, ProfileResponse>(Resources.RPC_URL, profileRequest);
        }
        public async Task<ProfileResponse> GetProfile(string apiUrl, Request profileRequest)
        {
            return await _httpClient.PostProto<Request, ProfileResponse>($"https://{apiUrl}/rpc", profileRequest);
        }
    }
}
