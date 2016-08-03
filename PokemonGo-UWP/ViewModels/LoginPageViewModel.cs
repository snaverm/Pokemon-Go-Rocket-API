using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;
using Universal_Authenticator_v2.Views;
using System.Collections.ObjectModel;

namespace PokemonGo_UWP.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {

        #region Game Management Vars

        private string _username;

        private string _password;

        #endregion

        #region Bindable Game Vars

        public string CurrentVersion => GameClient.CurrentVersion;

        public string Username
        {
            get { return _username; }
            set
            {
                Set(ref _username, value);
                DoLoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                Set(ref _password, value);
                DoLoginCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> LoginTypes => new ObservableCollection<string>
        {
            "PTC",
            "Google"
        };

        public string _selectedLoginType;
        
        public string SelectedLoginType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_selectedLoginType))
                {
                    SelectedLoginType = LoginTypes.First();
                }

                return _selectedLoginType;
            }
            set
            {
                Set(ref _selectedLoginType, value);
                DoLoginCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Game Logic        

        private DelegateCommand _doLoginCommand;

        public DelegateCommand DoLoginCommand => _doLoginCommand ?? (
            _doLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Logging in...");
                try
                {
                    var loginSuccess = false;

                    if (SelectedLoginType == "Google")
                    {
                        loginSuccess = await GameClient.DoGoogleLogin(Username, Password);
                    }
                    else
                    {
                        loginSuccess = await GameClient.DoPtcLogin(Username, Password);
                    }

                    if (!loginSuccess)
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog("Wrong username/password or offline server, please try again.").ShowAsyncQueue();
                    }
                    else
                    {
                        // Goto game page
                        await NavigationService.NavigateAsync(typeof(GameMapPage), true);
                    }
                }
                catch (Exception)
                {
                    await new MessageDialog("PTC login is probably down, please retry later.").ShowAsyncQueue();
                }
                finally
                {
                    Busy.SetBusy(false);
                }
            }, () => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            );

        #endregion
        
    }
}
