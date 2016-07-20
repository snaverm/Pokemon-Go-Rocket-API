using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Extensions;

namespace PokemonGo.RocketAPI
{
    public static class Settings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public const AuthType AuthType = Enums.AuthType.Google;
        public const string PtcUsername = "username";
        public const string PtcPassword = "pw";
        public static string GoogleRefreshToken = string.Empty;
        public const double DefaultLatitude = 10;
        public const double DefaultLongitude = 10;
    }
}
