using System.Configuration;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        public AuthType AuthType => (AuthType)Enum.Parse(typeof(AuthType), GetSetting());
        public  string PtcUsername => GetSetting() != string.Empty ? GetSetting() : "username";
        public  string PtcPassword => GetSetting() != string.Empty? GetSetting() : "password";
        public double DefaultLatitude => GetSetting() != string.Empty ? double.Parse(GetSetting(), CultureInfo.InvariantCulture) : 52.379189; //Default Amsterdam Central Station
        public double DefaultLongitude => GetSetting() != string.Empty ? double.Parse(GetSetting(),CultureInfo.InvariantCulture) : 4.899431;//Default Amsterdam Central Station
        public  string GoogleRefreshToken
        {
            get { return GetSetting() != string.Empty ? GetSetting() : string.Empty; }
            set { SetSetting(value); }
        }

        private string GetSetting([CallerMemberName]string key = null)
        {
            return ConfigurationManager.AppSettings[key];
        }
        private void SetSetting(string value, [CallerMemberName]string key = null)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (key != null) configFile.AppSettings.Settings[key].Value = value;
            configFile.Save();
        }
    }
}
