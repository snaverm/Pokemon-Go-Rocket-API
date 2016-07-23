using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.UI.Popups;
using PokeAPI;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logging;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Common;
using Template10.Mvvm;
using Universal_Authenticator_v2.Views;
using Pokemon = PokeAPI.Pokemon;

namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    ///     Main class for the game.
    ///     This handles connection to client and UI updating via binding.
    /// </summary>
    public class GameManagerViewModel : ViewModelBase
    {
        public GameManagerViewModel()
        {
            // Client init
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));
            DataFetcher.ShouldCacheData = true;
            _clientSettings = new Settings();
            _client = new Client(_clientSettings);
        }

        #region Logic

        private readonly Client _client;
        private readonly ISettings _clientSettings;

        #endregion

        #region PTC Login

        private string _ptcUsername;

        public string PtcUsername
        {
            get { return _ptcUsername; }
            set
            {
                Set(ref _ptcUsername, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _ptcPassword;

        public string PtcPassword
        {
            get { return _ptcPassword; }
            set
            {
                Set(ref _ptcPassword, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isLoggedIn;

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set { Set(ref _isLoggedIn, value); }
        }

        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Logging in...");
                try
                {
                    IsLoggedIn = await _client.DoPtcLogin(PtcUsername, PtcPassword);
                    if (!IsLoggedIn)
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog("Wrong username/password or offline server, please try again.").ShowAsync();
                    }
                    else
                    {
                        // Login worked, update data and go to game page
                        Busy.SetBusy(true, "Getting GPS position");
                        await InitGps();
                        Busy.SetBusy(true, "Getting player data");
                        PlayerProfile = (await _client.GetProfile()).Profile;
                        PlayerStats =
                            (await _client.GetInventory()).InventoryDelta.InventoryItems.First(
                                item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;
                        await NavigationService.NavigateAsync(typeof(GameMapPage));
                        // Avoid going back to login page using back button
                        NavigationService.ClearHistory();                        
                    }
                }
                catch (Exception)
                {
                    await new MessageDialog("PTC login is probably down, please retry later.").ShowAsync();
                }
                finally
                {
                    Busy.SetBusy(false);
                }                
            }, () => !string.IsNullOrEmpty(PtcUsername) && !string.IsNullOrEmpty(PtcPassword))
            );

        #endregion

        #region Bindable Game Vars        

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public Profile PlayerProfile { get; private set; }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats { get; private set; }

        /// <summary>
        /// Collection of Pokemon in 1 step from current position
        /// </summary>
        public ObservableCollection<MapPokemonWrapper> CatchablePokemons { get; set; } = new ObservableCollection<MapPokemonWrapper>();

        /// <summary>
        /// Collection of Pokemon in 2 steps from current position
        /// </summary>
        public ObservableCollection<NearbyPokemon> NearbyPokemons { get; set; } = new ObservableCollection<NearbyPokemon>();

        /// <summary>
        /// Key for Bing's Map Service (not included in GIT, you need to get your own token to use maps!)
        /// </summary>
        public string MapServiceToken => BingKey.MapServiceToken;

        /// <summary>
        /// Current GPS position
        /// </summary>
        public Geoposition CurrentGeoposition
        {
            get { return _currentGeoposition; }
            set { Set(ref _currentGeoposition, value); }
        }

        ///// <summary>
        ///// Current reading from Compass
        ///// </summary>
        //public double CompassHeading
        //{
        //    get { return _compassHeading; }
        //    set { Set(ref _compassHeading, value); }
        //}

        #endregion

        #region GPS

        //private Compass _compass;

        private Geolocator _geolocator;

        private Geoposition _currentGeoposition;

        //private double _compassHeading;

        private async Task InitGps()
        {
            // Set your current location.
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // Get the current location.
                    Logger.Write("GPS activated");
                    _geolocator = new Geolocator
                    {
                        DesiredAccuracy = PositionAccuracy.High,
                        DesiredAccuracyInMeters = 5,
                        ReportInterval = 3000,
                        MovementThreshold = 5
                    };
                    CurrentGeoposition = await _geolocator.GetGeopositionAsync();
                    //_compass = Compass.GetDefault();
                    //if (_compass != null)
                    //{
                    //    Logger.Write("Compass activated");
                    //    _compass.ReportInterval = 100;
                    //    _compass.ReadingChanged += CompassOnReadingChanged;
                    //}
                    break;
                default:
                    Logger.Write("Error during GPS activation");
                    await new MessageDialog("GPS error, sorry :(").ShowAsync();
                    BootStrapper.Current.Exit();
                    break;
            }
            _geolocator.PositionChanged += GeolocatorOnPositionChanged;
        }

        //private void CompassOnReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
        //{
        //    if (Math.Abs(CompassHeading - args.Reading.HeadingMagneticNorth) > 20)
        //        CompassHeading = args.Reading.HeadingMagneticNorth;
        //}

        private async void GeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            // Get new position
            await Dispatcher.DispatchAsync(() => {
                CurrentGeoposition = args.Position;
            });            
            // Report it to client and find things nearby
            await _client.UpdatePlayerLocation(CurrentGeoposition.Coordinate.Point.Position.Latitude, CurrentGeoposition.Coordinate.Point.Position.Longitude);
            var mapObjects = await _client.GetMapObjects();
            // Replace data with the new ones                      
            var catchableTmp = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);
            Logger.Write($"Found {catchableTmp.Count()} catchable pokemons");
            await Dispatcher.DispatchAsync(() => {
                CatchablePokemons.Clear();
                foreach (var pokemon in catchableTmp)
                {                    
                    CatchablePokemons.Add(new MapPokemonWrapper(pokemon));
                }
            });
            var nearbyTmp = mapObjects.MapCells.SelectMany(i => i.NearbyPokemons);
            await Dispatcher.DispatchAsync(() => {                 
                NearbyPokemons.Clear();
                foreach (var pokemon in nearbyTmp)
                {
                    NearbyPokemons.Add(pokemon);
                }
            });
        }

        #endregion
    }
}