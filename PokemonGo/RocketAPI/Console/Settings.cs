using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public AuthType AuthType { get;  } = Enums.AuthType.Google;
        public  string PtcUsername { get; } = "User";
        public  string PtcPassword { get; } = "alligator2";
        public  string GoogleRefreshToken { get; set; } = string.Empty;
        public  double DefaultLatitude { get; } = 10;
        public  double DefaultLongitude { get; } = 10;
    }
}
