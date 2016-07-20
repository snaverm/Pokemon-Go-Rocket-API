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
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;

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

            if (Settings.AuthType == AuthType.Ptc)
                await client.DoPtcLogin(Settings.PtcUsername, Settings.PtcPassword);
            else if (Settings.AuthType == AuthType.Google)
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

                await ExecuteCatchAllNearbyPokemons(client);

                await Task.Delay(15000);
            }
        }

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            foreach (var pokemon in pokemons)
            {
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonRespone = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL); //note: reverted from settings because this should not be part of settings but part of logic
                } 
                while(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed);

                System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {GetFriendlyPokemonName(pokemon.PokemonId)}" : $"[{DateTime.Now.ToString("HH:mm:ss")}] {GetFriendlyPokemonName(pokemon.PokemonId)} got away..");
                await Task.Delay(5000);
            }
        }

        private static string GetFriendlyPokemonName(PokemonId id)
        {
            var name = Enum.GetName(typeof (GetInventoryResponse), id);
            return name?.Substring(name.IndexOf("Pokemon") + 7);
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
    }
}
