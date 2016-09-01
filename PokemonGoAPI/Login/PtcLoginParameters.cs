﻿using Newtonsoft.Json;

namespace PokemonGoAPI.Login
{

    /// <summary>
    /// 
    /// </summary>
    internal class PtcLoginParameters
    {

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("lt", Required = Required.Always)]
        public string Lt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("execution", Required = Required.Always)]
        public string Execution { get; set; }

    }

}