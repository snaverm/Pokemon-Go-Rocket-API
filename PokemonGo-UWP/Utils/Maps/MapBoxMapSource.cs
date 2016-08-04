using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;

namespace PokemonGo_UWP.Utils.Maps
{
    public class MapBoxMapSource : BaseHttpTileSource
    {        

        protected override void MapUriRequested(HttpMapTileDataSource sender, MapTileUriRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();                             
            args.Request.Uri = new Uri($"https://api.mapbox.com/styles/v1/{ApplicationKeys.MapBoxStyle}/tiles/256/{args.ZoomLevel}/{args.X}/{args.Y}?access_token={ApplicationKeys.MapBoxToken}");
            deferral.Complete();
        }

        public override string ToString()
        {
            return "MapBox";
        }

    }
}
