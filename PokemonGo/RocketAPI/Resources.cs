using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI
{
    public class Resources
    {
        public const string RPC_URL = @"https://pgorelease.nianticlabs.com/plfe/rpc";
        public const string NUMBERED_RPC_URL = @"https://pgorelease.nianticlabs.com/plfe/{0}/rpc";
        public const string LOGIN_URL = "https://sso.pokemon.com/sso/login?service=https%3A%2F%2Fsso.pokemon.com%2Fsso%2Foauth2.0%2FcallbackAuthorize";
        public const string LOGIN_OAUTH = "https://sso.pokemon.com/sso/oauth2.0/accessToken";

        public const string GOOGLE_GRANT_REFRESH_ACCESS_URL = "https://android.clients.google.com/auth";
    }
}
