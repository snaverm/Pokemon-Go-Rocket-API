﻿using PokemonGo.RocketAPI.Helpers;
using Superbest_random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace PokemonGo_UWP.Utils
{

    /// <summary>
    /// Device infos used to sign requests
    /// </summary>
    public class DeviceInfosIOS : IDeviceInfoExtended
    {

        public DeviceInfosIOS()
        {
        }


        #region Device

        public string DeviceID
        {
            get
            {
                if (string.IsNullOrEmpty(SettingsService.Instance.Udid) || SettingsService.Instance.Udid.Length != 32)
                {
                    try
                    {
                        SettingsService.Instance.Udid = UdidGenerator.GenerateUdid().Substring(0,32).ToLower();
                    }
                    catch (Exception)
                    {
                        //Fallback solution with random hex
                        SettingsService.Instance.Udid = Utilities.RandomHex(32).ToLower();
                    }
                }
                return SettingsService.Instance.Udid.ToLower();
            }
        }

        public string AndroidBoardName => "";
        public string AndroidBootloader => "";
        public string DeviceBrand => "Apple";
        public string DeviceModel => "iPhone";
        public string DeviceModelIdentifier => "";
        public string DeviceModelBoot => "iPhone6,1";
        public string HardwareManufacturer => "Apple";
        public string HardwareModel => "N51AP";
        public string FirmwareTags => "";
        public string FirmwareFingerprint => "";

        public string FirmwareBrand => "iPhone OS";

        public string FirmwareType => "9.3.3";

        #endregion

        #region LocationFixes
        private List<ILocationFix> _gpsLocationFixes = new List<ILocationFix>();
        private object _gpsLocationFixesLock = new object();
        public ILocationFix[] LocationFixes
        {
            get
            {
                lock (_gpsLocationFixesLock)
                {
                    //atomically exchange lists (new is empty)
                    List<ILocationFix> data = _gpsLocationFixes;
                    _gpsLocationFixes = new List<ILocationFix>();
                    return data.ToArray();
                }
            }
        }

        public void CollectLocationData()
        {
            ILocationFix loc = LocationFixIOS.CollectData();

            if (loc != null)
            {
                lock (_gpsLocationFixesLock)
                {
                    _gpsLocationFixes.Add(loc);
                }
            }
        }


        #endregion


        public string Platform => "IOS";

        public int Version => 3300;

        public long TimeSnapshot => DeviceInfos.RelativeTimeFromStart;


        //IOS doe not use sattelites info
        private IGpsSattelitesInfo[] _gpsSattelitesInfo = new IGpsSattelitesInfo[0];
        public IGpsSattelitesInfo[] GpsSattelitesInfo => _gpsSattelitesInfo;

        private ActivityStatusIOS _activityStatus = new ActivityStatusIOS();
        public IActivityStatus ActivityStatus => _activityStatus;

        private class ActivityStatusIOS : IActivityStatus
        {
            public bool Stationary => true;
            public bool Tilting => GameClient.Geoposition?.Coordinate?.Speed < 3 ? true : false;
            public bool Walking => GameClient.Geoposition?.Coordinate?.Speed > 3 && GameClient.Geoposition.Coordinate?.Speed > 7 ? true : false;
            public bool Automotive => GameClient.Geoposition?.Coordinate?.Speed > 20 ? true : false;

            public bool Cycling => false;

            public bool Running => false;


        }


        private SensorInfoIOS _sensors = new SensorInfoIOS();
        public ISensorInfo Sensors => _sensors;

        private class SensorInfoIOS : ISensorInfo
        {
            private readonly Random _random = new Random();

            private readonly Accelerometer _accelerometer = Accelerometer.GetDefault();

            private readonly Magnetometer _magnetometer = Magnetometer.GetDefault();

            private readonly Gyrometer _gyrometer = Gyrometer.GetDefault();

            private readonly Inclinometer _inclinometer = Inclinometer.GetDefault();


            public long TimeSnapshot => DeviceInfos.RelativeTimeFromStart;

            //public ulong TimestampSnapshot = 0; //(ulong)(ElapsedMilliseconds - 230L) = TimestampSinceStart - 30L

            public double MagnetometerX => _magnetometer?.GetCurrentReading()?.MagneticFieldX ?? _random.NextGaussian(0.0, 0.1);

            public double MagnetometerY => _magnetometer?.GetCurrentReading()?.MagneticFieldY ?? _random.NextGaussian(0.0, 0.1);

            public double MagnetometerZ => _magnetometer?.GetCurrentReading()?.MagneticFieldZ ?? _random.NextGaussian(0.0, 0.1);

            public double GyroscopeRawX => _gyrometer?.GetCurrentReading()?.AngularVelocityX ?? _random.NextGaussian(0.0, 0.1);

            public double GyroscopeRawY => _gyrometer?.GetCurrentReading()?.AngularVelocityY ?? _random.NextGaussian(0.0, 0.1);

            public double GyroscopeRawZ => _gyrometer?.GetCurrentReading()?.AngularVelocityZ ?? _random.NextGaussian(0.0, 0.1);

            public double AngleNormalizedX => _inclinometer?.GetCurrentReading()?.PitchDegrees ?? _random.NextGaussian(0.0, 5.0);
            public double AngleNormalizedY => _inclinometer?.GetCurrentReading()?.YawDegrees ?? _random.NextGaussian(0.0, 5.0);
            public double AngleNormalizedZ => _inclinometer?.GetCurrentReading()?.RollDegrees ?? _random.NextGaussian(0.0, 5.0);

            public double AccelRawX => _accelerometer?.GetCurrentReading()?.AccelerationX ?? _random.NextGaussian(0.0, 0.3);

            public double AccelRawY => _accelerometer?.GetCurrentReading()?.AccelerationY ?? _random.NextGaussian(0.0, 0.3);

            public double AccelRawZ => _accelerometer?.GetCurrentReading()?.AccelerationZ ?? _random.NextGaussian(0.0, 0.3);


            public ulong AccelerometerAxes => 3;


        }

        #region Locationfix

        private class LocationFixIOS : ILocationFix
        {
            private static readonly Random _random = new Random();

            private LocationFixIOS()
            {
            }

            public static ILocationFix CollectData()
            {
                if (GameClient.Geoposition.Coordinate == null)
                    return null; //Nothing to collect

                LocationFixIOS loc = new LocationFixIOS();
                //Collect provider
                switch (GameClient.Geoposition.Coordinate.PositionSource)
                {
                    case Windows.Devices.Geolocation.PositionSource.WiFi:
                    case Windows.Devices.Geolocation.PositionSource.Cellular:
                        loc.Provider = "network"; break;
                    default:
                        loc.Provider = "fused"; break;
                }

                //1 = no fix, 2 = acquiring/inaccurate, 3 = fix acquired
                loc.ProviderStatus = 3;

                //Collect coordinates

                loc.Latitude = (float)GameClient.Geoposition.Coordinate.Point.Position.Latitude;
                loc.Longitude = (float)GameClient.Geoposition.Coordinate.Point.Position.Longitude;
                loc.Altitude = (float)GameClient.Geoposition.Coordinate.Point.Position.Altitude;

                //Not filled on iOS
                //loc.Floor = 3;

                // TODO: why 1? need more infos.
                loc.LocationType = 1;

                loc.TimeSnapshot = DeviceInfos.RelativeTimeFromStart;

                loc.HorizontalAccuracy = (float?)GameClient.Geoposition.Coordinate?.Accuracy ?? (float)_random.NextDouble(5.0, 50.0);

                loc.VerticalAccuracy = (float?)GameClient.Geoposition.Coordinate?.AltitudeAccuracy ?? (float)_random.NextDouble(10.0, 30.0);

                loc.Course = -1.0f;

                return loc;
            }

            public string Provider { get; private set; }

            public ulong ProviderStatus { get; private set; }

            public float Latitude { get; private set; }

            public float Longitude { get; private set; }

            public float Altitude { get; private set; }

            public uint Floor { get; private set; }

            public ulong LocationType { get; private set; }

            public long TimeSnapshot { get; private set; }
            public float HorizontalAccuracy { get; private set; }
            public float VerticalAccuracy { get; private set; }
            public float Course { get; private set; }

            public float Speed { get; private set; }

        }

        #endregion

    }

}
