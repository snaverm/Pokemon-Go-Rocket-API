using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        public static Request GetRequest(RequestType requestType, string authToken, double lat, double lng, double altitude)
        {
            return new Request()
            {
                Altitude = altitude,
                Auth = new Request.Types.AuthInfo()
                {
                    Provider = "google",
                    Token = new Request.Types.AuthInfo.Types.JWT()
                    {
                        Contents = authToken,
                        Unknown13 = 59
                    }
                },
                Latitude = lat,
                Longitude = lng,
                RpcId = 6032429073588813826,// RandomHelper.GetLongRandom(1000000000000000000, long.MaxValue),
                Unknown1 = 2,
                Unknown12 = 138, //Required otherwise we receive incompatible protocol
                Requests =
                {
                    new Request.Types.Requests() { Type = (int)requestType },
                }
            };
        }
    }
}
