using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo.RocketAPI
{
    public interface ISettings
    {
        AuthType AuthType { get; }
        double DefaultLatitude { get; }
        double DefaultLongitude { get; }
        string GoogleRefreshToken { get; set; }
        string PtcPassword { get; }
        string PtcUsername { get; }
    }
}