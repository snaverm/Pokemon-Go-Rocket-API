using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PokemonGo_UWP.Utils
{
    public class PokemonIdToPokemonSpriteConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {            
            return new Uri($"http://pokeapi.co/media/sprites/pokemon/{(int)value}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }
}
