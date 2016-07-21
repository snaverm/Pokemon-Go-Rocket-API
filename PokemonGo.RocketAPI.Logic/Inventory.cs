using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo.RocketAPI.Logic
{
    public class Inventory
    {
        private readonly Client _client;

        public Inventory(Client client)
        {
            _client = client;
        }

        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            var inventory = await _client.GetInventory();
            return inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);
        }

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer()
        {
            var myPokemon = await GetPokemons();

            return myPokemon
                .GroupBy(p => p.PokemonId)
                .Where(x => x.Count() > 1)
                .SelectMany(p => p.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Skip(1).ToList());
        }

        public async Task<IEnumerable<Item>> GetItems()
        {
            var inventory = await _client.GetInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null);
        }

        public async Task<int> GetItemAmountByType(MiscEnums.Item type)
        {
            var pokeballs = await GetItems();
            return pokeballs.FirstOrDefault(i => (MiscEnums.Item)i.Item_ == type)?.Count ?? 0;
        }

        public async Task<IEnumerable<Item>> GetItemsToRecycle(ISettings settings)
        {
            var myItems = await GetItems();

            return myItems
                .Where(x => settings.itemRecycleFilter.Any(f => f.Key == ((AllEnum.ItemId)x.Item_) && x.Count > f.Value))
                .Select(x => new Item { Item_ = x.Item_, Count = x.Count - settings.itemRecycleFilter.Single(f => f.Key == (AllEnum.ItemId)x.Item_).Value, Unseen = x.Unseen });
        }
    }
}