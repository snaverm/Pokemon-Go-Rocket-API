using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI
{
    public static class Settings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public const string DeviceId = "cool-device-id";
        public const string Email = "fake@gmail.com";
        public const string ClientSig = "fake";
        public const string LongDurationToken = "fakeid";
        public const double DefaultLatitude = 10;
        public const double DefaultLongitude = 10;

    }
}
