using PokemonGo.RocketAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
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

        public float Latitude => (float)GameClient.Geoposition.Coordinate.Point.Position.Latitude;

        public float Longitude => (float)GameClient.Geoposition.Coordinate.Point.Position.Longitude;

        public float Altitude => (float)GameClient.Geoposition.Coordinate.Point.Position.Altitude;

        // TODO: why 3? need more infos.
        public uint Floor => 3;

        // TODO: why 1? need more infos.
        public ulong LocationType => 1;

    }
    /// <summary>
    /// Device infos used to sign requests
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


        #region Device

        public string DeviceID
        {
            get
            {
                if (string.IsNullOrEmpty(SettingsService.Instance.UDID)) SettingsService.Instance.UDID = RandomString(40);
                return SettingsService.Instance.UDID;
            }
        }

        private readonly Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEF0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // TODO: check for iPhone
        public string FirmwareBrand => "iPhone OS";

        // TODO: check for iPhone
        public string FirmwareType => "9.3.3";

        #endregion

        #region LocationFixes
        private ILocationFix[] _locationFixes = { LocationFixFused.Instance };
        public ILocationFix[] LocationFixes => _locationFixes;


        #endregion

        #region Sensors

        private Random _random = new Random();

        private readonly Accelerometer _accelerometer = Accelerometer.GetDefault();

        private Compass _compass = Compass.GetDefault();

        private readonly Magnetometer _magnetometer = Magnetometer.GetDefault();

        private readonly Gyrometer _gyrometer = Gyrometer.GetDefault();

        // TODO: if no accelerometer/compass (e.g. on desktop) what should we do?
        //Almost all iPhones have accell && magnetometer, so if we dont have it, simulate it with some basic distrubutions
        public double AccelNormalizedX => _accelerometer != null && _accelerometer.GetCurrentReading() != null ?
                                           _accelerometer.GetCurrentReading().AccelerationX : _random.NextGaussian(0.0, 0.3);

        public double AccelNormalizedY => _accelerometer != null && _accelerometer.GetCurrentReading() != null ?
                                           _accelerometer.GetCurrentReading().AccelerationY : _random.NextGaussian(0.0, 0.3);

        public double AccelNormalizedZ => _accelerometer != null && _accelerometer.GetCurrentReading() != null ?
                                           _accelerometer.GetCurrentReading().AccelerationZ : _random.NextGaussian(0.0, 0.3);

        public string TimestampSnapshot = ""; //(ulong)(ElapsedMilliseconds - 230L) = TimestampSinceStart - 30L

        public double MagnetometerX => _magnetometer != null && _magnetometer.GetCurrentReading() != null ?
                                           _magnetometer.GetCurrentReading().MagneticFieldX : _random.NextGaussian(0.0, 0.1);

        public double MagnetometerY => _magnetometer != null && _magnetometer.GetCurrentReading() != null ?
                                           _magnetometer.GetCurrentReading().MagneticFieldY : _random.NextGaussian(0.0, 0.1);

        public double MagnetometerZ => _magnetometer != null && _magnetometer.GetCurrentReading() != null ?
                                           _magnetometer.GetCurrentReading().MagneticFieldZ : _random.NextGaussian(0.0, 0.1);

        public double AccelRawX => _gyrometer != null && _gyrometer.GetCurrentReading() != null ?
                                        _gyrometer.GetCurrentReading().AngularVelocityX : _random.NextGaussian(0.0, 0.1);

        public double AccelRawY => _gyrometer != null && _gyrometer.GetCurrentReading() != null ?
                                        _gyrometer.GetCurrentReading().AngularVelocityY : _random.NextGaussian(0.0, 0.1);

        public double AccelRawZ => _gyrometer != null && _gyrometer.GetCurrentReading() != null ?
                                        _gyrometer.GetCurrentReading().AngularVelocityZ : _random.NextGaussian(0.0, 0.1);



        // TODO: missing values!
        //AngleNormalizedX = 17.950439453125,
        //AngleNormalizedY = -23.36273193359375,
        //AngleNormalizedZ = -48.8250732421875,

        //GyroscopeRawX = 7.62939453125E-05,
        //GyroscopeRawY = -0.00054931640625,
        //GyroscopeRawZ = 0.0024566650390625,

        public string AccelerometerAxes = "3";

        #endregion

    }
}
