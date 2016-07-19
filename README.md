# Pokemon-Go-Rocket-API

# Pokemon Go Client API Library in C# #

Example:

```
var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);

await client.LoginPtc("FeroxRev", "Sekret");
//await client.LoginGoogle(Settings.DeviceId, Settings.Email, Settings.LongDurationToken);
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
#PTC Login / Google last part
#Get Map Objects and Inventory
#Search for gyms/pokestops/spawns
#Farm pokestops
#Farm all pokemons in neighbourhood
```

Todo

```
#Gotta catch them all
#Map Enums
```

To use the google login:
```
Install charles and follow ios/android:
https://www.charlesproxy.com/documentation/using-charles/ssl-certificates/

Disable firewall and login to Pokemon Go on phone. Look for android.google.com request and fetch the headers from the android.google.com/auth request and put them in Settings.cs.
AndroidID = DeviceId
Token = LongDurationToken (the oauth.#blabla token)
```
