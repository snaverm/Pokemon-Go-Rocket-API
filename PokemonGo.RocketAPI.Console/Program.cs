using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Task.Run(() =>
            {
                try
                {
                    new Logic.Logic(new Settings()).Execute();
                }
                catch (PtcOfflineException)
                {
                    System.Console.WriteLine("PTC Servers are probably down OR your credentials are wrong. Try google");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Unhandled exception: {ex}");
                }
            });
             System.Console.ReadLine();
        }

        
    }
}
