using System;
using Windows.Devices.Sensors;
using PokemonGo.RocketAPI.Helpers;
using Superbest_random;

namespace PokemonGo_UWP.Utils
{
    public class LocationFixFused : ILocationFix
    {
        public static readonly LocationFixFused Instance;


        static LocationFixFused()
        {
            Instance = Instance ?? new LocationFixFused();
        }

        private LocationFixFused()
        {
        }

        public string Provider => "fused";
        //1 = no fix, 2 = acquiring/inaccurate, 3 = fix acquired
        public ulong ProviderStatus => 3;

        public float Latitude => (float) GameClient.Geoposition.Coordinate.Point.Position.Latitude;

        public float Longitude => (float) GameClient.Geoposition.Coordinate.Point.Position.Longitude;

        public float Altitude => (float) GameClient.Geoposition.Coordinate.Point.Position.Altitude;

        // TODO: why 3? need more infos.
        public uint Floor => 3;

        // TODO: why 1? need more infos.
        public ulong LocationType => 1;
    }

    /// <summary>
    ///     Device infos used to sign requests
    /// </summary>
    public class DeviceInfos : IDeviceInfo
    {
        public static readonly DeviceInfos Instance;


        static DeviceInfos()
        {
            Instance = Instance ?? new DeviceInfos();
        }

        private DeviceInfos()
        {
        }

        #region LocationFixes

        public ILocationFix[] LocationFixes { get; } = {LocationFixFused.Instance};

        #endregion

        #region Device

        public string DeviceID
        {
            get
            {
                if (string.IsNullOrEmpty(SettingsService.Instance.Udid))
                {
                    try
                    {
                        SettingsService.Instance.Udid = UdidGenerator.GenerateUdid();
                    }
                    catch (Exception)
                    {
                        //Fallback solution with random hex
                        SettingsService.Instance.Udid = Utilities.RandomHex(40);
                    }
                }
                return SettingsService.Instance.Udid;
            }
        }

        // TODO: check for iPhone
        public string FirmwareBrand => "iPhone OS";

        // TODO: check for iPhone
        public string FirmwareType => "9.3.3";

        #endregion

        #region Sensors

        private readonly Random _random = new Random();

        private readonly Accelerometer _accelerometer = Accelerometer.GetDefault();

        private readonly Magnetometer _magnetometer = Magnetometer.GetDefault();

        private readonly Gyrometer _gyrometer = Gyrometer.GetDefault();

        private readonly Inclinometer _inclinometer = Inclinometer.GetDefault();

        public string TimestampSnapshot = ""; //(ulong)(ElapsedMilliseconds - 230L) = TimestampSinceStart - 30L

        public double MagnetometerX => _magnetometer?.GetCurrentReading() != null
            ? _magnetometer.GetCurrentReading().MagneticFieldX
            : _random.NextGaussian(0.0, 0.1);

        public double MagnetometerY => _magnetometer?.GetCurrentReading() != null
            ? _magnetometer.GetCurrentReading().MagneticFieldY
            : _random.NextGaussian(0.0, 0.1);

        public double MagnetometerZ => _magnetometer?.GetCurrentReading() != null
            ? _magnetometer.GetCurrentReading().MagneticFieldZ
            : _random.NextGaussian(0.0, 0.1);

        public double GyroscopeRawX => _gyrometer?.GetCurrentReading() != null
            ? _gyrometer.GetCurrentReading().AngularVelocityX
            : _random.NextGaussian(0.0, 0.1);

        public double GyroscopeRawY => _gyrometer?.GetCurrentReading() != null
            ? _gyrometer.GetCurrentReading().AngularVelocityY
            : _random.NextGaussian(0.0, 0.1);

        public double GyroscopeRawZ => _gyrometer?.GetCurrentReading() != null
            ? _gyrometer.GetCurrentReading().AngularVelocityZ
            : _random.NextGaussian(0.0, 0.1);

        public double AngleNormalizedX => _inclinometer?.GetCurrentReading() != null
            ? _inclinometer.GetCurrentReading().PitchDegrees
            : _random.NextGaussian(0.0, 5.0);

        public double AngleNormalizedY => _inclinometer?.GetCurrentReading() != null
            ? _inclinometer.GetCurrentReading().YawDegrees
            : _random.NextGaussian(0.0, 5.0);

        public double AngleNormalizedZ => _inclinometer?.GetCurrentReading() != null
            ? _inclinometer.GetCurrentReading().RollDegrees
            : _random.NextGaussian(0.0, 5.0);

        public double AccelRawX => _accelerometer?.GetCurrentReading() != null
            ? _accelerometer.GetCurrentReading().AccelerationX
            : _random.NextGaussian(0.0, 0.3);

        public double AccelRawY => _accelerometer?.GetCurrentReading() != null
            ? _accelerometer.GetCurrentReading().AccelerationY
            : _random.NextGaussian(0.0, 0.3);

        public double AccelRawZ => _accelerometer?.GetCurrentReading() != null
            ? _accelerometer.GetCurrentReading().AccelerationZ
            : _random.NextGaussian(0.0, 0.3);

        public ulong AccelerometerAxes => 3;

        #endregion
    }
}