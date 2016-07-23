using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using AllEnum;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo_UWP.Entities
{
    public class MapPokemonWrapper
    {
        #region Wrapped Properties

        public PokemonId PokemonId => _mapPokemon.PokemonId;        

        public ulong EncounterId => _mapPokemon.EncounterId;

        public long ExpirationTimestampMs => _mapPokemon.ExpirationTimestampMs;

        public string SpawnPointId => _mapPokemon.SpawnpointId;

        public Geopoint Geoposition { get; private set; }

        public double Latitude => Geoposition.Position.Latitude;

        public double Longitude => Geoposition.Position.Longitude;

        #endregion

        private readonly MapPokemon _mapPokemon;

        public MapPokemonWrapper(MapPokemon mapPokemon)
        {
            _mapPokemon = mapPokemon;            
            Geoposition = new Geopoint(new BasicGeoposition {Latitude = _mapPokemon.Latitude, Longitude = _mapPokemon.Longitude});
        }
    }
}
