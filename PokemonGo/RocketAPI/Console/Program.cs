using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using static PokemonGo.RocketAPI.GeneratedCode.InventoryResponse.Types;
using static PokemonGo.RocketAPI.GeneratedCode.InventoryResponse.Types.PokemonProto.Types;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => Execute());
             System.Console.ReadLine();
        }

        static async void Execute()
        {
            var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);

            if (Settings.UsePTC)
            {
                await client.LoginPtc(Settings.PtcUsername, Settings.PtcPassword);
            }
            else
            {
                await client.LoginGoogle(Settings.DeviceId, Settings.Email, Settings.LongDurationToken);
            }
            var serverResponse = await client.GetServer();
            var profile = await client.GetProfile();
            var settings = await client.GetSettings();
            var mapObjects = await client.GetMapObjects();
            var inventory = await client.GetInventory();
            var pokemons = inventory.Payload[0].Bag.Items.Select(i => i.Item?.Pokemon).Where(p => p != null && p?.PokemonType != PokemonProto.Types.PokemonIds.PokemonUnset);


            await ExecuteFarmingPokestopsAndPokemons(client);
            //await ExecuteCatchAllNearbyPokemons(client);

            
        }

        private static async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokeStops = mapObjects.Payload[0].Profile.SelectMany(i => i.Fort).Where(i => i.FortType == (int)MiscEnums.FortType.CHECKPOINT && i.CooldownCompleteMs < DateTime.UtcNow.ToUnixTime());

            foreach (var pokeStop in pokeStops)
            {
                var update = await client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await client.GetFort(pokeStop.FortId, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.SearchFort(pokeStop.FortId, pokeStop.Latitude, pokeStop.Longitude);
                var bag = fortSearch.Payload[0];

                System.Console.WriteLine($"Farmed XP: {bag.XpAwarded}, Gems: { bag.GemsAwarded}, Eggs: {bag.EggPokemon} Items: {GetFriendlyItemsString(bag.Items)}");

                await ExecuteCatchAllNearbyPokemons(client);

                await Task.Delay(15000);
            }
        }

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokemons = mapObjects.Payload[0].Profile.SelectMany(i => i.MapPokemon);

            foreach (var pokemon in pokemons)
            {
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonRespone = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude);
                } 
                while(caughtPokemonResponse.Payload[0].Status == 2);

                System.Console.WriteLine(caughtPokemonResponse.Payload[0].Status == 1 ? $"We caught a {GetFriendlyPokemonName(pokemon.PokedexTypeId)}" : $"{GetFriendlyPokemonName(pokemon.PokedexTypeId)} got away..");
                await Task.Delay(5000);
            }
        }
        private static async Task TransferAllGivenPokemons(Client client, IEnumerable<PokemonProto> unwantedPokemons)
        {
            foreach (var pokemon in unwantedPokemons)
            {
                var transferPokemonResponse = await client.TransferPokemon(pokemon.Id);

                /*
                ReleasePokemonOutProto.Status {
	                UNSET = 0;
	                SUCCESS = 1;
	                POKEMON_DEPLOYED = 2;
	                FAILED = 3;
	                ERROR_POKEMON_IS_EGG = 4;
                }*/

                if (transferPokemonResponse.Status == 1)
                {
                    System.Console.WriteLine($"Shoved another {pokemon.PokemonType} down the meat grinder");
                }
                else
                {
                    var status = transferPokemonResponse.Status;

                    System.Console.WriteLine($"Somehow failed to grind {pokemon.PokemonType}. " +
                                             $"ReleasePokemonOutProto.Status was {status}");
                }

                await Task.Delay(3000);
            }
        }

        private static async Task EvolveAllGivenPokemons(Client client, IEnumerable<PokemonProto> pokemonToEvolve)
        {
            foreach (var pokemon in pokemonToEvolve)
            {
                /*
                enum Holoholo.Rpc.Types.EvolvePokemonOutProto.Result {
	                UNSET = 0;
	                SUCCESS = 1;
	                FAILED_POKEMON_MISSING = 2;
	                FAILED_INSUFFICIENT_RESOURCES = 3;
	                FAILED_POKEMON_CANNOT_EVOLVE = 4;
	                FAILED_POKEMON_IS_DEPLOYED = 5;
                }
                }*/

                var countOfEvolvedUnits = 0;
                var xpCount = 0;

                EvolvePokemonOutProto evolvePokemonOutProto;
                do
                {
                    evolvePokemonOutProto = await client.EvolvePokemon(pokemon.Id);

                    if (evolvePokemonOutProto.Result == 1)
                    {
                        System.Console.WriteLine($"Evolved {pokemon.PokemonType} successfully for {evolvePokemonOutProto.ExpAwarded}xp");

                        countOfEvolvedUnits++;
                        xpCount += evolvePokemonOutProto.ExpAwarded;
                    }
                    else
                    {
                        var result = evolvePokemonOutProto.Result;

                        System.Console.WriteLine($"Failed to evolve {pokemon.PokemonType}. " +
                                                 $"EvolvePokemonOutProto.Result was {result}");

                        System.Console.WriteLine($"Due to above error, stopping evolving {pokemon.PokemonType}");
                    }
                }
                while (evolvePokemonOutProto.Result == 1);

                System.Console.WriteLine($"Evolved {countOfEvolvedUnits} pieces of {pokemon.PokemonType} for {xpCount}xp");

                await Task.Delay(3000);
            }
        }
        
        private static async Task TransferAllButStrongestUnwantedPokemon(Client client)
        {
            System.Console.WriteLine("[!] firing up the meat grinder");

            var unwantedPokemonTypes = new[]
            {
                PokemonIds.V0016PokemonPidgey,
                PokemonIds.V0019PokemonRattata,
                PokemonIds.V0013PokemonWeedle,
                PokemonIds.V0041PokemonZubat,
                PokemonIds.V0010PokemonCaterpie,
                PokemonIds.V0017PokemonPidgeotto,
                PokemonIds.V0029PokemonNidoran,
                PokemonIds.V0046PokemonParas,
                PokemonIds.V0048PokemonVenonat,
                PokemonIds.V0054PokemonPsyduck,
                PokemonIds.V0060PokemonPoliwag,
                PokemonIds.V0079PokemonSlowpoke,
                PokemonIds.V0096PokemonDrowzee,
                PokemonIds.V0092PokemonGastly,
                PokemonIds.V0118PokemonGoldeen,
                PokemonIds.V0120PokemonStaryu,
                PokemonIds.V0129PokemonMagikarp,
                PokemonIds.V0133PokemonEevee,
                PokemonIds.V0147PokemonDratini
            };

            var inventory = await client.GetInventory();
            var pokemons = inventory.Payload[0].Bag
                                .Items
                                .Select(i => i.Item?.Pokemon)
                                .Where(p => p != null && p?.PokemonType != PokemonIds.PokemonUnset)
                                .ToArray();

            foreach (var unwantedPokemonType in unwantedPokemonTypes)
            {
                var pokemonOfDesiredType = pokemons.Where(p => p.PokemonType == unwantedPokemonType)
                                                   .OrderByDescending(p => p.Cp)
                                                   .ToList();

                var unwantedPokemon = pokemonOfDesiredType.Skip(1) // keep the strongest one for potential battle-evolving
                                                          .ToList();

                System.Console.WriteLine($"Grinding {unwantedPokemon.Count} pokemons of type {unwantedPokemonType}");
                await TransferAllGivenPokemons(client, unwantedPokemon);
            }

            System.Console.WriteLine("[!] finished grinding all the meat");
        }

        private static string GetFriendlyPokemonName(MapObjectsResponse.Types.Payload.Types.PokemonIds id)
        {
            var name = Enum.GetName(typeof (PokemonProto.Types.PokemonIds), id);
            return name?.Substring(name.IndexOf("Pokemon") + 7);
        }

        private static string GetFriendlyItemsString(IEnumerable<FortSearchResponse.Types.Item> items)
        {
            var enumerable = items as IList<FortSearchResponse.Types.Item> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => (MiscEnums.Item) i.Item_)
                          .Select(kvp => new {ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount)})
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }
    }
}
