using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;

namespace PokemonGo_UWP.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
	{
		#region Game Management Vars

		private bool _isVibrationActivated;

		#endregion
		#region Bindable Game Vars

		public string CurrentVersion => GameClient.CurrentVersion;        
		
		/// <summary>
		/// Whether the player wants vibration (when a Pokémon is nearby)
		/// </summary>
		public bool IsVibrationActivated
		{
			get { return _isVibrationActivated; }
			set { Set(ref _isVibrationActivated, value); }
		}

		#endregion

		#region Game Logic

		#region Logout

		private DelegateCommand _doPtcLogoutCommand;

		public DelegateCommand DoPtcLogoutCommand => _doPtcLogoutCommand ?? (
			_doPtcLogoutCommand = new DelegateCommand(() =>
			{
				// Clear stored token
				GameClient.DoLogout();
				// Navigate to login page
				NavigationService.Navigate(typeof(MainPage));
			}, () => true)
			);


		#endregion

		#region Close

		private DelegateCommand _closeCommand;

		public DelegateCommand CloseCommand => _closeCommand ?? (
			_closeCommand = new DelegateCommand(() =>
			{
				// Navigate back
				NavigationService.GoBack();
			}, () => true)
			);


		#endregion

		#endregion

	}
}
