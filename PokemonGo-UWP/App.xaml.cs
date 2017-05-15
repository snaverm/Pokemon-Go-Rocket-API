using Microsoft.HockeyApp;
using NotificationsExtensions.Tiles;
using POGOProtos.Data;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Logging;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
using Windows.Phone.Devices.Notification;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokemonGo_UWP.Utils.Helpers;
using PokemonGo_UWP.Controls;

namespace PokemonGo_UWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    sealed partial class App : BootStrapper
    {

        #region Private Members

        /// <summary>
        ///     We use it to notify that we found at least one catchable Pokemon in our area.
        /// </summary>
        private VibrationDevice _vibrationDevice;

        /// <summary>
        ///     Stores the current <see cref="DisplayRequest"/> instance for the app.
        /// </summary>
        private readonly DisplayRequest _displayRequest;

        private readonly Utils.Helpers.ProximityHelper _proximityHelper;

        #endregion

        #region Properties

        /// <summary>
        /// The TileUpdater instance for the app.
        /// </summary>
        public static TileUpdater LiveTileUpdater { get; private set; }

        #endregion

        #region Constructor

        public App()
        {
            InitializeComponent();
            SplashFactory = e => new Splash(e);

            // ensure unobserved task exceptions (unawaited async methods returning Task or Task<T>) are handled
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // ensure general app exceptions are handled
            Application.Current.UnhandledException += App_UnhandledException;

            // Init HockeySDK
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.Configure(ApplicationKeys.HockeyAppToken);

            // Set this in the instance constructor to prevent the creation of an unnecessary static constructor.
            _displayRequest = new DisplayRequest();

            // Initialize the Live Tile Updater.
            LiveTileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

            // Init the proximity helper to turn the screen off when it's in your pocket
            _proximityHelper = new ProximityHelper();
            _proximityHelper.EnableDisplayAutoOff(false);
        }

        #endregion

        #region Event Handlers

        private static async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await ExceptionHandler.HandleException(e.Exception);
            // We should be logging these exceptions too so they can be tracked down.
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.TrackException(e.Exception);
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Logger.Write(e.Exception.Message);
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.TrackException(e.Exception);
        }

        private async void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var tmpNetworkStatus = connectionProfile != null &&
                                  connectionProfile.GetNetworkConnectivityLevel() ==
                                  NetworkConnectivityLevel.InternetAccess;
            await WindowWrapper.Current().Dispatcher.DispatchAsync(() => {
                if (tmpNetworkStatus)
                {
                    Logger.Write("Network is online");
                    Busy.SetBusy(false);
                }
                else
                {
                    Logger.Write("Network is offline");
                    Busy.SetBusy(true, Utils.Resources.CodeResources.GetString("WaitingForNetworkText"));
                }
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PokemonsInventory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SettingsService.Instance.LiveTileMode == LiveTileModes.Off) return;
            // Using a Switch here because we might handle other changed events in other ways.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    UpdateLiveTile(e.NewItems.Cast<PokemonData>().OrderByDescending(c => c.Cp).ToList());
                    break;

            }
        }

        /// <summary>
        ///     Vibrates and/or plays a sound when new pokemons are in the area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CatchablePokemons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (SettingsService.Instance.IsVibrationEnabled)
                _vibrationDevice?.Vibrate(TimeSpan.FromMilliseconds(500));
            AudioUtils.PlaySound(AudioUtils.POKEMON_FOUND_DING);
        }

        #endregion

        #region Application Lifecycle

        /// <summary>
        ///     Disable vibration on suspending
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="prelaunchActivated"></param>
        /// <returns></returns>
        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {                        
            GameClient.PokemonsInventory.CollectionChanged -= PokemonsInventory_CollectionChanged;
            GameClient.CatchablePokemons.CollectionChanged -= CatchablePokemons_CollectionChanged;
            NetworkInformation.NetworkStatusChanged -= NetworkInformationOnNetworkStatusChanged;            

            if (SettingsService.Instance.IsBatterySaverEnabled)
                _proximityHelper.EnableDisplayAutoOff(false);

            if (SettingsService.Instance.LiveTileMode == LiveTileModes.Peek)
            {
                LiveTileUpdater.EnableNotificationQueue(false);
            }
            return base.OnSuspendingAsync(s, e, prelaunchActivated);
        }

        public override void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
            if (SettingsService.Instance.IsBatterySaverEnabled)
                _proximityHelper.EnableDisplayAutoOff(true);
        }

        /// <summary>
        ///     This runs everytime the app is launched, even after suspension, so we use this to initialize stuff
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
#if DEBUG
            // Init logger
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));
#endif            
            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }

            // Enter into full screen mode
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            ApplicationView.GetForCurrentView().FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Standard;            

            // Forces the display to stay on while we play
            //_displayRequest.RequestActive();
            WindowWrapper.Current().Window.VisibilityChanged += WindowOnVisibilityChanged;

            // Initialize Map styles
            await MapStyleHelpers.Initialize();

            // Turn the display off when the proximity stuff detects the display is covered (battery saver)
            if (SettingsService.Instance.IsBatterySaverEnabled)
                _proximityHelper.EnableDisplayAutoOff(true);

            // Init vibration device
            if (ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                _vibrationDevice = VibrationDevice.GetDefault();
            }

            if (SettingsService.Instance.LiveTileMode == LiveTileModes.Peek)
            {
                LiveTileUpdater.EnableNotificationQueue(true);
            }

            // Check for network status
            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;

            // Respond to changes in inventory and Pokemon in the immediate viscinity.
            GameClient.PokemonsInventory.CollectionChanged += PokemonsInventory_CollectionChanged;
            GameClient.CatchablePokemons.CollectionChanged += CatchablePokemons_CollectionChanged;         

            await Task.CompletedTask;
        }        

        /// <summary>
        ///
        /// </summary>
        /// <param name="startKind"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            bool forceToMainPage = false;
            // Check for updates (ignore resume)
            if (startKind == StartKind.Launch)
            {
                var latestUpdateInfo = await UpdateManager.IsUpdateAvailable();

                while (latestUpdateInfo == null || latestUpdateInfo.Status == UpdateManager.UpdateStatus.NoInternet)
                {
                    var dialog = new MessageDialog("Do you want try to connect again?", "No internet connection");

                    dialog.Commands.Add(new UICommand(Utils.Resources.CodeResources.GetString("YesText")) { Id = 0 });
                    dialog.Commands.Add(new UICommand(Utils.Resources.CodeResources.GetString("NoText")) { Id = 1 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsyncQueue();

                    if ((int)result.Id != 0)
                        App.Current.Exit();
                    else
                        latestUpdateInfo = await UpdateManager.IsUpdateAvailable();
                }

                if (latestUpdateInfo.Status == UpdateManager.UpdateStatus.UpdateAvailable)
                {
                    var dialog =
                        new MessageDialog(string.Format(Utils.Resources.CodeResources.GetString("UpdatedVersion"),
                            latestUpdateInfo.Version, latestUpdateInfo.Description));

                    dialog.Commands.Add(new UICommand(Utils.Resources.CodeResources.GetString("YesText")) { Id = 0 });
                    dialog.Commands.Add(new UICommand(Utils.Resources.CodeResources.GetString("NoText")) { Id = 1 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsyncQueue();

                    if ((int)result.Id != 0)
                        return;

                    var t1 = UpdateManager.InstallUpdate();
                    forceToMainPage = true;
                }
                else if (latestUpdateInfo.Status == UpdateManager.UpdateStatus.UpdateForced)
                {
                    //start forced update
                    var t1 = UpdateManager.InstallUpdate();
                    forceToMainPage = true;
                }
                else if (latestUpdateInfo.Status == UpdateManager.UpdateStatus.NextVersionNotReady)
                {
                    var twoLines = Environment.NewLine + Environment.NewLine;
                    var dialog = new MessageDialog("Niantic has raised the minimum API level above what we have access to, so we've temporarily disabled the app to protect your account." + 
                        twoLines + "DO NOT attempt to bypass this check. Accounts that access lower APIs than the minimum WILL BE BANNED by Niantic." + twoLines + 
                        "An update will be ready soon. Please DO NOT open an issue on GitHub, you are seeing this message because we already know about it, and this is how we're telling you. " +
                        "Thank you for your patience." + twoLines + "- The PoGo-UWP Team");
                    dialog.Commands.Add(new UICommand("OK"));
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsyncQueue();

                    App.Current.Exit();
                }
            }


            AsyncSynchronizationContext.Register();
            var currentAccessToken = GameClient.LoadAccessToken();
            if (currentAccessToken == null || forceToMainPage)
            {
                await NavigationService.NavigateAsync(typeof(MainPage));
            }
            else
            {
                await GameClient.InitializeClient();
                NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.AppStart);
            }


            await Task.CompletedTask;
        }

        private void WindowOnVisibilityChanged(object sender, VisibilityChangedEventArgs visibilityChangedEventArgs)
        {
            if (!visibilityChangedEventArgs.Visible)
                _displayRequest.RequestRelease();
            else
                _displayRequest.RequestActive();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pokemonList"></param>
        /// <remarks>
        /// advancedrei: The LiveTileUpdater is on teh App class, so this has to stay here for now. Might refactor later.
        /// </remarks>
        internal static void UpdateLiveTile(IList<PokemonData> pokemonList)
        {
            // Let's run this on a separate thread.
            Task.Run(() => {
                try
                {
                    TileContent tile = null;
                    var images = new List<string>();
                    var mode = SettingsService.Instance.LiveTileMode;

                    // Generate the images list for multi-image modes.
                    if (mode == LiveTileModes.People || mode == LiveTileModes.Photo)
                    {
                        images.AddRange(pokemonList.Select(c => new PokemonDataWrapper(c).ImageFileName));
                    }

                    if (mode != LiveTileModes.Peek)
                    {
                        LiveTileUpdater.EnableNotificationQueue(true);
                    }
                    else
                    {
                        LiveTileUpdater.EnableNotificationQueue(false);
                        LiveTileUpdater.Clear();
                    }

                    switch (mode)
                    {
                        case LiveTileModes.Off:
                            break;
                        case LiveTileModes.Peek:
                            foreach (PokemonData pokemonData in pokemonList)
                            {
                                if (LiveTileUpdater.GetScheduledTileNotifications().Count >= 300) return;
                                var peekTile = LiveTileHelper.GetPeekTile(new PokemonDataWrapper(pokemonData));
                                var scheduled = new ScheduledTileNotification(peekTile.GetXml(),
                                    DateTimeOffset.Now.AddSeconds((pokemonList.IndexOf(pokemonData) * 30) + 1));
                                LiveTileUpdater.AddToSchedule(scheduled);
                            }
                            break;
                        case LiveTileModes.People:
                            tile = LiveTileHelper.GetPeopleTile(images);
                            LiveTileUpdater.Update(new TileNotification(tile.GetXml()));
                            break;
                        case LiveTileModes.Photo:
                            tile = LiveTileHelper.GetPhotosTile(images);
                            LiveTileUpdater.Update(new TileNotification(tile.GetXml()));
                            break;
                        case LiveTileModes.Transparent:
                            tile = LiveTileHelper.GetImageTile("LiveTiles/Transparent/Square44x44Logo.scale-400.png");
                            LiveTileUpdater.Update(new TileNotification(tile.GetXml()));
                            break;
                    }
                    if (tile != null)
                    {
                        //Logger.Write(tile.GetXml().GetXml());
                    }

                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message);
                    if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                        HockeyClient.Current.TrackException(ex);
                }
            });
        }

        #endregion

    }

}
