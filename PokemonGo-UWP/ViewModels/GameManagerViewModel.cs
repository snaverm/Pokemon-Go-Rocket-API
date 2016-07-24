using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Phone.Devices.Notification;
using Windows.UI.Popups;
using AllEnum;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logging;
using PokemonGo.RocketAPI.Logic;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Common;
using Template10.Mvvm;
using Universal_Authenticator_v2.Views;
using Item = PokemonGo.RocketAPI.GeneratedCode.Item;

namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    ///     Main class for the game.
    ///     This handles connection to client and UI updating via binding.
    /// </summary>
    public class GameManagerViewModel : ViewModelBase
    {
        #region ctor

        public GameManagerViewModel()
        {
            // Client init            
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));
            _clientSettings = new Settings();
            _client = new Client(_clientSettings);
            _inventory = new Inventory(_client);
        }

        #endregion

        #region Client Vars

        private readonly Client _client;
        private readonly ISettings _clientSettings;
        private readonly Inventory _inventory;

        #endregion

        #region Game Management Vars

        /// <summary>
        /// We use it to notify that we found at least one catchable Pokemon in our area
        /// </summary>
        private readonly VibrationDevice _vibrationDevice = VibrationDevice.GetDefault();

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        private Profile _playerProfile;

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        private PlayerStats _playerStats;

        /// <summary>
        /// Player's inventory
        /// TODO: do we really need it?
        /// </summary>
        private InventoryDelta _inventoryDelta;        

        /// <summary>
        /// Pokemon that we're trying to capture
        /// </summary>
        private MapPokemonWrapper _currentPokemon;

        /// <summary>
        /// Encounter for the Pokemon that we're trying to capture
        /// </summary>
        private EncounterResponse _currentEncounter;

        /// <summary>
        /// Current item for capture page
        /// </summary>
        private Item _selectedCaptureItem;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public Profile PlayerProfile {
            get
            {
                return _playerProfile;                
            }
            set
            {
                Set(ref _playerProfile, value); 
                
            }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats {
            get
            {
                return _playerStats;
            }
            set
            {
                Set(ref _playerStats, value);

            }
        }

        public InventoryDelta InventoryDelta
        {
            get { return _inventoryDelta; }
            set { Set(ref _inventoryDelta, value); }
        }

        /// <summary>
        /// Collection of Pokemon in 1 step from current position
        /// </summary>
        public ObservableCollection<MapPokemonWrapper> CatchablePokemons { get; set; } = new ObservableCollection<MapPokemonWrapper>();

        /// <summary>
        /// Collection of Pokemon in 2 steps from current position
        /// </summary>
        public ObservableCollection<NearbyPokemon> NearbyPokemons { get; set; } = new ObservableCollection<NearbyPokemon>();

        /// <summary>
        /// Stores the current inventory
        /// </summary>
        public ObservableCollection<Item> Inventory { get; set; } = new ObservableCollection<Item>();
        
        /// <summary>
        /// Pokemon that we're trying to capture
        /// </summary>
        public MapPokemonWrapper CurrentPokemon
        {
            get
            {                
                return _currentPokemon;
            }
            set { Set(ref _currentPokemon, value); }
        }

        /// <summary>
        /// Encounter for the Pokemon that we're trying to capture
        /// </summary>
        public EncounterResponse CurrentEncounter
        {
            get
            {
                return _currentEncounter;
            }
            set
            {
                Set(ref _currentEncounter, value); 
                
            }
        }

        /// <summary>
        /// Current item for capture page
        /// </summary>
        public Item SelectedCaptureItem
        {
            get { return _selectedCaptureItem; }
            set { Set(ref _selectedCaptureItem, value); }
        }

        #endregion

        #region Game Logic

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


        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Logging in...");
                try
                {                    
                    if (!await _client.DoPtcLogin(PtcUsername, PtcPassword))
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
                        UpdatePlayerData();
                        Busy.SetBusy(true, "Getting player items");
                        UpdateInventory();
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

        #region Data Update

        /// <summary>
        /// Retrieves data for the current position
        /// </summary>
        private async void UpdateMapData()
        {
            // Report it to client and find things nearby
            await _client.UpdatePlayerLocation(CurrentGeoposition.Coordinate.Point.Position.Latitude, CurrentGeoposition.Coordinate.Point.Position.Longitude);
            var mapObjects = await _client.GetMapObjects();
            // Replace data with the new ones                                  
            var catchableTmp = new List<MapPokemon>(mapObjects.MapCells.SelectMany(i => i.CatchablePokemons));
            Logger.Write($"Found {catchableTmp.Count} catchable pokemons");
            if (catchableTmp.Count != CatchablePokemons.Count) _vibrationDevice.Vibrate(TimeSpan.FromMilliseconds(500));
            await Dispatcher.DispatchAsync(() => {
                CatchablePokemons.Clear();
                foreach (var pokemon in catchableTmp)
                {
                    CatchablePokemons.Add(new MapPokemonWrapper(pokemon));
                }
            });
            var nearbyTmp = new List<NearbyPokemon>(mapObjects.MapCells.SelectMany(i => i.NearbyPokemons));
            Logger.Write($"Found {nearbyTmp.Count} nearby pokemons");
            await Dispatcher.DispatchAsync(() => {
                NearbyPokemons.Clear();
                foreach (var pokemon in nearbyTmp)
                {
                    NearbyPokemons.Add(pokemon);
                }
            });
            // TODO: PokeStops
        }

        /// <summary>
        /// Updates player data like profile and stats
        /// </summary>
        private async void UpdatePlayerData()
        {
            PlayerProfile = (await _client.GetProfile()).Profile;
            InventoryDelta = (await _client.GetInventory()).InventoryDelta;
            PlayerStats = InventoryDelta.InventoryItems.First(
                    item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;
        }

        /// <summary>
        /// Retrieves inventory for the player
        /// </summary>
        private async void UpdateInventory()
        {
            var inventoryTmp = new List<Item>(await _inventory.GetItems());
            await Dispatcher.DispatchAsync(() => {
                Inventory.Clear();
                foreach (var item in inventoryTmp)
                {                    
                    Inventory.Add(item);
                }
            });
        }
        #endregion

        #region Pokemon Catching

        private DelegateCommand<MapPokemonWrapper> _tryCatchPokemon;

        /// <summary>
        /// We're just navigating to the capture page, reporting that the player wants to capture the selected Pokemon.
        /// The only logic here is to check if the encounter was successful before navigating, everything else is handled by the actual capture method.
        /// </summary>
        public DelegateCommand<MapPokemonWrapper> TryCatchPokemon => _tryCatchPokemon ?? (
            _tryCatchPokemon = new DelegateCommand<MapPokemonWrapper>(async pokemon =>
            {
                Logger.Write($"Catching {pokemon.PokemonId}");
                Busy.SetBusy(true, $"Loading encounter with {pokemon.PokemonId}");
                // Get the pokemon and navigate to capture page where we can handle capturing
                CurrentPokemon = pokemon;
                CurrentEncounter = await _client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);
                // Reset selected item to default one
                SelectedCaptureItem = Inventory.First(item => item.Item_ == ItemType.Pokeball);
                Busy.SetBusy(false);
                if (CurrentEncounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                    NavigationService.Navigate(typeof(CapturePokemonPage));
                else
                {
                    // Encounter failed, probably the Pokemon ran away
                    await new MessageDialog("Pokemon ran away, sorry :(").ShowAsync();
                }
            }, pokemon => true)
            );
        #endregion

        #endregion        

        #region GPS & Maps

        //private Compass _compass;

        private Geolocator _geolocator;

        private Geoposition _currentGeoposition;

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
            UpdateMapData();                  
        }

        #endregion
    }
}