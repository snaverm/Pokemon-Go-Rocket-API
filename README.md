# Pokemon-Go-Rocket-API

Example:

```
var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);

await client.LoginPtc("FeroxRev", "Sekret");
//await client.LoginGoogle(Settings.DeviceId, Settings.Email, Settings.LongDurationToken);
var serverResponse = await client.GetServer();
var profile = await client.GetProfile();
var settings = await client.GetSettings();
var mapObjects = await client.GetMapObjects();

await ExecuteFarmingPokestops(client);
```

Features
```
#PTC Login / Google last part
#Get Map Objects
#Search for gyms/pokestops/spawns
#Farm pokestops
```

Todo

```
#catch the pokemon!
#Gotta catch them all
```
