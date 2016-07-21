using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AllEnum;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        public AuthType AuthType => AuthType.Ptc;
        public string PtcUsername => "";
        public string PtcPassword => "";
        public double DefaultLatitude => 0;
        public double DefaultLongitude => 0;

        ICollection<KeyValuePair<ItemId, int>> ISettings.itemRecycleFilter
        {
            get
            {
                //Type and amount to keep
                return new[]
                {
                    new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, 50),
                    new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, 50)
                };
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string GoogleRefreshToken
        {
            get { return ""; }
            set
            {
                //UserSettings.Default.GoogleRefreshToken = value;
                //UserSettings.Default.Save();
            }
        }
    }
}
