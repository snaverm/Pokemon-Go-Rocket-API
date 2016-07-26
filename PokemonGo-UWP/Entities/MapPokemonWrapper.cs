using Windows.Devices.Geolocation;
using AllEnum;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo_UWP.Entities
{
    public class MapPokemonWrapper
    {
        private readonly MapPokemon _mapPokemon;

        public MapPokemonWrapper(MapPokemon mapPokemon)
        {
            _mapPokemon = mapPokemon;
            Geoposition =
                new Geopoint(new BasicGeoposition {Latitude = _mapPokemon.Latitude, Longitude = _mapPokemon.Longitude});
        }

        #region Wrapped Properties

        public PokemonId PokemonId => _mapPokemon.PokemonId;

        public ulong EncounterId => _mapPokemon.EncounterId;

        public long ExpirationTimestampMs => _mapPokemon.ExpirationTimestampMs;

        public string SpawnpointId => _mapPokemon.SpawnpointId;

        public Geopoint Geoposition { get; }

        public double Latitude => Geoposition.Position.Latitude;

        public double Longitude => Geoposition.Position.Longitude;

        #endregion
    }
}