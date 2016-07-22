using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo_UWP.Views;
using Template10.Mvvm;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    /// Main class for the game.
    /// This handles connection to client and UI updating via binding.
    /// </summary>
    public class GameManagerViewModel : ViewModelBase
    {

        #region Logic

        private readonly Client _client;
        private readonly ISettings _clientSettings;        
        #endregion

        public GameManagerViewModel()
        {
            _clientSettings = new Settings();
            _client = new Client(_clientSettings);
        }

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
            set
            {
                Set(ref _isLoggedIn, value);
            }
        }

        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {                
                Busy.SetBusy(true, "Logging in...");                
                IsLoggedIn = await _client.DoPtcLogin(PtcUsername, PtcPassword);
                Busy.SetBusy(false);
                if (!IsLoggedIn)
                {
                    // Login failed, show a message
                    await new MessageDialog("Wrong username/password or offline server, please try again.").ShowAsync();
                }
                else
                {
                    // Login worked, let's go to game page
                    await NavigationService.NavigateAsync(typeof(GameMapPage));
                    // Avoid going back to login page using back button
                    NavigationService.ClearHistory();
                }
            }, () => !string.IsNullOrEmpty(PtcUsername) && !string.IsNullOrEmpty(PtcPassword))
            );

        #endregion

    }
}
