# Pokemon-Go-Rocket-API

# Pokemon Go Client API Library in C# #

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

Features
```
#PTC Login / Google
#Get Map Objects and Inventory
#Search for gyms/pokestops/spawns
#Farm pokestops
#Farm all pokemons in neighbourhood
#Transfer pokemon
#Evolve pokemon
```

Todo

```
#Gotta catch them all
#Map Enums
```

