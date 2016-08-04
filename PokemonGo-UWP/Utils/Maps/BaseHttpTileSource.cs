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
    public abstract class BaseHttpTileSource : ITileSource
    {
        protected BaseHttpTileSource()
        {
            var t = new HttpMapTileDataSource {AllowCaching = true};            
            t.UriRequested += MapUriRequested;            
            TileSource = t;
        }

        public MapTileDataSource TileSource { get; private set; }

        protected abstract void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args);
    }
}
