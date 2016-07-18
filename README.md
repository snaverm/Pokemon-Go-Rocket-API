# Pokemon-Go-Rocket-API

Working:

```
await client.LoginPtc("FeroxRev", "Sekret");
//await client.LoginGoogle(Settings.DeviceId, Settings.Email, Settings.LongDurationToken);
var serverResponse = await client.GetServer();
var profile = await client.GetProfile();
var settings = await client.GetSettings();
var encounters = await client.GetEncounters();
```

Todo:

-Get pokemon/pokestops/gyms

-Gotta catch them all
