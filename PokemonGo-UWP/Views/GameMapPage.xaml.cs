using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameMapPage : Page
    {
        public GameMapPage()
        {
            InitializeComponent();
        }

        #region Menu Animation

        private bool _isMenuOpen;
        private bool _isNearbyOpen;

        private void PokeMenuMainButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_isMenuOpen)
                ShowPokeMenuStoryboard.Begin();
            else
                HidePokeMenuStoryboard.Begin();
            _isMenuOpen = !_isMenuOpen;
        }

        private void NearbyPokemonGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_isNearbyOpen)
                ShowNearbyGridStoryboard.Begin();
            else
                HideNearbyGridStoryboard.Begin();
            _isNearbyOpen = !_isNearbyOpen;
        }

        #endregion
    }
}