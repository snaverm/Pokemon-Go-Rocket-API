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
using Template10.Common;

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

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }

        #region Overrides of Page

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }

        #endregion

        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            App.ViewModelLocator.GameManagerViewModel.CatchSuccess += GameManagerViewModelOnCatchSuccess;
            App.ViewModelLocator.GameManagerViewModel.CatchEscape += GameManagerViewModelOnCatchEscape;
            App.ViewModelLocator.GameManagerViewModel.CatchMissed += GameManagerViewModelOnCatchMissed;
            // Add also handlers to enable the button once the animation is done
            CatchSuccess.Completed += (s, e) => LaunchPokeballButton.IsEnabled = true;
            CatchEscape.Completed += (s, e) => BootStrapper.Current.NavigationService.GoBack();
            CatchMissed.Completed += (s, e) => LaunchPokeballButton.IsEnabled = true;
        }

        private void UnsubscribeToCaptureEvents()
        {
            App.ViewModelLocator.GameManagerViewModel.CatchSuccess -= GameManagerViewModelOnCatchSuccess;
            App.ViewModelLocator.GameManagerViewModel.CatchEscape -= GameManagerViewModelOnCatchEscape;
            App.ViewModelLocator.GameManagerViewModel.CatchMissed -= GameManagerViewModelOnCatchMissed;
        }

        private void GameManagerViewModelOnCatchMissed(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchMissed.Begin();
        }

        private void GameManagerViewModelOnCatchEscape(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchEscape.Begin();
        }

        private void GameManagerViewModelOnCatchSuccess(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchSuccess.Begin();
        }

        #endregion

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
