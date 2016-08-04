using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Template10.Utils;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonInventoryPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                PokemonInventory = (ObservableCollection<PokemonData>) suspensionState[nameof(PokemonInventory)];
            }
            else if (parameter is bool)
            {
                // Navigating from game page, so we need to actually load the inventory                
                PokemonInventory.AddRange(GameClient.PokemonsInventory);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(PokemonInventory)] = PokemonInventory;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Bindable Game Vars

        /// <summary>
        /// Reference to Pokemon inventory
        /// </summary>
        public ObservableCollection<PokemonData> PokemonInventory {get; private set;} = new ObservableCollection<PokemonData>();

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (
            _returnToGameScreen = new DelegateCommand(() =>
            {                
                NavigationService.Navigate(typeof(GameMapPage));
            }, () => true)
            );

        #endregion

        #region Pokemon Inventory Handling

        private DelegateCommand _sortByCpCommand;

        public DelegateCommand SortByCpCommand => _sortByCpCommand ?? (
            _sortByCpCommand = new DelegateCommand(() =>
            {
                PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Cp));
            }, () => true));

        private DelegateCommand _sortByNumberCommand;

        public DelegateCommand SortByNumberCommand => _sortByNumberCommand ?? (
            _sortByNumberCommand = new DelegateCommand(() =>
            {
                PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderBy(pokemon => pokemon.PokemonId));
            }, () => true));

        private DelegateCommand _sortByNameCommand;

        public DelegateCommand SortByNameCommand => _sortByNameCommand ?? (
            _sortByNameCommand = new DelegateCommand(() =>
            {
                PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderBy(pokemon => pokemon.PokemonId.ToString()));
            }, () => true));

        private DelegateCommand _sortByDateCommand;

        public DelegateCommand SortByDateCommand => _sortByDateCommand ?? (
            _sortByDateCommand = new DelegateCommand(() =>
            {
                PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.CreationTimeMs));
            }, () => true));

        #endregion

        #endregion

    }
}
