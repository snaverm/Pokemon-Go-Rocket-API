using Newtonsoft.Json;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Utils.Helpers;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory.Item;
using Template10.Common;
using Template10.Mvvm;
using System.ComponentModel;

namespace PokemonGo_UWP.Entities
{
    public class ItemDataWrapper : INotifyPropertyChanged
    {
        private DelegateCommand _gotoDiscardCommand;
        private ItemData _wrappedData;

        public ItemDataWrapper(ItemData itemData)
        {
            _wrappedData = itemData;
        }

        [JsonProperty, JsonConverter(typeof(ProtobufJsonNetConverter))]
        public ItemData WrappedData { get; }

        /// <summary>
        ///     Navigate to detail page for the selected egg
        /// </summary>
        public DelegateCommand GotoDiscardCommand => _gotoDiscardCommand ?? (
            _gotoDiscardCommand = new DelegateCommand(() =>
            {
                //NavigationHelper.NavigationState["CurrentEgg"] = this;
                //BootStrapper.Current.NavigationService.Navigate(typeof(EggDetailPage), true);
            }, () => true));

        public void Update(ItemData update)
        {
            _wrappedData = update;

            OnPropertyChanged(nameof(ItemId));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Unseen));
        }

        #region Wrapped Properties

        public ItemData WrappedData => _wrappedData;

        public ItemId ItemId => _wrappedData.ItemId;

        public int Count => _wrappedData.Count;

        public bool Unseen => _wrappedData.Unseen;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}