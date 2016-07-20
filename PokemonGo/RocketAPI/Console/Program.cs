using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {

        static readonly ISettings ClientSettings = new Settings();

        static void Main(string[] args)
        {
            Task.Run(() =>
            {
                try
                {
                    Execute();
                }
                catch (PtcOfflineException)
                {
                    System.Console.WriteLine("PTC Servers are probably down OR your credentials are wrong. Try google");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Unhandled exception: {ex}");
                }
            });
             System.Console.ReadLine();
        }

        static async void Execute()
        {
            var client = new Client(ClientSettings);

            if (ClientSettings.AuthType == AuthType.Ptc)
                await client.DoPtcLogin(ClientSettings.PtcUsername, ClientSettings.PtcPassword);
            else if (ClientSettings.AuthType == AuthType.Google)
                await client.DoGoogleLogin();
            
            await client.SetServer();
            var profile = await client.GetProfile();
            var settings = await client.GetSettings();
            var mapObjects = await client.GetMapObjects();
            var inventory = await client.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);


            await ExecuteFarmingPokestopsAndPokemons(client);
            //await ExecuteCatchAllNearbyPokemons(client);

            
        }

        private static async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts).Where(i => i.Type == FortType.Checkpoint && i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());

            foreach (var pokeStop in pokeStops)
            {
                var update = await client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await client.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Farmed XP: {fortSearch.ExperienceAwarded}, Gems: { fortSearch.GemsAwarded}, Eggs: {fortSearch.PokemonDataEgg} Items: {GetFriendlyItemsString(fortSearch.ItemsAwarded)}");

                await Task.Delay(15000);
                await ExecuteCatchAllNearbyPokemons(client);
            }
        }

        private static int checkForDuplicates = -1;

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            foreach (var pokemon in pokemons)
            {
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonResponse = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL); //note: reverted from settings because this should not be part of settings but part of logic
                } 
                while(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed);

                System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemon.PokemonId} with {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp}" : $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} got away..");
                await Task.Delay(5000);
                

                await Task.Delay(5000);
            }
        }
        private static async Task TransferAllGivenPokemons(Client client, IEnumerable<PokemonData> unwantedPokemons)
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
                    System.Console.WriteLine($"Shoved another {pokemon.PokemonId} down the meat grinder");
                }
                else
                {
                    var status = transferPokemonResponse.Status;

                    System.Console.WriteLine($"Somehow failed to grind {pokemon.PokemonId}. " +
                                             $"ReleasePokemonOutProto.Status was {status}");
                }

                await Task.Delay(3000);
            }
        }

        private static async Task EvolveAllGivenPokemons(Client client, IEnumerable<Pokemon> pokemonToEvolve)
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

                EvolvePokemonOut evolvePokemonOutProto;
                do
                {
                    evolvePokemonOutProto = await client.EvolvePokemon((ulong)pokemon.Id); //todo: someone check whether this still works

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
                PokemonId.Pidgey,
                PokemonId.Rattata,
                PokemonId.Weedle,
                PokemonId.Zubat,
                PokemonId.Caterpie,
                PokemonId.Pidgeotto,
                PokemonId.NidoranFemale,
                PokemonId.Paras,
                PokemonId.Venonat,
                PokemonId.Psyduck,
                PokemonId.Poliwag,
                PokemonId.Slowpoke,
                PokemonId.Drowzee,
                PokemonId.Gastly,
                PokemonId.Goldeen,
                PokemonId.Staryu,
                PokemonId.Magikarp,
                PokemonId.Eevee,
                PokemonId.Dratini
            };

            var inventory = await client.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems
                                .Select(i => i.InventoryItemData?.Pokemon)
                                .Where(p => p != null && p?.PokemonId > 0)
                                .ToArray();

            foreach (var unwantedPokemonType in unwantedPokemonTypes)
            {
                var pokemonOfDesiredType = pokemons.Where(p => p.PokemonId == unwantedPokemonType)
                                                   .OrderByDescending(p => p.Cp)
                                                   .ToList();

                var unwantedPokemon = pokemonOfDesiredType.Skip(1) // keep the strongest one for potential battle-evolving
                                                          .ToList();

                System.Console.WriteLine($"Grinding {unwantedPokemon.Count} pokemons of type {unwantedPokemonType}");
                await TransferAllGivenPokemons(client, unwantedPokemon);
            }

            System.Console.WriteLine("[!] finished grinding all the meat");
        }

        private static string GetFriendlyItemsString(IEnumerable<FortSearchResponse.Types.ItemAward> items)
        {
            var enumerable = items as IList<FortSearchResponse.Types.ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                          .Select(kvp => new {ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount)})
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }

        private static async void TransferDuplicatePokemon(Client client)
        {

            checkForDuplicates++;
            if (checkForDuplicates % 50 == 0)
            {
                checkForDuplicates = 0;
                System.Console.WriteLine($"Check for duplicates");
                var inventory = await client.GetInventory();
                var allpokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId >0);

                var dupes = allpokemons.OrderBy(x => x.Cp).Select((x, i) => new { index = i, value = x })
                  .GroupBy(x => x.value.PokemonId)
                  .Where(x => x.Skip(1).Any());

                for (int i = 0; i < dupes.Count(); i++)
                {
                    for (int j = 0; j < dupes.ElementAt(i).Count() - 1; j++)
                    {
                        var dubpokemon = dupes.ElementAt(i).ElementAt(j).value;
                        var transfer = await client.TransferPokemon(dubpokemon.Id);
                        System.Console.WriteLine($"Transfer {dubpokemon.PokemonId} with {dubpokemon.Cp} CP (highest has {dupes.ElementAt(i).Last().value.Cp})");
                    }
                }
            }
        }
    }
}
