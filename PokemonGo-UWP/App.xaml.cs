using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Data;
using Microsoft.HockeyApp;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.ViewModels;
using PokemonGo_UWP.Views;
using Template10.Common;
using System;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Popups;

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

        /// <summary>
        /// Used to prevent lockscreen while playing
        /// </summary>
        public static DisplayRequest DisplayRequest;

        public App()
        {
            InitializeComponent();

            // Init HockeySDK
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.Configure(ApplicationKeys.HockeyAppToken);

            // Forces the display to stay on while we play
            DisplayRequest = new DisplayRequest();
            DisplayRequest.RequestActive();                            
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
            // Get a static reference to viewmodel locator to use it within viewmodels
            ViewModelLocator = (ViewModelLocator) Current.Resources["Locator"];
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {            
            await NavigationService.NavigateAsync(typeof(MainPage));
            if (!string.IsNullOrEmpty(SettingsService.Instance.PtcAuthToken))
            {
                // We have a stored token, let's go to game page 
                NavigationService.Navigate(typeof(GameMapPage), true);
                //await ViewModelLocator.GameManagerViewModel.InitGame(true);
            }

            // Check for updates
            var latestVersionUri = await UpdateManager.IsUpdateAvailable();
            if (latestVersionUri != null)
            {                
                var dialog = new MessageDialog(
                $"An updated version is available on\n{latestVersionUri}\nDo you want to visit the link?");

                dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
                dialog.Commands.Add(new UICommand("No") { Id = 1 });
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                
                var result = await dialog.ShowAsyncQueue();
                if ((int) result.Id != 0) return;
                await Launcher.LaunchUriAsync(new Uri(latestVersionUri));
            }
            await Task.CompletedTask;
        }
    }
}