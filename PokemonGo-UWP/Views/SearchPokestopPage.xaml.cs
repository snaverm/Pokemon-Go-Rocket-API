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
    public sealed partial class SearchPokestopPage : Page
    {
        public SearchPokestopPage()
        {
            this.InitializeComponent();
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }


        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            App.ViewModelLocator.GameManagerViewModel.SearchInCooldown += GameManagerViewModelOnSearchInCooldown;
            App.ViewModelLocator.GameManagerViewModel.SearchInventoryFull += GameManagerViewModelOnSearchInventoryFull;
            App.ViewModelLocator.GameManagerViewModel.SearchOutOfRange += GameManagerViewModelOnSearchOutOfRange;
            App.ViewModelLocator.GameManagerViewModel.SearchSuccess += GameManagerViewModelOnSearchSuccess;
            // Add also handlers to report which items the user gained after the animation
            SpinPokestopImage.Completed += (s, e) => ShowGatheredItemsMenu.Begin(); ;
        }

        private void UnsubscribeToCaptureEvents()
        {
            App.ViewModelLocator.GameManagerViewModel.SearchInCooldown -= GameManagerViewModelOnSearchInCooldown;
            App.ViewModelLocator.GameManagerViewModel.SearchInventoryFull -= GameManagerViewModelOnSearchInventoryFull;
            App.ViewModelLocator.GameManagerViewModel.SearchOutOfRange -= GameManagerViewModelOnSearchOutOfRange;
            App.ViewModelLocator.GameManagerViewModel.SearchSuccess -= GameManagerViewModelOnSearchSuccess;
        }

        private void GameManagerViewModelOnSearchOutOfRange(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            OutOfRangeTextBlock.Visibility = Visibility.Visible;

        }

        private void GameManagerViewModelOnSearchInventoryFull(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            InventoryFulleTextBlock.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchInCooldown(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            CooldownTextBlock.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchSuccess(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            SpinPokestopImage.Begin();
        }

        #endregion

    }
}
