using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Login;
using static PokemonGo.RocketAPI.GeneratedCode.Response.Types;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        private readonly ISettings _settings;
        private readonly HttpClient _httpClient;
        private AuthType _authType = AuthType.Google;
        private string _accessToken;
        private string _apiUrl;
        private Request.Types.UnknownAuth _unknownAuth;

        private double _currentLat;
        private double _currentLng;

        public Client(ISettings settings)
        {
            _settings = settings;
            SetCoordinates(_settings.DefaultLatitude, _settings.DefaultLongitude);

            //Setup HttpClient and create default headers
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(new RetryHandler(handler));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
                //"Dalvik/2.1.0 (Linux; U; Android 5.1.1; SM-G900F Build/LMY48G)");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",
                "application/x-www-form-urlencoded");
        }

        private void SetCoordinates(double lat, double lng)
        {
            _currentLat = lat;
            _currentLng = lng;
        }

        public async Task DoGoogleLogin()
        {
            _authType = AuthType.Google;
            if (_settings.GoogleRefreshToken != string.Empty)
            {
                var tokenResponse = await GoogleLogin.GetAccessToken(_settings.GoogleRefreshToken);
                _accessToken = tokenResponse.id_token;
            }
            
            if (_accessToken == null)
            {
                var tokenResponse = await GoogleLogin.GetAccessToken();
                _accessToken = tokenResponse.id_token;
                _settings.GoogleRefreshToken = tokenResponse.access_token;
            }
        }

        public async Task DoPtcLogin(string username, string password)
        {
            _accessToken = await PtcLogin.GetAccessToken(username, password);
            _authType = AuthType.Ptc;
        }

        public async Task<PlayerUpdateResponse> UpdatePlayerLocation(double lat, double lng)
        {
            this.SetCoordinates(lat, lng);
            var customRequest = new Request.Types.PlayerUpdateProto()
            {
                Lat = Utils.FloatAsUlong(_currentLat),
                Lng = Utils.FloatAsUlong(_currentLng)
            };

            var updateRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 10,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.PLAYER_UPDATE,
                    Message = customRequest.ToByteString()
                });
            var updateResponse =
                await
                    _httpClient.PostProtoPayload<Request, PlayerUpdateResponse>($"https://{_apiUrl}/rpc", updateRequest);
            return updateResponse;
        }

        public async Task SetServer()
        {
            var serverRequest = RequestBuilder.GetInitialRequest(_accessToken, _authType, _currentLat, _currentLng, 10,
                RequestType.GET_PLAYER, RequestType.GET_HATCHED_OBJECTS, RequestType.GET_INVENTORY,
                RequestType.CHECK_AWARDED_BADGES, RequestType.DOWNLOAD_SETTINGS);
            var serverResponse = await _httpClient.PostProto<Request>(Resources.RpcUrl, serverRequest);
            _unknownAuth = new Request.Types.UnknownAuth()
            {
                Unknown71 = serverResponse.Auth.Unknown71,
                Timestamp = serverResponse.Auth.Timestamp,
                Unknown73 = serverResponse.Auth.Unknown73,
            };

            _apiUrl = serverResponse.ApiUrl;
        }

        public async Task<GetPlayerResponse> GetProfile()
        {
            var profileRequest = RequestBuilder.GetInitialRequest(_accessToken, _authType, _currentLat, _currentLng, 10,
                new Request.Types.Requests() { Type = (int)RequestType.GET_PLAYER });
            return await _httpClient.PostProtoPayload<Request, GetPlayerResponse>($"https://{_apiUrl}/rpc", profileRequest);
        }

        public async Task<DownloadSettingsResponse> GetSettings()
        {
            var settingsRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 10,
                RequestType.DOWNLOAD_SETTINGS);
            return await _httpClient.PostProtoPayload<Request, DownloadSettingsResponse>($"https://{_apiUrl}/rpc", settingsRequest);
        }

        public async Task<DownloadItemTemplatesResponse> GetItemTemplates()
        {
            var settingsRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 10,
                RequestType.DOWNLOAD_ITEM_TEMPLATES);
            return
                await
                    _httpClient.PostProtoPayload<Request, DownloadItemTemplatesResponse>($"https://{_apiUrl}/rpc",
                        settingsRequest);
        }



        public async Task<GetMapObjectsResponse> GetMapObjects()
        {
            var customRequest = new Request.Types.MapObjectsRequest()
            {
                CellIds =
                    ByteString.CopyFrom(
                        ProtoHelper.EncodeUlongList(S2Helper.GetNearbyCellIds(_currentLng,
                            _currentLat))),
                Latitude = Utils.FloatAsUlong(_currentLat),
                Longitude = Utils.FloatAsUlong(_currentLng),
                Unknown14 = ByteString.CopyFromUtf8("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
            };

            var mapRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 10,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.GET_MAP_OBJECTS,
                    Message = customRequest.ToByteString()
                },
                new Request.Types.Requests() { Type = (int)RequestType.GET_HATCHED_OBJECTS },
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.GET_INVENTORY,
                    Message = new Request.Types.Time() { Time_ = DateTime.UtcNow.ToUnixTime() }.ToByteString()
                },
                new Request.Types.Requests() { Type = (int)RequestType.CHECK_AWARDED_BADGES },
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.DOWNLOAD_SETTINGS,
                    Message =
                        new Request.Types.SettingsGuid()
                        {
                            Guid = ByteString.CopyFromUtf8("4a2e9bc330dae60e7b74fc85b98868ab4700802e")
                        }.ToByteString()
                });

            return await _httpClient.PostProtoPayload<Request, GetMapObjectsResponse>($"https://{_apiUrl}/rpc", mapRequest);
        }

        public async Task<FortDetailsResponse> GetFort(string fortId, double fortLat, double fortLng)
        {
            var customRequest = new Request.Types.FortDetailsRequest()
            {
                Id = ByteString.CopyFromUtf8(fortId),
                Latitude = Utils.FloatAsUlong(fortLat),
                Longitude = Utils.FloatAsUlong(fortLng),
            };

            var fortDetailRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 10,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.FORT_DETAILS,
                    Message = customRequest.ToByteString()
                });
            return await _httpClient.PostProtoPayload<Request, FortDetailsResponse>($"https://{_apiUrl}/rpc", fortDetailRequest);
        }

        public async Task<FortSearchResponse> SearchFort(string fortId, double fortLat, double fortLng)
        {
            var customRequest = new Request.Types.FortSearchRequest()
            {
                Id = ByteString.CopyFromUtf8(fortId),
                FortLatDegrees = Utils.FloatAsUlong(fortLat),
                FortLngDegrees = Utils.FloatAsUlong(fortLng),
                PlayerLatDegrees = Utils.FloatAsUlong(_currentLat),
                PlayerLngDegrees = Utils.FloatAsUlong(_currentLng)
            };

            var fortDetailRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.FORT_SEARCH,
                    Message = customRequest.ToByteString()
                });
            return await _httpClient.PostProtoPayload<Request, FortSearchResponse>($"https://{_apiUrl}/rpc", fortDetailRequest);
        }

        public async Task<EncounterResponse> EncounterPokemon(ulong encounterId, string spawnPointGuid)
        {
            var customRequest = new Request.Types.EncounterRequest()
            {
                EncounterId = encounterId,
                SpawnpointId = spawnPointGuid,
                PlayerLatDegrees = Utils.FloatAsUlong(_currentLat),
                PlayerLngDegrees = Utils.FloatAsUlong(_currentLng)
            };

            var encounterResponse = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.ENCOUNTER,
                    Message = customRequest.ToByteString()
                });
            return await _httpClient.PostProtoPayload<Request, EncounterResponse>($"https://{_apiUrl}/rpc", encounterResponse);
        }

        public async Task<UseItemCaptureRequest> UseCaptureItem(ulong encounterId, AllEnum.ItemId itemId, string spawnPointGuid)
        {
            var customRequest = new UseItemCaptureRequest
            {
                EncounterId = encounterId,
                ItemId = itemId,
                SpawnPointGuid = spawnPointGuid
            };

            var useItemRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.USE_ITEM_CAPTURE,
                    Message = customRequest.ToByteString()
                });
            return await _httpClient.PostProtoPayload<Request, UseItemCaptureRequest>($"https://{_apiUrl}/rpc", useItemRequest);
        }

        public async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnPointGuid, double pokemonLat,
            double pokemonLng, MiscEnums.Item pokeball)
        {

            var customRequest = new Request.Types.CatchPokemonRequest()
            {
                EncounterId = encounterId,
                Pokeball = (int)pokeball,
                SpawnPointGuid = spawnPointGuid,
                HitPokemon = 1,
                NormalizedReticleSize = Utils.FloatAsUlong(1.950),
                SpinModifier = Utils.FloatAsUlong(1),
                NormalizedHitPosition = Utils.FloatAsUlong(1)
            };

            var catchPokemonRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.CATCH_POKEMON,
                    Message = customRequest.ToByteString()
                });
            return
                await
                    _httpClient.PostProtoPayload<Request, CatchPokemonResponse>($"https://{_apiUrl}/rpc", catchPokemonRequest);
        }

        public async Task<TransferPokemonOut> TransferPokemon(ulong pokemonId)
        {
            var customRequest = new TransferPokemon
            {
                PokemonId = pokemonId
            };

            var releasePokemonRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.RELEASE_POKEMON,
                    Message = customRequest.ToByteString()
                });
            return await _httpClient.PostProtoPayload<Request, TransferPokemonOut>($"https://{_apiUrl}/rpc", releasePokemonRequest);
        }

        public async Task<EvolvePokemonOut> EvolvePokemon(ulong pokemonId)
        {
            var customRequest = new EvolvePokemon
            {
                PokemonId = pokemonId
            };

            var releasePokemonRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.EVOLVE_POKEMON,
                    Message = customRequest.ToByteString()
                });
            return
                await
                    _httpClient.PostProtoPayload<Request, EvolvePokemonOut>($"https://{_apiUrl}/rpc", releasePokemonRequest);
        }

        public async Task<GetInventoryResponse> GetInventory()
        {
            var inventoryRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30, RequestType.GET_INVENTORY);
            return await _httpClient.PostProtoPayload<Request, GetInventoryResponse>($"https://{_apiUrl}/rpc", inventoryRequest);
        }

        public async Task<RecycleInventoryItemResponse> RecycleItem(AllEnum.ItemId itemId, int amount)
        {
            var customRequest = new RecycleInventoryItem
            {
                ItemId = (AllEnum.ItemId)Enum.Parse(typeof(AllEnum.ItemId), itemId.ToString()),
                Count = amount
            };

            var releasePokemonRequest = RequestBuilder.GetRequest(_unknownAuth, _currentLat, _currentLng, 30,
                new Request.Types.Requests()
                {
                    Type = (int)RequestType.RECYCLE_INVENTORY_ITEM,
                    Message = customRequest.ToByteString()
                });
            return await _httpClient.PostProtoPayload<Request, RecycleInventoryItemResponse>($"https://{_apiUrl}/rpc", releasePokemonRequest);
        }
    }
}
