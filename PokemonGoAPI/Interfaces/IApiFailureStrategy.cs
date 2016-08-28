using POGOProtos.Networking.Envelopes;
using PokemonGoAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoAPI.Interfaces
{

    public interface IApiFailureStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<ApiOperation> HandleApiFailure(string[] url, RequestEnvelope request, ResponseEnvelope response);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response);
    }

}
