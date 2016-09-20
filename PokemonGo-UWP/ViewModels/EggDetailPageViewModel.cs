using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Google.Protobuf;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace PokemonGo_UWP.ViewModels
{
    public class EggDetailPageViewModel : ViewModelBase
    {
        //#region Lifecycle Handlers

        ///// <summary>
        ///// </summary>
        ///// <param name="parameter">MapPokemonWrapper containing the Pokemon that we're trying to capture</param>
        ///// <param name="mode"></param>
        ///// <param name="suspensionState"></param>
        ///// <returns></returns>
        //public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
        //    IDictionary<string, object> suspensionState)
        //{
        //    if (suspensionState.Any())
        //    {
        //        // Recovering the state
        //        SelectedEggIncubator = new EggIncubator();
        //        CurrentEgg = JsonConvert.DeserializeObject<PokemonDataWrapper>((string)suspensionState[nameof(CurrentEgg)]);
        //        SelectedEggIncubator.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(SelectedEggIncubator)]).CreateCodedInput());
        //        RaisePropertyChanged(() => SelectedEggIncubator);
        //    }
        //    else
        //    {
        //        // Navigating from game page, so we need to actually load the encounter
        //        CurrentEgg = (PokemonDataWrapper)NavigationHelper.NavigationState[nameof(CurrentEgg)];
        //    }
        //    await Task.CompletedTask;
        //}

        ///// <summary>
        /////     Save state before navigating
        ///// </summary>
        ///// <param name="suspensionState"></param>
        ///// <param name="suspending"></param>
        ///// <returns></returns>
        //public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        //{
        //    if (suspending)
        //    {
        //        suspensionState[nameof(CurrentEgg)] = JsonConvert.SerializeObject(CurrentEgg);
        //        suspensionState[nameof(SelectedEggIncubator)] = SelectedEggIncubator.ToByteString().ToBase64();                
        //    }
        //    await Task.CompletedTask;
        //}

        //public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        //{
        //    args.Cancel = false;
        //    await Task.CompletedTask;
        //}

        //#endregion

        //#region Game Management Vars

        ///// <summary>
        /////     Pokemon that we're trying to capture
        ///// </summary>
        //private PokemonDataWrapper _currentEgg;

        ///// <summary>
        /////     Current item for capture page
        ///// </summary>
        //private EggIncubator _selectedEggIncubator;

        //#endregion

        //#region Bindable Game Vars

        ///// <summary>
        /////     Reference to global inventory
        ///// </summary>
        //public ObservableCollection<EggIncubator> IncubatorsInventory => GameClient.FreeIncubatorsInventory;

        ///// <summary>
        /////     Pokemon that we're trying to capture
        ///// </summary>
        //public PokemonDataWrapper CurrentEgg
        //{
        //    get { return _currentEgg; }
        //    set { Set(ref _currentEgg, value); }
        //}

        ///// <summary>
        /////     Current item for capture page
        ///// </summary>
        //public EggIncubator SelectedEggIncubator
        //{
        //    get { return _selectedEggIncubator; }
        //    set { Set(ref _selectedEggIncubator, value); }
        //}

        //#endregion

        //#region Game Logic

        //#region Shared Logic

        //private DelegateCommand _returnToPokemonInventoryScreen;

        ///// <summary>
        /////     Going back to inventory page
        ///// </summary>
        //public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
        //    _returnToPokemonInventoryScreen =
        //        new DelegateCommand(() => { NavigationService.Navigate(typeof(PokemonInventoryPage), true); },
        //            () => true)
        //    );

        //#endregion

        //#region Incubator

        //#region Incubator Events

        ///// <summary>
        /////     Event fired if using the incubator returned Success
        ///// </summary>
        //public event EventHandler IncubatorSuccess;

        //#endregion

        //private DelegateCommand<EggIncubator> _useIncubatorCommand;

        //public DelegateCommand<EggIncubator> UseIncubatorCommand => _useIncubatorCommand ?? (
        //    _useIncubatorCommand = new DelegateCommand<EggIncubator>(async incubator =>
        //    {
        //        var response = await GameClient.UseEggIncubator(incubator, CurrentEgg.WrappedData);
        //        switch (response.Result)
        //        {
        //            case UseItemEggIncubatorResponse.Types.Result.Success:
        //                IncubatorSuccess?.Invoke(this, null);
        //                await GameClient.UpdateInventory();
        //                CurrentEgg =
        //                    new PokemonDataWrapper(GameClient.EggsInventory.First(item => item.Id == CurrentEgg.Id));
        //                break;
        //            default:
        //                Logger.Write($"Error using {incubator.Id} on {CurrentEgg.Id}");
        //                break;
        //        }
        //    }, incubator => true));

        //#endregion

        //#endregion

        public EggDetailPageViewModel() { }

        #region Lifecycle Handlers

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
            }
            else
            {
                // Navigating from inventory page so we need to load the egginventory and the current egg
                Load(Convert.ToUInt64(parameter));
            }

            await Task.CompletedTask;
        }

        public void Load(ulong selectedEggId)
        {
            EggInventory.Clear();

            var unincubatedEggs = GameClient.EggsInventory.Where(o => string.IsNullOrEmpty(o.EggIncubatorId))
                                              .OrderBy(c => c.EggKmWalkedTarget).Select(eggData => new PokemonDataWrapper(eggData));
            var incubatedEggs = GameClient.EggsInventory.Where(o => !string.IsNullOrEmpty(o.EggIncubatorId))
                                                          .OrderBy(c => c.EggKmWalkedTarget).Select(eggData => new IncubatedEggDataWrapper(GameClient.UsedIncubatorsInventory.FirstOrDefault(incubator => incubator.Id == eggData.EggIncubatorId), GameClient.PlayerStats.KmWalked, eggData));

            foreach(PokemonDataWrapper egg in incubatedEggs)
            {
                EggInventory.Add(egg);
            }
            foreach (PokemonDataWrapper egg in unincubatedEggs)
            {
                EggInventory.Add(egg);
            }

            SelectedEgg = EggInventory.FirstOrDefault(egg => egg.Id == selectedEggId);
        }


        #endregion

        #region Bindable Vars

        public ObservableCollection<PokemonDataWrapper> EggInventory { get; private set; } = new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        /// Flag for an ongoing server request. Used to disable the controls
        /// </summary>
        private bool _serverRequestRunning;
        public bool ServerRequestRunning
        {
            get { return _serverRequestRunning; }
            set
            {
                Set(ref _serverRequestRunning, value);
                //FavoritePokemonCommand.RaiseCanExecuteChanged();
                //PowerUpPokemonCommand.RaiseCanExecuteChanged();
                //EvolvePokemonCommand.RaiseCanExecuteChanged();
                //TransferPokemonCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Current displayed Pokemon
        /// </summary>
        private PokemonDataWrapper _selectedEgg;
        public PokemonDataWrapper SelectedEgg
        {
            get { return _selectedEgg; }
            set { Set(ref _selectedEgg, value); }
        }

        #endregion

        #region Game Logic

        #region Shared Logic


        private DelegateCommand _returnToPokemonInventoryScreen;

        /// <summary>
        /// Going back to inventory page
        /// </summary>
        public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
            _returnToPokemonInventoryScreen = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
            }, () => true)
            );

        #endregion

        #endregion
    }
}