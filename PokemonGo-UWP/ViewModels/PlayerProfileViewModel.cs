using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Template10.Utils;
using POGOProtos.Data.Player;
using POGOProtos.Inventory;

namespace PokemonGo_UWP.ViewModels
{
    public class PlayerProfileViewModel : ViewModelBase
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
            if(suspensionState.Any()) {
                // Recovering the state                
                PlayerProfile = (PlayerData)suspensionState[nameof(PlayerProfile)];
                PlayerStats = (PlayerStats)suspensionState[nameof(PlayerStats)];
            } else {
                // No saved state, get them from the client                
                PlayerProfile = (await GameClient.GetProfile()).PlayerData;
                InventoryDelta = (await GameClient.GetInventory()).InventoryDelta;
                var tmpStats = InventoryDelta.InventoryItems.First(item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;
                PlayerStats = tmpStats;
                RaisePropertyChanged(nameof(ExperienceValue));
                RaisePropertyChanged(nameof(ExperienceString));
            }
            ReadPlayerStatsValues();
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
            if(suspending) {
                suspensionState[nameof(PlayerProfile)] = PlayerProfile;
                suspensionState[nameof(PlayerStats)] = PlayerStats;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Game Management Vars

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        private PlayerData _playerProfile;

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        private PlayerStats _playerStats;

        /// <summary>
        ///     Player's inventory
        ///     TODO: do we really need it?
        /// </summary>
        private InventoryDelta _inventoryDelta;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats
        {
            get { return _playerStats; }
            set { Set(ref _playerStats, value); }
        }

        public InventoryDelta InventoryDelta
        {
            get { return _inventoryDelta; }
            set { Set(ref _inventoryDelta, value); }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> PlayerStatsValues { get; } = new ObservableCollection<KeyValuePair<string, string>>();

        public int ExperienceValue => _playerStats == null ? 0 : (int)(((double)_playerStats.Experience - _playerStats.PrevLevelXp) / (_playerStats.NextLevelXp - _playerStats.PrevLevelXp) * 100);

        public string ExperienceString => PlayerStats != null ? ExperienceToString(PlayerStats.Experience, PlayerStats.NextLevelXp) : "";

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (_returnToGameScreen = new DelegateCommand(() => { NavigationService.Navigate(typeof(GameMapPage)); }, () => true));

        #endregion

        #region Experience to String

        private static string ExperienceToString(long expirence, long nextLevel) {
            return expirence + " / " + nextLevel;
        }

        #endregion

        #region Read Player Stats Values

        private void ReadPlayerStatsValues() {
            PlayerStatsValues.Add(new KeyValuePair<string, string>("Walked", PlayerStats.KmWalked.ToString("0.00") + " " + Resources.Translation.GetString("DistanceType")));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PokemonsEncountered", PlayerStats.PokemonsEncountered.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PokedexEntries", PlayerStats.UniquePokedexEntries.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PokemonsCaptured", PlayerStats.PokemonsCaptured.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("Evolutions", PlayerStats.Evolutions.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PokestopsVisited", PlayerStats.PokeStopVisits.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PokeballsThrown", PlayerStats.PokeballsThrown.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("EggsHatched", PlayerStats.EggsHatched.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PokemonCaughtByType", PlayerStats.PokemonCaughtByType.Length.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("BattleAttackWon", PlayerStats.BattleAttackWon.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("BattleAttackTotal", PlayerStats.BattleAttackTotal.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("BattleDefendedWon", PlayerStats.BattleTrainingWon.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("BattleTrainingWon", PlayerStats.BattleTrainingWon.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("BattleTrainingTotal", PlayerStats.BattleTrainingTotal.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("PrestigeRaisedTotal", PlayerStats.PrestigeRaisedTotal.ToString()));
            PlayerStatsValues.Add(new KeyValuePair<string, string>("prestigeDroppedTotal", PlayerStats.PrestigeDroppedTotal.ToString()));
        }

        #endregion

        #endregion
    }
}
