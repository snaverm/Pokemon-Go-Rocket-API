using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;

namespace PokemonGo_UWP.Utils.Maps
{
    /// <summary>
    /// <see cref="http://dotnetbyexample.blogspot.it/2015/10/windows-10-maps-part-7-using-external.html"/>
    /// </summary>
    internal interface ITileSource
    {
        MapTileDataSource TileSource { get; }
    }
}
