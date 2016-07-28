using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.SettingsService;

namespace PokemonGo_UWP.Utils
{
    public class SettingsService
    {

        public static readonly SettingsService Instance;
        static SettingsService() { Instance = Instance ?? new SettingsService(); }

        SettingsHelper _helper;
        private SettingsService() { _helper = new SettingsHelper(); }

        public string PtcAuthToken
        {
            get { return _helper.Read(nameof(PtcAuthToken), string.Empty); }
            set { _helper.Write(nameof(PtcAuthToken), value); }
        }

    }
}
