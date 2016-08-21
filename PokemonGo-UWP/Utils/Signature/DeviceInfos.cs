using PokemonGo.RocketAPI.Helpers;
using System;
using Windows.Devices.Sensors;
using Superbest_random;
using System.Collections.Generic;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    /// Device infos used to sign requests
    /// </summary>
    public class DeviceInfos
    {

        public static readonly IDeviceInfoExtended Current;


        static DeviceInfos()
        {
            //Current = Current ?? new DeviceInfosAndroid();
            Current = Current ?? new DeviceInfosIOS();
        }

    }
}
