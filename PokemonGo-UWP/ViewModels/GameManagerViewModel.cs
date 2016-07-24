using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Phone.Devices.Notification;
using Windows.UI.Popups;
using PokeAPI;
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
            _inventory = new Inventory(_client);
        }

        #region Client

        private readonly Client _client;
        private readonly ISettings _clientSettings;
        private readonly Inventory _inventory;

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
                        InventoryDelta = (await _client.GetInventory()).InventoryDelta;                        
                        PlayerStats = InventoryDelta.InventoryItems.First(
                                item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;
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

        #region Base Logic

        /// <summary>
        /// Player's inventory
        /// </summary>
        private InventoryDelta _inventoryDelta;

        /// <summary>
        /// We use it to notify that we found at least one catchable Pokemon in our area
        /// </summary>
        private readonly VibrationDevice _vibrationDevice = VibrationDevice.GetDefault();

        /// <summary>
        /// Retrieves data for the current position
        /// </summary>
        private async void UpdateMapData()
        {
            // Report it to client and find things nearby
            await _client.UpdatePlayerLocation(CurrentGeoposition.Coordinate.Point.Position.Latitude, CurrentGeoposition.Coordinate.Point.Position.Longitude);
            var mapObjects = await _client.GetMapObjects();
            // Replace data with the new ones                                  
            var catchableTmp = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);
            Logger.Write($"Found {catchableTmp.Count()} catchable pokemons");            
            if (catchableTmp.Count() != CatchablePokemons.Count) _vibrationDevice.Vibrate(TimeSpan.FromMilliseconds(500));
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

        /// <summary>
        /// Retrieves the inventory for the player
        /// </summary>
        private async void UpdateInventory()
        {
            foreach (MiscEnums.Item itemType in Enum.GetValues(typeof(MiscEnums.Item)))
            {
                Inventory[itemType] = await _inventory.GetItemAmountByType(itemType);
            }
        }
        #endregion

        #region Pokemon Catching

        private DelegateCommand<MapPokemonWrapper> _tryCatchPokemon;

        /// <summary>
        /// Pokemon that we're trying to capture
        /// </summary>
        private MapPokemonWrapper _currentPokemon;

        private EncounterResponse _currentEncounter;

        public DelegateCommand<MapPokemonWrapper> TryCatchPokemon => _tryCatchPokemon ?? (
            _tryCatchPokemon = new DelegateCommand<MapPokemonWrapper>(async pokemon =>
            {
                Logger.Write($"Catching {pokemon.PokemonId}");
                Busy.SetBusy(true, $"Loading encounter with {pokemon.PokemonId}");
                // Get the pokemon and navigate to capture page where we can handle capturing
                CurrentPokemon = pokemon;
                CurrentEncounter = await _client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);                   
                NavigationService.Navigate(typeof(CapturePokemonPage));
                Busy.SetBusy(false);
                
                //var encounterPokemonResponse = await _client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);                
                ////var pokemonCP = encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp;                

                //CatchPokemonResponse caughtPokemonResponse;
                //do
                //{
                //    //if (encounterPokemonResponse?.CaptureProbability.CaptureProbability_.First() < 0.4)
                //    //{
                //    //    //Throw berry is we can
                //    //    await UseBerry(pokemon.EncounterId, pokemon.SpawnpointId);
                //    //}
                //    // TODO: proper capturing!
                //    caughtPokemonResponse = await _client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL);
                //    await Task.Delay(2000);
                //}
                //while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed);
                //Logger.Write(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"We caught a {pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} using a {MiscEnums.Item.ITEM_POKE_BALL}" : $"{pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} got away while using a {MiscEnums.Item.ITEM_POKE_BALL}..");
                //await new MessageDialog((caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"We caught a {pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} using a {MiscEnums.Item.ITEM_POKE_BALL}" : $"{pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} got away while using a {MiscEnums.Item.ITEM_POKE_BALL}..")).ShowAsync();
                //UpdateMapData();
                // After capturing we need to update the map because the Pokemon may be no longer there                
            }, pokemon => true)
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

        public InventoryDelta InventoryDelta
        {
            get { return _inventoryDelta; }
            set { Set(ref _inventoryDelta, value); }
        }

        /// <summary>
        /// Stores (Item, count) pairs
        /// </summary>
        public ObservableDictionary<MiscEnums.Item, int> Inventory { get; set; } = new ObservableDictionary<MiscEnums.Item, int>();

        /// <summary>
        /// Collection of Pokemon in 1 step from current position
        /// </summary>
        public ObservableCollection<MapPokemonWrapper> CatchablePokemons { get; set; } = new ObservableCollection<MapPokemonWrapper>();

        /// <summary>
        /// Collection of Pokemon in 2 steps from current position
        /// </summary>
        public ObservableCollection<NearbyPokemon> NearbyPokemons { get; set; } = new ObservableCollection<NearbyPokemon>();

        public MiscEnums.Item SelectedBall { get; set; } = MiscEnums.Item.ITEM_POKE_BALL;

        public int SelectedBallCount => Inventory[SelectedBall];

        public MapPokemonWrapper CurrentPokemon
        {
            get
            {
                return _currentPokemon;
            }
            set { Set(ref _currentPokemon, value); }
        }

        public EncounterResponse CurrentEncounter
        {
            get
            {
                return _currentEncounter;
            }
            set { Set(ref _currentEncounter, value); }
        }

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
            UpdateMapData();                  
        }

        #endregion
    }
}