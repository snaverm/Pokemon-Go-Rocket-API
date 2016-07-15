using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
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
            var client = new Client();
            
            var accessToken = await client.GetGoogleAccessToken(Settings.DeviceId, Settings.ClientSig, Settings.Email, Settings.LongDurationToken);
            var profileRequest = RequestBuilder.GetRequest(RequestType.Profile, accessToken, Settings.DefaultLatitude, Settings.DefaultLongitude, 30);

            var serverResponse = await client.GetServer(profileRequest);
            var profile = await client.GetProfile(serverResponse.ApiUrl, profileRequest);

        }
    }
}
