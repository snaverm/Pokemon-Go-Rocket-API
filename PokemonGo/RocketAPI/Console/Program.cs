using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
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
            var client = new Client();

            await client.LoginPtc("Sekret-username", "Sekret-password");
            //await client.LoginGoogle(Settings.DeviceId, Settings.Email, Settings.LongDurationToken);
            var serverResponse = await client.GetServer();
            var profile = await client.GetProfile();
            var settings = await client.GetSettings();
            var encounters = await client.GetEncounters();
        }
    }
}
