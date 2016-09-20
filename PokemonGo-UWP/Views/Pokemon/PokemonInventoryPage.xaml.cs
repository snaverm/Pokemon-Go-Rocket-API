using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using Template10.Services.NavigationService;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokemonInventoryPage : Page
	{
		public PokemonInventoryPage()
		{
			InitializeComponent();

			NavigationCacheMode = NavigationCacheMode.Enabled;

			// TODO: fix header
			// Setup incubators translation
			//Loaded += (s, e) =>
			//{
			//	ShowIncubatorsModalAnimation.From =
			//						HideIncubatorsModalAnimation.To = IncubatorsModal.ActualHeight;
			//	HideIncubatorsModalStoryboard.Completed += (ss, ee) => { IncubatorsModal.IsModal = false; };
			//};

		    //ViewModel.ResetView +=
		    //    () =>
		    //    {
		    //        if (ViewModel.PokemonInventory != null && ViewModel.PokemonInventory.Count > 0)
		    //            PokemonInventoryGridView?.ScrollIntoView(ViewModel.PokemonInventory[ViewModel.LastVisibleIndex]);
		    //    };
		}

        //      private void ToggleIncubatorModel(object sender, TappedRoutedEventArgs e)
        //{
        //	if (IncubatorsModal.IsModal)
        //	{
        //		HideIncubatorsModalStoryboard.Begin();
        //	}
        //	else
        //	{
        //		IncubatorsModal.IsModal = true;
        //		ShowIncubatorsModalStoryboard.Begin();
        //	}
        //}

        #region  Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.ScrollPokemonToVisibleRequired += ScrollPokemonToVisible;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ViewModel.ScrollPokemonToVisibleRequired -= ScrollPokemonToVisible;
        }

        #endregion

        private void ScrollPokemonToVisible(PokemonDataWrapper p)
        {
            PokemonInventory.PokemonInventoryGridView.ScrollIntoView(p);
        }

    }
}