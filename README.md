# Pokemon-Go-Rocket-API

# Pokemon Go Client API Library in C# #

General chat: https://discord.gg/5CMa3CY

GitHub issues will be deleted if they are not API related. PR's about improved logic is welcome.

Example:

```
var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);

await client.LoginPtc("FeroxRev", "Sekret");
var serverResponse = await client.GetServer();
var profile = await client.GetProfile();
var settings = await client.GetSettings();
var mapObjects = await client.GetMapObjects();
var inventory = await client.GetInventory();

await ExecuteFarmingPokestops(client);
await ExecuteCatchAllNearbyPokemons(client);
```

*Features*

Note: There is a list of feature requests [here](https://github.com/FeroxRev/Pokemon-Go-Rocket-API/wiki/Feature-requests).

```
#PTC Login / Google
#Get Map Objects and Inventory
#Search for gyms/pokestops/spawns
#Farm pokestops
#Farm all pokemons in neighbourhood and throw berries/use best pokeball
#Transfers duplicate pokemons
#Evolve all pokemons
#Throws away unneeded items

```

*Todo*

```
#Gotta catch them all
#Map Enums
```

