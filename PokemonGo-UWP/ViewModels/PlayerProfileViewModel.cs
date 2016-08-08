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
using POGOProtos.Data.Player;
using POGOProtos.Inventory;
using System;
using POGOProtos.Enums;

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
                RaisePropertyChanged(nameof(CurrentLevelXP));
                RaisePropertyChanged(nameof(TotalLevelXP));
                RaisePropertyChanged(nameof(Pokecoins));
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
        public ObservableCollection<Achievement> PlayerStatsValues { get; } = new ObservableCollection<Achievement>();

        public int ExperienceValue => _playerStats == null ? 0 : (int)(((double)_playerStats.Experience - _playerStats.PrevLevelXp) / (_playerStats.NextLevelXp - _playerStats.PrevLevelXp) * 100);

        public long CurrentLevelXP => PlayerStats != null ? PlayerStats.Experience - PlayerStats.PrevLevelXp : 0;

        public long TotalLevelXP => PlayerStats != null ? PlayerStats.NextLevelXp - PlayerStats.PrevLevelXp : 0;
        
        public long Pokecoins => PlayerStats != null ? PlayerProfile.Currencies.First(item => item.Name.Equals("POKECOIN")).Amount : 0;

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (_returnToGameScreen = new DelegateCommand(() => { NavigationService.Navigate(typeof(GameMapPage)); }, () => true));

        #endregion

        #region Read Player Stats Values

        private void ReadPlayerStatsValues() {
            Achievement.Jogger.Value = PlayerStats.KmWalked;
            Achievement.Kanto.Value = PlayerStats.UniquePokedexEntries;
            Achievement.Collector.Value = PlayerStats.PokemonsCaptured;
            Achievement.Scientist.Value = PlayerStats.Evolutions;
            Achievement.Breeder.Value = PlayerStats.EggsHatched;
            Achievement.Backpacker.Value = PlayerStats.PokeStopVisits;
            Achievement.SchoolKid.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.None];
            Achievement.Fisherman.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Normal];
            Achievement.BlackBelt.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Fighting];
            Achievement.BirdKeeper.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Flying];
            Achievement.PunkGirl.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Poison];
            Achievement.RuinManiac.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Ground];
            Achievement.Hiker.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Rock];
            Achievement.BugCatcher.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Bug];
            Achievement.HexManiac.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Ghost];
            Achievement.DepotAgent.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Steel];
            Achievement.Kindler.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Fire];
            Achievement.Swimmer.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Water];
            Achievement.Gardener.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Grass];
            Achievement.Rocker.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Electric];
            Achievement.Psychic.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Psychic];
            Achievement.Skier.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Ice];
            Achievement.DragonTamer.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Dragon];
            Achievement.FairyTaleGirl.Value = PlayerStats.PokemonCaughtByType[(int)PokemonType.Fairy];
            Achievement.Youngster.Value = PlayerStats.SmallRattataCaught;
            Achievement.Fisherman.Value = PlayerStats.BigMagikarpCaught;
            Achievement.AceTrainer.Value = PlayerStats.BattleTrainingTotal;

            PlayerStatsValues.Add(Achievement.Jogger);
            PlayerStatsValues.Add(Achievement.Kanto);
            PlayerStatsValues.Add(Achievement.Collector);
            PlayerStatsValues.Add(Achievement.Scientist);
            PlayerStatsValues.Add(Achievement.Breeder);
            PlayerStatsValues.Add(Achievement.Backpacker);
            PlayerStatsValues.Add(Achievement.SchoolKid);
            PlayerStatsValues.Add(Achievement.Fisherman);
            PlayerStatsValues.Add(Achievement.BlackBelt);
            PlayerStatsValues.Add(Achievement.BirdKeeper);
            PlayerStatsValues.Add(Achievement.PunkGirl);
            PlayerStatsValues.Add(Achievement.RuinManiac);
            PlayerStatsValues.Add(Achievement.Hiker);
            PlayerStatsValues.Add(Achievement.BugCatcher);
            PlayerStatsValues.Add(Achievement.HexManiac);
            PlayerStatsValues.Add(Achievement.DepotAgent);
            PlayerStatsValues.Add(Achievement.Kindler);
            PlayerStatsValues.Add(Achievement.Swimmer);
            PlayerStatsValues.Add(Achievement.Gardener);
            PlayerStatsValues.Add(Achievement.Rocker);
            PlayerStatsValues.Add(Achievement.Psychic);
            PlayerStatsValues.Add(Achievement.Skier);
            PlayerStatsValues.Add(Achievement.DragonTamer);
            PlayerStatsValues.Add(Achievement.FairyTaleGirl);
            PlayerStatsValues.Add(Achievement.Youngster);
            PlayerStatsValues.Add(Achievement.Fisherman);
            PlayerStatsValues.Add(Achievement.AceTrainer);


            foreach (Achievement achievement in PlayerStatsValues) {
                RaisePropertyChanged(nameof(achievement));
            }
        }

        #endregion

        #endregion
    }
}
