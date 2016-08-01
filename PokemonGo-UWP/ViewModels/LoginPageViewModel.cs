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

namespace PokemonGo_UWP.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {

        #region Game Management Vars

        private string _ptcUsername;

        private string _ptcPassword;

        #endregion

        #region Bindable Game Vars

        public string CurrentVersion => GameClient.CurrentVersion;

        public string PtcUsername
        {
            get { return _ptcUsername; }
            set
            {
                Set(ref _ptcUsername, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string PtcPassword
        {
            get { return _ptcPassword; }
            set
            {
                Set(ref _ptcPassword, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Game Logic        

        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Logging in...");
                try
                {
                    if (!await GameClient.DoPtcLogin(PtcUsername, PtcPassword))
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
            }, () => !string.IsNullOrEmpty(PtcUsername) && !string.IsNullOrEmpty(PtcPassword))
            );

        #endregion
        
    }
}
