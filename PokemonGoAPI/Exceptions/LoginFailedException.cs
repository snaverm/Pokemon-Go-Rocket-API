﻿using System;

namespace PokemonGo.RocketAPI.Exceptions
{

    /// <summary>
    /// 
    /// </summary>
    public class LoginFailedException : Exception
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string LoginResponse { get; }

        #endregion


        public LoginFailedException()
        {
        }

        public LoginFailedException(string loginResponse)
        {
            LoginResponse = loginResponse;
        }

    }

}