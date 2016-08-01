using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CapturePokemonPage : Page
    {
        public CapturePokemonPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Of course binding doesn't work so we need to manually setup height for animations
                ShowInventoryDoubleAnimation.From =
                    HideInventoryDoubleAnimation.To = InventoryMenuTranslateTransform.Y = ActualHeight*3/2;
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }

        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            ViewModel.CatchSuccess += GameManagerViewModelOnCatchSuccess;
            ViewModel.CatchEscape += GameManagerViewModelOnCatchEscape;
            ViewModel.CatchMissed += GameManagerViewModelOnCatchMissed;
            // Add also handlers to enable the button once the animation is done            
            // TODO: fix names for actions in capture score and choose a proper font
            CatchSuccess.Completed += (s, e) => ShowCaptureStatsStoryboard.Begin();
            CatchEscape.Completed += (s, e) => ShowCaptureStatsStoryboard.Begin();
            CatchMissed.Completed += (s, e) => LaunchPokeballButton.IsEnabled = true;
        }

        private void UnsubscribeToCaptureEvents()
        {
            ViewModel.CatchSuccess -= GameManagerViewModelOnCatchSuccess;
            ViewModel.CatchEscape -= GameManagerViewModelOnCatchEscape;
            ViewModel.CatchMissed -= GameManagerViewModelOnCatchMissed;
        }

        private void GameManagerViewModelOnCatchMissed(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchMissed.Begin();
        }

        private void GameManagerViewModelOnCatchEscape(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchEscape.Begin();
        }

        private void GameManagerViewModelOnCatchSuccess(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchSuccess.Begin();
        }

        #endregion
    }
}