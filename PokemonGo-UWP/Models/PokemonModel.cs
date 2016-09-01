using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Google.Protobuf.WellKnownTypes;
using PokemonGo_UWP.Utils;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using POGOProtos.Settings.Master.Pokemon;

namespace PokemonGo_UWP.Models
{
    public class PokemonModel
    {
        public PokemonId Id { get; set; } = PokemonId.Missingno;

        public string Name => (string)new PokemonIdToPokemonNameConverter().Convert(this.Id, typeof(string), null, string.Empty);
        public BitmapImage Sprite => (BitmapImage)new PokemonIdToPokemonSpriteConverter().Convert(this.Id, typeof(BitmapImage), null, string.Empty);
        public Uri BackgroundImage => (Uri)new PokemonTypeToBackgroundImageConverter().Convert(this.Id, typeof(Uri), null, string.Empty);

        public string Description => (string)new PokemonIdToPokedexDescription().Convert(this.Id, typeof(string), null, string.Empty);
        public List<PokemonModel> Evolutions { get; set; } = new List<PokemonModel>();
        public List<PokemonId> EvolutionIds { get; set; } = new List<PokemonId>();


        public PokemonModel() { }

        public PokemonModel(PokemonSettings settings)
        {
            BuildFromSettings(settings);
        }

        private void BuildFromSettings(PokemonSettings settings)
        {
            Id = settings.PokemonId;
            ModelScale = settings.ModelScale;
            Type = settings.Type;
            Type2 = settings.Type2;
            Camera = settings.Camera;
            CandyToEvolve = settings.CandyToEvolve;
            HeightStdDev = settings.HeightStdDev;
            KmBuddyDistance = settings.KmBuddyDistance;
            ParentPokemonId = settings.ParentPokemonId;
            PokedexHeightM = settings.PokedexHeightM;
            PokedexWeightKg = settings.PokedexWeightKg;
            EvolutionIds = settings.EvolutionIds.ToList();
        }

        public PokemonModel(PokemonId id)
        {
            PokemonSettings pokeman = GameClient.GetExtraDataForPokemon(id);
            BuildFromSettings(pokeman);

            PokedexEntry pokedexEntry = GameClient.PokedexInventory.FirstOrDefault(x => x.PokemonId == id);
            BuildFromPokedexEntry(pokedexEntry);

            PokemonModel pokemonModel = new PokemonModel(pokeman, pokedexEntry);

            var CurrPokemon = pokemonModel;
            Evolutions.Add(pokemonModel);
            while (CurrPokemon.ParentPokemonId != PokemonId.Missingno)
            {

                Evolutions.Insert(0, FromId(CurrPokemon.ParentPokemonId));
                CurrPokemon = FromId(CurrPokemon.ParentPokemonId);
            }
            CurrPokemon = pokemonModel;
            while (CurrPokemon.EvolutionIds.Count > 0)
            {
                foreach (PokemonId ev in CurrPokemon.EvolutionIds) //for Eevee
                    Evolutions.Add(FromId(ev));
                CurrPokemon = new PokemonModel(CurrPokemon.EvolutionIds.ElementAt(0));
            }


        }

        private PokemonModel FromId(PokemonId id)
        {
            PokemonModel temp = new PokemonModel();
            PokemonSettings pokeman = GameClient.GetExtraDataForPokemon(id);
            temp.BuildFromSettings(pokeman);

            return temp;
        }

        public PokemonModel(PokemonSettings settings, PokedexEntry pokedexEntry)
        {
            BuildFromSettings(settings);
            BuildFromPokedexEntry(pokedexEntry);
        }

        public void BuildFromPokedexEntry(PokedexEntry pokedexEntry)
        {
            if (pokedexEntry != null)
            {
                TimesCaptured = pokedexEntry.TimesCaptured;
                TimesEncountered = pokedexEntry.TimesEncountered;
            }
        }

        public int TimesEncountered { get; set; }

        public int TimesCaptured { get; set; }

        public float PokedexWeightKg { get; set; }

        public float PokedexHeightM { get; set; }

        public PokemonId ParentPokemonId { get; set; }

        public float KmBuddyDistance { get; set; }

        public float HeightStdDev { get; set; }
        public float ModelScale { get; set; } = 1;
        public PokemonType Type { get; set; } = PokemonType.Bug;
        public PokemonType Type2 { get; set; } = PokemonType.Bug;
        public CameraAttributes Camera { get; set; }

        public int CandyToEvolve { get; set; } = 0;
    }

    public static class PokemonModelExtensions
    {

    }

}
