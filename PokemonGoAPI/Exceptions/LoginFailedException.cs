using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class LoginFailedException : Exception
    {
        public LoginFailedException(HttpResponseMessage loginResponse)
        {
            this.LoginResponse = loginResponse;
        }
        public HttpResponseMessage LoginResponse { get; private set; }

        public string LoginResponseContentString
        {
            get
            {
                return Task.Run(async () => await this.LoginResponse.Content.ReadAsStringAsync()).Result;
            }
        }
    }
}