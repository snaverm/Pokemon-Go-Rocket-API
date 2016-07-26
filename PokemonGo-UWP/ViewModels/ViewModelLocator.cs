namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    ///     Locator used to provide singleton viewmodels.
    /// </summary>
    public class ViewModelLocator
    {
        private GameManagerViewModel _gameManagerViewModel;

        public GameManagerViewModel GameManagerViewModel =>
            _gameManagerViewModel ?? (_gameManagerViewModel = new GameManagerViewModel());
    }
}