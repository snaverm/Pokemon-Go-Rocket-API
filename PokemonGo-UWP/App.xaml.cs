using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Data;
using PokemonGo_UWP.ViewModels;
using PokemonGo_UWP.Views;
using Template10.Common;
using System;
using PokemonGo_UWP.Utils;

namespace PokemonGo_UWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    sealed partial class App : BootStrapper
    {
        /// <summary>
        ///     Locator instance
        /// </summary>
        public static ViewModelLocator ViewModelLocator;

        public App()
        {
            InitializeComponent();
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // Get a static reference to viewmodel locator to use it within viewmodels
            ViewModelLocator = (ViewModelLocator) Current.Resources["Locator"];
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (string.IsNullOrEmpty(SettingsService.Instance.PtcAuthToken))
                NavigationService.Navigate(typeof(MainPage)); // No stored tokens
            else
            {
                // We have a stored token, let's go to game page 
                await ViewModelLocator.GameManagerViewModel.InitGame(true);
                NavigationService.Navigate(typeof(GameMapPage));                               
            }
            await Task.CompletedTask;
        }


    }
}