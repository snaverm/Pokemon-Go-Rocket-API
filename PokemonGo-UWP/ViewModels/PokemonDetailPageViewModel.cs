using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Google.Protobuf;
using PokemonGo_UWP.Controls;
using PokemonGo_UWP.Views;
using PokemonGo_UWP.Utils.Extensions;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonDetailPageViewModel : ViewModelBase
    {

        public PokemonDetailPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {

            }
        }


        public List<PokemonDataWrapper> PokemonInventory { get; private set; } = new List<PokemonDataWrapper>();

        private PokemonDataWrapper _selectedPokemon;
        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public PokemonDataWrapper SelectedPokemon
        {
            get { return _selectedPokemon; }
            set
            {
                Set(ref _selectedPokemon, value);
                //RaisePropertyChanged(() => EvolvedPokemonId);
            }
        }

        private int _stardustAmount;
        public int StardustAmount
        {
            get { return _stardustAmount; }
            set { Set(ref _stardustAmount, value); }
        }

        #region Lifecycle Handlers

        public void Load(ulong selectedPokemonId, PokemonSortingModes sortingMode)
        {
            // Navigating from inventory page so we need to load the pokemoninventory and the current pokemon
            PokemonInventory.Clear();
            PokemonInventory.AddRange(GameClient.PokemonsInventory.Select(pokemonData => new PokemonDataWrapper(pokemonData)).SortBySortingmode(sortingMode));

            SelectedPokemon = PokemonInventory.FirstOrDefault(pokemon => pokemon.Id == selectedPokemonId);

            StardustAmount = GameClient.PlayerProfile.Currencies.FirstOrDefault(item => item.Name.Equals("STARDUST")).Amount;
        }

        #endregion



        #region Bindable Game Vars

        


        #endregion

        #region Game Logic

        #region Shared Logic


        private DelegateCommand _returnToPokemonInventoryScreen;

        /// <summary>
        ///     Going back to inventory page
        /// </summary>
        public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
            _returnToPokemonInventoryScreen = new DelegateCommand(() =>
            {
                // HACK - if we're coming fro the inventory we may go back, otherwise we go to map page
                if (NavigationService.Frame.BackStack.Last().SourcePageType == typeof(PokemonInventoryPage))
                    NavigationService.GoBack();
                else
                    NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.PokemonUpdate);
            }, () => true)
            );

        #endregion

        #region Appraise

        private DelegateCommand _appraisePokemonCommand;

        public DelegateCommand AppraisePokemonCommand => _appraisePokemonCommand ?? (
          _appraisePokemonCommand = new DelegateCommand(() =>
          {
              // TODO: Implement appraise
              var dialog = new MessageDialog("Sorry, check back later ;)", "Not yet implemented");
              dialog.ShowAsync();
          }, () => true));

        #endregion

        #region Transfer

        private DelegateCommand _transferPokemonCommand;

        public DelegateCommand TransferPokemonCommand => _transferPokemonCommand ?? (
          _transferPokemonCommand = new DelegateCommand(() =>
          {
              // Ask for confirmation before moving the Pokemon
              var name = Resources.Pokemon.GetString(SelectedPokemon.PokemonId.ToString());
              var dialog =
                  new PoGoMessageDialog(
                      string.Format(Resources.CodeResources.GetString("TransferPokemonWarningTitle"), name),
                      Resources.CodeResources.GetString("TransferPokemonWarningText"))
                  {
                      AcceptText = Resources.CodeResources.GetString("YesText"),
                      CancelText = Resources.CodeResources.GetString("NoText"),
                      CoverBackground = true,
                      AnimationType = PoGoMessageDialogAnimation.Bottom
                  };

              dialog.AcceptInvoked += async (sender, e) =>
              {
                  // User confirmed transfer
                  try
                  {
                      Busy.SetBusy(true);
                      var pokemonTransferResponse = await GameClient.TransferPokemon(SelectedPokemon.Id);

                      switch (pokemonTransferResponse.Result)
                      {
                          case ReleasePokemonResponse.Types.Result.Unset:
                              break;
                          case ReleasePokemonResponse.Types.Result.Success:
                              // This isn't a MessageDialog in the original. Need to implement the additional UI
                              await GameClient.UpdateInventory();
                              await GameClient.UpdatePlayerStats();
                              // HACK - if we're coming fro the inventory we may go back, otherwise we go to map page
                              if (NavigationService.Frame.BackStack.Last().SourcePageType ==
                                  typeof(PokemonInventoryPage))
                                  NavigationService.GoBack();
                              else
                                  NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.PokemonUpdate);
                              break;

                          case ReleasePokemonResponse.Types.Result.PokemonDeployed:
                              break;
                          case ReleasePokemonResponse.Types.Result.Failed:
                              break;
                          case ReleasePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                              break;
                          default:
                              throw new ArgumentOutOfRangeException();
                      }
                  }
                  finally
                  {
                      Busy.SetBusy(false);
                  }

              };

              dialog.Show();

          }, () => true));

        #endregion

        #region Favorite

        private DelegateCommand _favoritePokemonCommand;

        public DelegateCommand FavoritePokemonCommand => _favoritePokemonCommand ?? (
          _favoritePokemonCommand = new DelegateCommand(async () =>
          {
              try
              {
                  Busy.SetBusy(true);
                  var isFavorite = Convert.ToBoolean(SelectedPokemon.Favorite);
                  var pokemonFavoriteResponse = await GameClient.SetFavoritePokemon(SelectedPokemon.Id, !isFavorite);
                  switch (pokemonFavoriteResponse.Result)
                  {
                      case SetFavoritePokemonResponse.Types.Result.Unset:
                          break;
                      case SetFavoritePokemonResponse.Types.Result.Success:
                          // Inverse favorite state
                          SelectedPokemon.WrappedData.Favorite = Convert.ToInt32(!isFavorite);

                          //IsFavorite = !isFavorite;
                          break;

                      case SetFavoritePokemonResponse.Types.Result.ErrorPokemonNotFound:
                          break;
                      case SetFavoritePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                          break;
                      default:
                          throw new ArgumentOutOfRangeException();
                  }
              }
              finally
              {
                  Busy.SetBusy(false);
              }
          }, () => true));

        #endregion

        #region Rename

        private DelegateCommand _renamePokemonCommand;

        public DelegateCommand RenamePokemonCommand => _renamePokemonCommand ?? (
          _renamePokemonCommand = new DelegateCommand(() =>
          {
              var textbox = new TextboxMessageDialog(SelectedPokemon.Name, 12);
              var dialog = new PoGoMessageDialog("", Resources.CodeResources.GetString("SetNickName"))
              {
                  DialogContent = textbox,
                  AcceptText = Resources.CodeResources.GetString("OkText"),
                  CancelText = Resources.CodeResources.GetString("CancelText"),
                  CoverBackground = true,
                  BackgroundTapInvokesCancel = true,
                  AnimationType = PoGoMessageDialogAnimation.Bottom
              };

              dialog.AppearCompleted += (sender, e) =>
              {
                  textbox.SelectAllOnTextBoxFocus = true;
                  textbox.FocusTextbox(FocusState.Programmatic);
              };
              dialog.AcceptInvoked += async (sender, e) =>
              {

                  try
                  {
                      Busy.SetBusy(true);
                      // Send rename request
                      var res = await GameClient.SetPokemonNickName((ulong)SelectedPokemon.Id, textbox.Text);
                      switch (res.Result)
                      {
                          case NicknamePokemonResponse.Types.Result.Unset:
                              break;
                          case NicknamePokemonResponse.Types.Result.Success:
                              // Reload updated data
                              var currentPokemonData = SelectedPokemon.WrappedData;
                              currentPokemonData.Nickname = textbox.Text;
                              SelectedPokemon = new PokemonDataWrapper(currentPokemonData);
                              await GameClient.UpdateInventory();
                              await GameClient.UpdateProfile();
                              //UpdateCurrentData();
                              break;
                          case NicknamePokemonResponse.Types.Result.ErrorPokemonNotFound:
                              break;
                          case NicknamePokemonResponse.Types.Result.ErrorInvalidNickname:
                              break;
                          case NicknamePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                              break;
                          default:
                              throw new ArgumentOutOfRangeException();
                      }
                  }
                  finally
                  {
                      Busy.SetBusy(false);
                  }
              };

              dialog.Show();

          }, () => true));

        #endregion

        #region Power Up

        private DelegateCommand _powerUpPokemonCommand;

        public DelegateCommand PowerUpPokemonCommand => _powerUpPokemonCommand ?? (_powerUpPokemonCommand = new DelegateCommand(() =>
        {
            // Ask for confirmation before powering up the Pokemon
            var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("PowerUpPokemonWarningText"),Resources.Pokemon.GetString(SelectedPokemon.PokemonId.ToString())));
            dialog.AcceptText = Resources.CodeResources.GetString("YesText");
            dialog.CancelText = Resources.CodeResources.GetString("NoText");
            dialog.CoverBackground = true;
            dialog.AnimationType = PoGoMessageDialogAnimation.Bottom;
            dialog.AcceptInvoked += async (sender, e) =>
            {
                // Send power up request
                var res = await GameClient.PowerUpPokemon(SelectedPokemon.WrappedData);
                switch (res.Result)
                {
                    case UpgradePokemonResponse.Types.Result.Unset:
                        break;
                    case UpgradePokemonResponse.Types.Result.Success:
                        // Reload updated data
                        SelectedPokemon = new PokemonDataWrapper(res.UpgradedPokemon);
                        await GameClient.UpdateInventory();
                        await GameClient.UpdateProfile();
                        //UpdateCurrentData();
                        break;

                    case UpgradePokemonResponse.Types.Result.ErrorPokemonNotFound:
                        break;
                    case UpgradePokemonResponse.Types.Result.ErrorInsufficientResources:
                        break;
                    case UpgradePokemonResponse.Types.Result.ErrorUpgradeNotAvailable:
                        break;
                    case UpgradePokemonResponse.Types.Result.ErrorPokemonIsDeployed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            dialog.Show();
        }, CanPowerUp));

        private bool CanPowerUp()
        {
            //if (CurrentPokemon == null) return false;
            //var pokemonLevel = PokemonInfo.GetLevel(CurrentPokemon.WrappedData);
            //return CurrentCandy != null && StardustAmount >= StardustToPowerUp && CurrentCandy.Candy_ >= CandiesToPowerUp && pokemonLevel < GameClient.PlayerStats.Level + 1.5;
            return true;
        }

        #endregion

        #region Evolve

        #region Evolve Events

        public event EventHandler PokemonEvolved;

        #endregion

        private DelegateCommand _evolvePokemonCommand;

        public DelegateCommand EvolvePokemonCommand => _evolvePokemonCommand ?? (_evolvePokemonCommand = new DelegateCommand(() =>
        {

        }, () => true));
        //public DelegateCommand EvolvePokemonCommand => _evolvePokemonCommand ?? (_evolvePokemonCommand = new DelegateCommand(() =>
        //{
        //    // Ask for confirmation before evolving the Pokemon
        //    var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("EvolvePokemonWarningText"),
        //        Resources.Pokemon.GetString(CurrentPokemon.PokemonId.ToString())));
        //    dialog.AcceptText = Resources.CodeResources.GetString("YesText");
        //    dialog.CancelText = Resources.CodeResources.GetString("NoText");
        //    dialog.CoverBackground = true;
        //    dialog.AnimationType = PoGoMessageDialogAnimation.Bottom;
        //    dialog.AcceptInvoked += async (sender, e) =>
        //    {
        //        EvolvePokemonResponse = await GameClient.EvolvePokemon(CurrentPokemon.WrappedData);
        //        RaisePropertyChanged(() => EvolvedPokemonId);
        //        switch (EvolvePokemonResponse.Result)
        //        {
        //            case EvolvePokemonResponse.Types.Result.Unset:
        //                break;
        //            case EvolvePokemonResponse.Types.Result.Success:
        //                PokemonEvolved?.Invoke(this, null);
        //                await GameClient.UpdateInventory();
        //                await GameClient.UpdateProfile();
        //                break;

        //            case EvolvePokemonResponse.Types.Result.FailedPokemonMissing:
        //                break;
        //            case EvolvePokemonResponse.Types.Result.FailedInsufficientResources:
        //                break;
        //            case EvolvePokemonResponse.Types.Result.FailedPokemonCannotEvolve:
        //                break;
        //            case EvolvePokemonResponse.Types.Result.FailedPokemonIsDeployed:
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }
        //    };

        //    dialog.Show();
        //}, () => CurrentCandy != null && CurrentCandy.Candy_ >= PokemonExtraData.CandyToEvolve));

        private DelegateCommand _replaceEvolvedPokemonCommand;

        public DelegateCommand ReplaceEvolvedPokemonCommand => _replaceEvolvedPokemonCommand ?? (
            _replaceEvolvedPokemonCommand = new DelegateCommand(() =>
        {
            //CurrentPokemon = new PokemonDataWrapper(EvolvePokemonResponse.EvolvedPokemonData);
            //UpdateCurrentData();
        }));

        #endregion

        #endregion
    }
}
