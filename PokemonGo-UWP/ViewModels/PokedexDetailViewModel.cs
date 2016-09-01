using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Models;
using PokemonGo_UWP.Utils;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using Template10.Mvvm;
using Template10.Utils;

namespace PokemonGo_UWP.ViewModels
{
    public class PokedexDetailViewModel : ViewModelBase
    {
        public int CapturedPokemons { get; set; } = 0;
        public int SeenPokemons { get; set; } = 0;
        public ObservableCollection<PokedexPokemonModel> PokedexItems { get; private set; } = new ObservableCollection<PokedexPokemonModel>();

        public class PokedexPokemonModel : PokemonModel
        {
            public int TimesCaptured { get; set; }
            public PokedexPokemonModel(PokemonSettings x): base(x) { }
            public PokedexPokemonModel(PokemonId x) : base(x) { }
        }


        public void Load()
        {
            var pokedexItems = GameClient.PokedexInventory;
            var lastPokemonIdSeen = pokedexItems == null || pokedexItems.Count == 0 ? 0 : pokedexItems.Max(x => (int)x.PokemonId);
            if (lastPokemonIdSeen > 0)
            {
                IEnumerable<PokemonId> listAllPokemon = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();

                List< PokemonModel > pokedexes = new List<PokemonModel>();

                foreach (PokemonId id in listAllPokemon.Where(t => t != PokemonId.Missingno && pokedexItems.Any(y => y.PokemonId == t)))
                {
                    PokemonSettings pokeman = GameClient.GetExtraDataForPokemon(id);
                    PokedexPokemonModel pokemonModel = new PokedexPokemonModel(pokeman);
                    var CurrPokemon = pokemonModel;
                    pokemonModel.Evolutions.Add(pokemonModel);
                    while (CurrPokemon.ParentPokemonId != PokemonId.Missingno)
                    {

                        pokemonModel.Evolutions.Insert(0, new PokedexPokemonModel(CurrPokemon.ParentPokemonId));
                        CurrPokemon = new PokedexPokemonModel(CurrPokemon.ParentPokemonId);
                    }
                    CurrPokemon = pokemonModel;
                    while (CurrPokemon.EvolutionIds.Count > 0)
                    {
                        foreach (var ev in CurrPokemon.EvolutionIds) //for Eevee
                            pokemonModel.Evolutions.Add(new PokedexPokemonModel(ev));
                        CurrPokemon = new PokedexPokemonModel(CurrPokemon.EvolutionIds.ElementAt(0));
                    }

                    PokedexItems.Add(pokemonModel);
                }
                
                CapturedPokemons = pokedexItems.Count(x => x.TimesCaptured > 0);
                if (pokedexItems != null) SeenPokemons = pokedexItems.Count;
            }
            else
            {
                CapturedPokemons = 0;
                SeenPokemons = 0;
            }

            Debug.WriteLine("Loaded!");
        }

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
          =>
          _closeCommand ??
          (_closeCommand = new DelegateCommand(() =>
          {
                  NavigationService.GoBack();
          }, () => true)
          );

        //private void PopulateEvolutions()
        //{
        //    PokemonEvolutions.Clear();
        //    EeveeEvolutions.Clear();
        //    IsEevee = false;
        //    PokemonId InitPokemon = SelectedPokedexEntry.Key;
        //    PokemonSettings CurrPokemon = PokemonDetails;
        //    switch (InitPokemon)
        //    {
        //        case PokemonId.Eevee:
        //        case PokemonId.Jolteon:
        //        case PokemonId.Flareon:
        //        case PokemonId.Vaporeon:
        //            InitPokemon = PokemonId.Eevee;
        //            CurrPokemon = GameClient.GetExtraDataForPokemon(InitPokemon);
        //            foreach (var ev in CurrPokemon.EvolutionIds)
        //                EeveeEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev, GetPokedexEntry(ev)));
        //            PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(PokemonId.Eevee, GetPokedexEntry(PokemonId.Eevee)));
        //            IsEevee = true;
        //            break;
        //        default:
        //            PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(InitPokemon, GetPokedexEntry(InitPokemon)));
        //            while (CurrPokemon.ParentPokemonId != PokemonId.Missingno)
        //            {
        //                PokemonEvolutions.Insert(0, new KeyValuePair<PokemonId, PokedexEntry>(CurrPokemon.ParentPokemonId, GetPokedexEntry(CurrPokemon.ParentPokemonId)));
        //                CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.ParentPokemonId);
        //            }
        //            CurrPokemon = PokemonDetails;
        //            while (CurrPokemon.EvolutionIds.Count > 0)
        //            {
        //                foreach (var ev in CurrPokemon.EvolutionIds) //for Eevee
        //                    PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev, GetPokedexEntry(ev)));
        //                CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.EvolutionIds.ElementAt(0));
        //            }
        //            break;
        //    }
        //}
    }
}
