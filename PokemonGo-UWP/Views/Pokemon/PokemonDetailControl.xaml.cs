using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PokemonGo_UWP.Views
{
    public sealed partial class PokemonDetailControl : UserControl
    {
        public PokemonDetailControl()
        {
            this.InitializeComponent();

            Loaded += PokemonDetailControl_Loaded;

            // Design time data
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //var pokeData = new PokemonData
                //{
                //    PokemonId = PokemonId.Abra,
                //    Cp = 10,
                //    Stamina = 800,
                //    StaminaMax = 1000,
                //    WeightKg = 12,
                //    BattlesAttacked = 5

                //};
                //CurrentPokemon = new PokemonDataWrapper(pokeData);
                //StardustAmount = 18000;
                //StardustToPowerUp = 1800;
                //CandiesToPowerUp = 100;
                //CurrentCandy = new Candy
                //{
                //    FamilyId = PokemonFamilyId.FamilyAbra,
                //    Candy_ = 10
                //};
            }

            PokemonTypeCol.MinWidth = PokemonTypeCol.ActualWidth;
            PokemonTypeCol.Width = new GridLength(1, GridUnitType.Star);
        }

        /// <summary>
        /// Hack to prevent flickering of the FlipView... it's just sad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PokemonDetailControl_Loaded(object sender, RoutedEventArgs e)
        {

            //TimeSpan delay = TimeSpan.FromMilliseconds(50);

            //ThreadPoolTimer DelayThread = ThreadPoolTimer.CreateTimer(
            //(source1) =>
            //{
            //    Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            //    {
            //        PokemonDetailControl pkmnContrl = (PokemonDetailControl)sender;
            //        pkmnContrl.Visibility = Visibility.Visible;
            //    });
            //}, delay);
        }

        #region Dependency Propertys

        public static readonly DependencyProperty FavoritePokemonCommandProperty = 
            DependencyProperty.Register(nameof(FavoritePokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RenamePokemonCommandProperty =
            DependencyProperty.Register(nameof(RenamePokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PowerUpPokemonCommandProperty =
            DependencyProperty.Register(nameof(PowerUpPokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EvolvePokemonCommandProperty =
            DependencyProperty.Register(nameof(EvolvePokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty StardustAmountProperty =
            DependencyProperty.Register(nameof(StardustAmount), typeof(int), typeof(PokemonDetailControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty PokemonExtraDataProperty =
            DependencyProperty.Register(nameof(PokemonExtraData), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public DelegateCommand FavoritePokemonCommand
        {
            get { return (DelegateCommand)GetValue(FavoritePokemonCommandProperty); }
            set { SetValue(FavoritePokemonCommandProperty, value); }
        }

        public DelegateCommand RenamePokemonCommand
        {
            get { return (DelegateCommand)GetValue(RenamePokemonCommandProperty); }
            set { SetValue(RenamePokemonCommandProperty, value); }
        }

        public DelegateCommand PowerUpPokemonCommand
        {
            get { return (DelegateCommand)GetValue(PowerUpPokemonCommandProperty); }
            set { SetValue(PowerUpPokemonCommandProperty, value); }
        }

        public DelegateCommand EvolvePokemonCommand
        {
            get { return (DelegateCommand)GetValue(EvolvePokemonCommandProperty); }
            set { SetValue(EvolvePokemonCommandProperty, value); }
        }

        public int StardustAmount
        {
            get { return (int)GetValue(StardustAmountProperty); }
            set { SetValue(StardustAmountProperty, value); }
        }

        public PokemonSettings PokemonExtraData
        {
            get { return (PokemonSettings)GetValue(PokemonExtraDataProperty); }
            set { SetValue(PokemonExtraDataProperty, value); }
        }

        #endregion
    }
}
