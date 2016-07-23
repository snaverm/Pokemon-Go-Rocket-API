using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo_UWP.Entities
{
    /// <summary>
    /// We need to wrap <see cref="NearbyPokemon"/> to add the sprite
    /// </summary>
    public class NearbyPokemonWrapper
    {

        #region Wrapped Properties

        public PokemonId PokemonId => _nearbyPokemon.PokemonId;

        public float DistanceInMeters => _nearbyPokemon.DistanceInMeters;

        public ulong EncounterId => _nearbyPokemon.EncounterId;

        #endregion

        private readonly NearbyPokemon _nearbyPokemon;

        public Uri PokemonSprite { get; }

        public NearbyPokemonWrapper(NearbyPokemon nearbyPokemon, Uri sprite)
        {
            _nearbyPokemon = nearbyPokemon;
            PokemonSprite = sprite;
        }
    }
}
