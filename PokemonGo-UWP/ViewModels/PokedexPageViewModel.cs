using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels
{
    public class PokedexPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.Any())
            {
                PokemonFoundAndSeen = (ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>)state[nameof(PokemonFoundAndSeen)];
                SeenPokemons = (int)state[nameof(SeenPokemons)];
                CapturedPokemons = (int)state[nameof(CapturedPokemons)];
                
            }
            else
            {
                var pokedexItems = GameClient.PokedexInventory;
                var lastPokemonIdSeen = pokedexItems == null || pokedexItems.Count == 0 ? 0 : pokedexItems.Max(x => (int)x.PokemonId);
                if (lastPokemonIdSeen > 0)
                {
                    var listAllPokemon = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();
                    foreach (var item in listAllPokemon)
                    {
                        if ((int)item > lastPokemonIdSeen)
                            break;
                        switch (item)
                        {
                            case PokemonId.Missingno:
                                break;
                            default:
                                var pokedexEntry = pokedexItems.Where(x => x.PokemonId == item);
                                if (pokedexEntry.Count() == 1)
                                    PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, pokedexEntry.ElementAt(0)));
                                else
                                    PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, null));
                                break;
                        }
                    }

                    foreach (PokemonId id in listAllPokemon.Where(t=>t!= PokemonId.Missingno && pokedexItems.Any(y=>y.PokemonId==t)))
                    {
                    PokedexEntries.Add(new PokedexItem(id, PokemonFoundAndSeen));
                    }

                    CapturedPokemons = pokedexItems.Where(x => x.TimesCaptured > 0).Count();
                    SeenPokemons = pokedexItems.Count;
                }
                else
                {
                    CapturedPokemons = 0;
                    SeenPokemons = 0;
                }
            }
            if(parameter!=null && parameter is PokemonId)  //utilized to open a pokedex page, passing pokemon id
            {
                
                NavigationService.Navigate(typeof(PokedexDetailPage), (PokemonId)parameter);

                //SelectedPokedexEntry = new KeyValuePair<PokemonId, PokedexEntry>((PokemonId)parameter, GetPokedexEntry((PokemonId)parameter));
                //IsPokemonDetailsOpen = true;
            }
            NavigationService.FrameFacade.BackRequested += FrameFacade_BackRequested;
            return Task.CompletedTask;
        }

        private void FrameFacade_BackRequested(object sender, Template10.Common.HandledEventArgs e)
        {
            e.Handled = true;
            CloseCommand.Execute();
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            if (suspending)
            {
                pageState[nameof(PokemonFoundAndSeen)] = PokemonFoundAndSeen;
                pageState[nameof(SeenPokemons)] = SeenPokemons;
                pageState[nameof(CapturedPokemons)] = CapturedPokemons;
                //if (IsPokemonDetailsOpen)
                //{
                //    pageState[nameof(IsPokemonDetailsOpen)] = IsPokemonDetailsOpen;
                //    pageState[nameof(SelectedPokedexEntry)] = SelectedPokedexEntry;
                //    pageState[nameof(IsEevee)] = IsEevee;
                //}
            }
            else
            {
                pageState?.Clear();
                PokemonFoundAndSeen?.Clear();
                EeveeEvolutions?.Clear();
                PokemonEvolutions?.Clear();
            }
            NavigationService.FrameFacade.BackRequested -= FrameFacade_BackRequested;
            return Task.CompletedTask;
        }
        #endregion

        #region Variables
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen { get; private set; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private int _captured, _seen;
        public int CapturedPokemons { get { return _captured; } set { Set(ref _captured, value); } }
        public int SeenPokemons { get { return _seen; } set { Set(ref _seen, value); } }
        
        private bool _pokemonDetailsOpen;
        public bool IsPokemonDetailsOpen { get { return _pokemonDetailsOpen; } set { Set(ref _pokemonDetailsOpen, value); } }
      
        private PokemonSettings _pokemonDetails;
        public PokemonSettings PokemonDetails { get { return _pokemonDetails; } set { Set(ref _pokemonDetails, value); } }
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonEvolutions { get; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> EeveeEvolutions { get; } =  new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private bool _isEevee;
        public bool IsEevee { get { return _isEevee; } set { Set(ref _isEevee, value); } }
        #endregion

        private DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> _openPokedexEntry;
        public DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> OpenPokedexEntry =>
            _openPokedexEntry ??
            (_openPokedexEntry = new DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>>(
                (x) => 
                    {
                        
                        //RaisePropertyChanged(nameof(SelectedPokedexEntry));
                        //IsPokemonDetailsOpen = true;
                        Debug.WriteLine("Navigating!");
                        NavigationService.Navigate(typeof(PokedexDetailPage), x.Key);
                        Debug.WriteLine("Navigated!");

                    }, 
                    (x) => { return true; }
                )
            );
        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
            =>
            _closeCommand ??
            (_closeCommand = new DelegateCommand(() =>
            {
                if (IsPokemonDetailsOpen)
                    IsPokemonDetailsOpen = false;
                else
                    NavigationService.Navigate(typeof(GameMapPage));
            }, () => true)
            );
       

        public ObservableCollection<PokedexItem> PokedexEntries { get; set; } = new ObservableCollection<PokedexItem>();


        public class PokedexItem : ViewModelBase
        {
            public PokemonId pokemonId { get; set; }
            public PokemonSettings PokemonDetails { get; set; }
            public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonEvolutions { get; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
            public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> EeveeEvolutions { get; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();

            private bool _isEevee;
            public bool IsEevee { get { return _isEevee; } set { Set(ref _isEevee, value); } }

            private void PopulateEvolutions(ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen)
            {
                PokemonEvolutions.Clear();
                EeveeEvolutions.Clear();
                IsEevee = false;
                PokemonDetails = GameClient.GetExtraDataForPokemon(pokemonId);
                PokemonSettings CurrPokemon = PokemonDetails;
                switch (pokemonId)
                {
                    case PokemonId.Eevee:
                    case PokemonId.Jolteon:
                    case PokemonId.Flareon:
                    case PokemonId.Vaporeon:
                        pokemonId = PokemonId.Eevee;
                        CurrPokemon = GameClient.GetExtraDataForPokemon(pokemonId);
                        foreach (var ev in CurrPokemon.EvolutionIds)
                        {
                            EeveeEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev,
                                ev.GetPokedexEntry(PokemonFoundAndSeen)));
                        }
                        PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(PokemonId.Eevee, PokemonId.Eevee.GetPokedexEntry(PokemonFoundAndSeen)));
                        IsEevee = true;
                        break;

                    default:
                        PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(pokemonId, pokemonId.GetPokedexEntry(PokemonFoundAndSeen)));
                        while (CurrPokemon.ParentPokemonId != PokemonId.Missingno)
                        {
                            PokemonEvolutions.Insert(0, new KeyValuePair<PokemonId, PokedexEntry>(CurrPokemon.ParentPokemonId, CurrPokemon.ParentPokemonId.GetPokedexEntry(PokemonFoundAndSeen)));
                            CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.ParentPokemonId);
                        }
                        CurrPokemon = PokemonDetails;
                        while (CurrPokemon.EvolutionIds.Count > 0)
                        {
                            foreach (var ev in CurrPokemon.EvolutionIds) //for Eevee
                                PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev, ev.GetPokedexEntry(PokemonFoundAndSeen)));
                            CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.EvolutionIds.ElementAt(0));
                        }
                        break;
                }
            }

            public PokedexItem(PokemonId id,ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen)
            {
                this.pokemonId = id;
                this.PopulateEvolutions(PokemonFoundAndSeen);

            }
        }
    }

    public static class PokedexExtensions
    {
        public static PokedexEntry GetPokedexEntry(this PokemonId pokemon, ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen)
        {
            var found = PokemonFoundAndSeen.Where(x => x.Key == pokemon);
            if (found != null && found.Count() > 0)
                return found.ElementAt(0).Value;
            return null;
        }
    }
}
