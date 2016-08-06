using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokemonInventoryPage : Page
    {
        public PokemonInventoryPage()
        {
            this.InitializeComponent();
            // TODO: fix header
            // Setup incubators translation
            Loaded += (s, e) =>
            {                
                ShowIncubatorsModalAnimation.From =
                    HideIncubatorsModalAnimation.To = IncubatorsModal.ActualHeight;
                HideIncubatorsModalStoryboard.Completed += (ss, ee) =>
                {
                    IncubatorsModal.IsModal = false;
                };
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            if (!(SortMenuPanel.Opacity > 0)) return;
            backRequestedEventArgs.Handled = true;
            HideSortMenuStoryboard.Begin();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
        }

        #endregion


        private void ToggleIncubatorModel(object sender, TappedRoutedEventArgs e)
        {
            if (IncubatorsModal.IsModal)
            {
                HideIncubatorsModalStoryboard.Begin();
            }
            else
            {
                IncubatorsModal.IsModal = true;
                ShowIncubatorsModalStoryboard.Begin();
            }
        }

    }

}
