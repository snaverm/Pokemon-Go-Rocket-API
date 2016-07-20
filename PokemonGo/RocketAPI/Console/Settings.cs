using System.Configuration;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using System;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        public AuthType AuthType => (AuthType)Enum.Parse(typeof(AuthType), GetSetting("AuthType"));
        public  string PtcUsername => GetSetting("PtcUsername") != string.Empty ? GetSetting("PtcUsername") : "username";
        public  string PtcPassword => GetSetting("PtcPassword") != string.Empty? GetSetting("PtcPassword") : "password";
        public double DefaultLatitude => GetSetting("DefaultLatitude") != string.Empty ? double.Parse(GetSetting("DefaultLatitude")) : 52.379189; //Default Amsterdam Central Station
        public double DefaultLongitude => GetSetting("DefaultLongitude") != string.Empty ? double.Parse(GetSetting("DefaultLongitude")) : 4.899431;//Default Amsterdam Central Station
        public  string GoogleRefreshToken
        {
            get { return GetSetting("GoogleRefreshToken") != string.Empty ? GetSetting("GoogleRefreshToken") : string.Empty; }
            set { SetSetting("GoogleRefreshToken", value); }
        }

        private string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        private void SetSetting(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configFile.AppSettings.Settings[key].Value = value;
            configFile.Save();
        }
    }
}
