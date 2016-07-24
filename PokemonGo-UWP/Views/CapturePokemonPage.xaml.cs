using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    /// </summary>
    public sealed partial class CapturePokemonPage : Page
    {
        public CapturePokemonPage()
        {
            this.InitializeComponent();
        }

        private void LaunchPokeballButton_OnClick(object sender, RoutedEventArgs e)
        {
            LaunchPokeballStoryboard.SpeedRatio = 0.25;
            LaunchPokeballStoryboard.Begin();
            // TODO: Pokemon scale inside ball
            //LaunchPokeballStoryboard.Completed
        }

        private void InventoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: replace code-behind for animations with Template10's behaviors?
            ShowInventoryMenuStoryboard.Begin();
        }

        private void CloseInventoryMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            HideInventoryMenuStoryboard.Begin();
        }
    }
}
