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
    public class OpenStreetMapSource : BaseHttpTileSource
    {

        private readonly static string[] TilePathPrefixes = { "a", "b", "c" };

        protected override void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            // TilePathPrefixes - load balancing + caching
            args.Request.Uri = new Uri($"http://{TilePathPrefixes[args.Y % 3]}.tile.openstreetmap.fr/hot/{args.ZoomLevel}/{args.X}/{args.Y}.png");
            deferral.Complete();
        }

        public override string ToString()
        {
            return "OpenStreetMap";
        }
    }
}
