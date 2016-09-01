using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Google.Protobuf.WellKnownTypes;
using PokemonGo_UWP.Utils;
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
        public Uri BackgroundImage => (Uri)new PokemonTypeToBackgroundImageConverter().Convert(this.Id,typeof(Uri),null,string.Empty);
        public List<PokemonModel> Evolutions { get; set; } = new List<PokemonModel>();


        public float ModelScale { get; set; } = 1;
        public PokemonType Type { get; set; } = PokemonType.Bug;
        public PokemonType Type2 { get; set; } = PokemonType.Bug;
        public CameraAttributes Camera { get; set; }

        public int CandyToEvolve { get; set; } = 0;

        public PokemonModel(PokemonSettings settings)
        {
            Id = settings.PokemonId;
            ModelScale = settings.ModelScale;
            Type = settings.Type;
            Type2 = settings.Type2;
            Camera = settings.Camera;
            CandyToEvolve = settings.CandyToEvolve;
        }
    }

    public static class PokemonModelExtensions
    {
        
    }

}
