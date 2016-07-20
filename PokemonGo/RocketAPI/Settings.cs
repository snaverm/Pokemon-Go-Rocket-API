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
        public const bool UsePTC = false;
        public const string PtcUsername = "User";
        public const string PtcPassword = "alligator2";
        public static string GoogleRefreshToken = string.Empty;
        public const double DefaultLatitude = 10;
        public const double DefaultLongitude = 10;

    }
}
