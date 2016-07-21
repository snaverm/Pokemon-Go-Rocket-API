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

        public async Task<IEnumerable<PokemonData>> GetMyPokemon()
        {
            var inventory = await _client.GetInventory();
            return inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);
        } 

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer()
        {
            var myPokemon = await GetMyPokemon();

            return myPokemon
                .GroupBy(p => p.PokemonId)
                .Where(x => x.Count() > 1)
                .SelectMany(p => p.OrderByDescending(x => x.Cp).Skip(1).ToList());
        }
    }
}
