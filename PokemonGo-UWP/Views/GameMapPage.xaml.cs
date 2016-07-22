using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// TODO: bottom bar blur on AU http://stackoverflow.com/questions/36276856/uwp-app-realtime-blur-background-using-dx-compositor/36441888#36441888
    /// </summary>
    public sealed partial class GameMapPage : Page
    {
        public GameMapPage()
        {
            this.InitializeComponent();
            MapControl.Center = new Geopoint(new BasicGeoposition()
            {
                Latitude = 48.8530,
                Longitude = 2.3498
            });
        }

    }
}
