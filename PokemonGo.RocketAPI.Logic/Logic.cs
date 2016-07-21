using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logic.Utils;

namespace PokemonGo.RocketAPI.Logic
{
    public class Logic
    {
        private readonly Client _client;
        private readonly ISettings _clientSettings;
        private readonly Inventory _inventory;

        public Logic(ISettings clientSettings)
        {
            _clientSettings = clientSettings;
            _client = new Client(_clientSettings);
            _inventory = new Inventory(_client);
        }

        public async void Execute()
        {
            Console.WriteLine($"Starting Execute on login server: {_clientSettings.AuthType}");
            
            if (_clientSettings.AuthType == AuthType.Ptc)
                await _client.DoPtcLogin(_clientSettings.PtcUsername, _clientSettings.PtcPassword);
            else if (_clientSettings.AuthType == AuthType.Google)
                await _client.DoGoogleLogin();

            while (true)
            {
                try
                {
                    await _client.SetServer();
                    await TransferDuplicatePokemon();
                    await RepeatAction(10, async () => await ExecuteFarmingPokestopsAndPokemons(_client));

                    /*
                * Example calls below
                *
                var profile = await _client.GetProfile();
                var settings = await _client.GetSettings();
                var mapObjects = await _client.GetMapObjects();
                var inventory = await _client.GetInventory();
                var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);
                */
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                }

                await Task.Delay(10000);
            }
        }

        public async Task RepeatAction(int repeat, Func<Task> action)
        {
            for (int i = 0; i < repeat; i++)
                await action();
        }

        private async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts).Where(i => i.Type == FortType.Checkpoint && i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());

            foreach (var pokeStop in pokeStops)
            {
                var update = await client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await client.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Farmed XP: {fortSearch.ExperienceAwarded}, Gems: { fortSearch.GemsAwarded}, Eggs: {fortSearch.PokemonDataEgg} Items: {StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded)}");

                await Task.Delay(15000);
                await ExecuteCatchAllNearbyPokemons(client);
            }
        }

        private async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            foreach (var pokemon in pokemons)
            {
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonResponse = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);
                var pokemonCP = encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp;
                var pokeball = await GetBestBall(pokemonCP);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, pokeball);
                }
                while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed);

                System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} using a {pokeball}" : $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemon.PokemonId} with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} got away..");
                await Task.Delay(5000);
                
                await TransferDuplicatePokemon();
            }
        }

        private async Task EvolveAllGivenPokemons(IEnumerable<Pokemon> pokemonToEvolve)
        {
            foreach (var pokemon in pokemonToEvolve)
            {
                EvolvePokemonOut evolvePokemonOutProto;
                do
                {
                    evolvePokemonOutProto = await _client.EvolvePokemon((ulong)pokemon.Id); 

                    if (evolvePokemonOutProto.Result == EvolvePokemonOut.Types.EvolvePokemonStatus.PokemonEvolvedSuccess)
                        System.Console.WriteLine($"Evolved {pokemon.PokemonType} successfully for {evolvePokemonOutProto.ExpAwarded}xp");
                    else
                        System.Console.WriteLine($"Failed to evolve {pokemon.PokemonType}. EvolvePokemonOutProto.Result was {evolvePokemonOutProto.Result}, stopping evolving {pokemon.PokemonType}");

                    await Task.Delay(3000);
                }
                while (evolvePokemonOutProto.Result == EvolvePokemonOut.Types.EvolvePokemonStatus.PokemonEvolvedSuccess);

                await Task.Delay(3000);
            }
        }

        private async Task TransferDuplicatePokemon()
        {
            var duplicatePokemons = await _inventory.GetDuplicatePokemonToTransfer();

            if (duplicatePokemons != null && duplicatePokemons.Any())
                System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Transfering duplicate Pokemon");

            foreach (var duplicatePokemon in duplicatePokemons)
            {
                var transfer = await _client.TransferPokemon(duplicatePokemon.Id);
                System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Transfer {duplicatePokemon.PokemonId} with {duplicatePokemon.Cp} CP");
                await Task.Delay(500);
            }
        }
        
        private async Task<MiscEnums.Item> GetBestBall(int? pokemonCP)
        {
            var inventory = await _client.GetInventory();
            var ballCollection = inventory.InventoryDelta.InventoryItems
                   .Select(i => i.InventoryItemData?.Item)
                   .Where(p => p != null)
                   .GroupBy(i => (MiscEnums.Item)i.Item_)
                   .Select(kvp => new { ItemId = kvp.Key, Amount = kvp.Sum(x => x.Count) })
                   .Where(y => y.ItemId == MiscEnums.Item.ITEM_POKE_BALL
                            || y.ItemId == MiscEnums.Item.ITEM_GREAT_BALL
                            || y.ItemId == MiscEnums.Item.ITEM_ULTRA_BALL
                            || y.ItemId == MiscEnums.Item.ITEM_MASTER_BALL);

            var pokeBallsCount = ballCollection.Where(p => p.ItemId == MiscEnums.Item.ITEM_POKE_BALL).
                DefaultIfEmpty(new { ItemId = MiscEnums.Item.ITEM_POKE_BALL, Amount = 0 }).FirstOrDefault().Amount;
            var greatBallsCount = ballCollection.Where(p => p.ItemId == MiscEnums.Item.ITEM_GREAT_BALL).
                DefaultIfEmpty(new { ItemId = MiscEnums.Item.ITEM_GREAT_BALL, Amount = 0 }).FirstOrDefault().Amount;
            var ultraBallsCount = ballCollection.Where(p => p.ItemId == MiscEnums.Item.ITEM_ULTRA_BALL).
                DefaultIfEmpty(new { ItemId = MiscEnums.Item.ITEM_ULTRA_BALL, Amount = 0 }).FirstOrDefault().Amount;
            var masterBallsCount = ballCollection.Where(p => p.ItemId == MiscEnums.Item.ITEM_MASTER_BALL).
                DefaultIfEmpty(new { ItemId = MiscEnums.Item.ITEM_MASTER_BALL, Amount = 0 }).FirstOrDefault().Amount;

            if (masterBallsCount > 0 && pokemonCP >= 1000)
                return MiscEnums.Item.ITEM_MASTER_BALL;
            else if (ultraBallsCount > 0 && pokemonCP >= 1000)
                return MiscEnums.Item.ITEM_ULTRA_BALL;
            else if (greatBallsCount > 0 && pokemonCP >= 1000)
                return MiscEnums.Item.ITEM_GREAT_BALL;

            if (ultraBallsCount > 0 && pokemonCP >= 600)
                return MiscEnums.Item.ITEM_ULTRA_BALL;
            else if(greatBallsCount > 0 && pokemonCP >= 600)
                return MiscEnums.Item.ITEM_GREAT_BALL;

            if (greatBallsCount > 0 && pokemonCP >= 350)
                return MiscEnums.Item.ITEM_GREAT_BALL;

            if (pokeBallsCount > 0)
                return MiscEnums.Item.ITEM_POKE_BALL;
            if (greatBallsCount > 0)
                return MiscEnums.Item.ITEM_GREAT_BALL;
            if (ultraBallsCount > 0)
                return MiscEnums.Item.ITEM_ULTRA_BALL;
            if (masterBallsCount > 0)
                return MiscEnums.Item.ITEM_MASTER_BALL;

            return MiscEnums.Item.ITEM_POKE_BALL;
        }
    }
}
