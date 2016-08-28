using System.Collections.Generic;

namespace PokemonGoAPI.Login
{
    internal class PtcAuthenticationTicketResponse : PtcLoginParameters
    {

        /// <summary>
        /// A list of errors returned from the Authentication request.
        /// </summary>
        public List<string> Errors { get; set; }

    }

}