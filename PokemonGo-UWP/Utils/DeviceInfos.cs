using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;

namespace PokemonGo_UWP.Utils
{
    public class AdapterInfo
    {
        public string Type { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string MAC { get; set; }
    }

    public static class AdaptersHelper
    {
        const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        const int ERROR_BUFFER_OVERFLOW = 111;
        const int MAX_ADAPTER_NAME_LENGTH = 256;
        const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        const int MIB_IF_TYPE_OTHER = 1;
        const int MIB_IF_TYPE_ETHERNET = 6;
        const int MIB_IF_TYPE_TOKENRING = 9;
        const int MIB_IF_TYPE_FDDI = 15;
        const int MIB_IF_TYPE_PPP = 23;
        const int MIB_IF_TYPE_LOOPBACK = 24;
        const int MIB_IF_TYPE_SLIP = 28;

        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

        public static List<AdapterInfo> GetAdapters()
        {
            var adapters = new List<AdapterInfo>();

            long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));
            IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

            int ret = GetAdaptersInfo(pArray, ref structSize);

            if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
            {
                // Buffer was too small, reallocate the correct size for the buffer.
                pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                ret = GetAdaptersInfo(pArray, ref structSize);
            }

            if (ret == 0)
            {
                // Call Succeeded
                IntPtr pEntry = pArray;

                do
                {
                    var adapter = new AdapterInfo();

                    // Retrieve the adapter info from the memory address
                    var entry = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                    // Adapter Type
                    switch (entry.Type)
                    {
                        case MIB_IF_TYPE_ETHERNET:
                            adapter.Type = "Ethernet";
                            break;
                        case MIB_IF_TYPE_TOKENRING:
                            adapter.Type = "Token Ring";
                            break;
                        case MIB_IF_TYPE_FDDI:
                            adapter.Type = "FDDI";
                            break;
                        case MIB_IF_TYPE_PPP:
                            adapter.Type = "PPP";
                            break;
                        case MIB_IF_TYPE_LOOPBACK:
                            adapter.Type = "Loopback";
                            break;
                        case MIB_IF_TYPE_SLIP:
                            adapter.Type = "Slip";
                            break;
                        default:
                            adapter.Type = "Other/Unknown";
                            break;
                    } // switch

                    adapter.Name = entry.AdapterName;
                    adapter.Description = entry.AdapterDescription;

                    // MAC Address (data is in a byte[])
                    adapter.MAC = string.Join(":", Enumerable.Range(0, (int)entry.AddressLength).Select(s => string.Format("{0:X2}", entry.Address[s])));

                    // Get next adapter (if any)

                    adapters.Add(adapter);

                    pEntry = entry.Next;
                }
                while (pEntry != IntPtr.Zero);

                Marshal.FreeHGlobal(pArray);
            }
            else
            {
                Marshal.FreeHGlobal(pArray);
                throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            }

            return adapters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO
        {
            public IntPtr Next;
            public Int32 ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string AdapterDescription;
            public UInt32 AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;
            public Int32 Index;
            public UInt32 Type;
            public UInt32 DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public Int32 LeaseObtained;
            public Int32 LeaseExpires;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public Int32 Context;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDRESS_STRING
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
        }
    }


    /// <summary>
    /// Device infos used to sign requests
    /// </summary>
    public static class DeviceInfos
    {
        #region UDID

        // Suffixes for IPhones from:
        // https://pikeralpha.wordpress.com/2013/10/10/apple-serial-numbers-ending-with-f000-fzzz/
        // https://pikeralpha.wordpress.com/2014/06/11/apple-serial-numbers-ending-with-g000-gzzz/
        private static readonly string[] iphone_serial_suffixes =
        {
            "07P", "07Q", "07R", "07T", "07V", "38W", "38Y", "39C", "39D", "5MC", "5MD", "5MF", "5MG", "5MH", "5MJ",
            "5MK", "5ML", "5MM", "5MN", "5MP", "5MQ", "5MR", "5MT", "5MV", "5MW", "5MY", "5N0", "5QF", "5QG", "5QH",
            "5QJ", "5QK", "5QL", "5QM", "5QN", "5QP", "5QQ", "5QR", "5QT", "5QV", "5QW", "5QY", "5R0", "5R1", "5R2",
            "8GH", "8GJ", "8GK", "8GL", "8GM", "8GN", "8H2", "8H4", "8H5", "8H6", "8H7", "8H8", "F9R", "F9V", "F9Y",
            "FDN", "FDP", "FDQ", "FDR", "FFJ", "FFK", "FFL", "FFM", "FFN", "FFP", "FFQ", "FFR", "FFT", "FFV", "FFW",
            "FG8", "FG9", "FGC", "FGD", "FGF", "FGG", "FGH", "FGJ", "FGK", "FHG", "FHH", "FHJ", "FHK", "FHL", "FHM",
            "FHN", "FHP", "FHQ", "FHR", "FT5", "FT6", "FT7", "FTM", "FTN", "H19", "H1C", "H1D", "H1F", "H1G", "H1H",
            "L01", "L02", "L03", "L04", "L05", "LFL", "LFM", "LFN", "LFP", "LFT", "LFV", "LFW", "LFY", "LG0", "LG2",
            "M1N", "M1P", "M1Q", "M1R", "M1T", "M1V", "M1W", "M1Y", "M20", "M21", "NDD", "NDF", "NDG", "NDH", "NDJ",
            "NDK", "NDL", "NDM", "NDN", "NDP", "NJJ", "NJK", "NJL", "NJM", "NJN", "NJP", "NJQ", "NJR", "NJT", "NLQ",
            "NLR", "NLT", "NLV", "NLW", "NLY", "NM0", "NM1", "NM2", "NM3", "NNK", "NNL", "NNM", "NNN", "NNP", "NNQ",
            "NNR", "NNT", "NNV", "P6H", "P6J", "P6K", "P6L", "P6M", "P6N", "P6P", "P6Q", "P6R", "Q0Y", "Q10", "Q11",
            "Q12", "Q13", "Q14", "Q15", "Q16", "Q17", "Q18", "R8F", "R8G", "R8H", "R8J", "R8M", "R8N", "R8P", "R8Q",
            "R8R", "R8T", "R8V", "R8W", "R8Y", "R90", "R91", "R92", "R93", "R94", "R95", "R96", "R97", "R98", "R99",
            "R9C", "R9D", "R9F", "R9G", "R9H", "R9J", "R9K", "R9L", "R9M", "R9N", "R9P", "R9Q", "R9R", "R9T", "R9V",
            "RC4", "RC5", "RC6", "RC7", "RC8", "RC9", "RCC", "RCD", "RCF", "RWD", "RWF", "RWH", "RWJ", "RWL", "RWM",
            "RWP", "RWQ", "RWT", "RWV", "RWX", "RWY", "RX1", "RX2", "RX4", "RX5", "RX7", "RX8", "RXC", "RXD", "RXG",
            "RXH", "RXK", "RXL", "RXQ", "RXR", "RXT", "RXV", "RXW", "RXX", "RXY", "RY0", "RY1", "RY2", "RY3", "RY4",
            "RY5", "RY6", "RY7", "RY8", "RY9", "RYC", "RYD", "RYF", "RYG", "RYH", "RYJ", "RYK"
        };

        public static readonly Random Random = new Random();

        // Serial Generation: http://osxdaily.com/2012/01/26/reading-iphone-serial-number/
        public static string SerialGenerator()
        {
            var factoryAndMachineId = RandomNum(2);
            var maxYear = DateTime.Now.Year;
            var yearManufactured = Random.NextInclusive(5, EnsureRange(maxYear - 2010, 6, 9));
            var weekInYear = Random.NextInclusive(1, 52).ToString("D2");
            var uniqueIdentifier = RandomHex(3);

            var colorAndSize = iphone_serial_suffixes[Random.Next(iphone_serial_suffixes.Length)];
            return $"{factoryAndMachineId}{yearManufactured}{weekInYear}{uniqueIdentifier}{colorAndSize}";
        }

        // ECID Generation: https://www.theiphonewiki.com/wiki/ECID
        public static string ECIDGenerator()
        {
            return RandomHex(11);
        }

        public static string UDIDGenerator()
        {
            var adapters = AdaptersHelper.GetAdapters();

            var btMacTmp = adapters.FirstOrDefault(a => a.Name.Contains("Bluetooth"));
            var wfMacTmp = adapters.FirstOrDefault(a => a.Name.Contains("Wireless"));

            // Alg for generation here: https://www.theiphonewiki.com/wiki/UDID
            var serial = SerialGenerator();
            var ecid = ECIDGenerator();
            var bluetoothMac = btMacTmp != null ? btMacTmp.MAC.ToLower() : RandomMac().ToLower();
            var wifiMac = wfMacTmp != null ? wfMacTmp.MAC.ToLower() : RandomMac().ToLower();

            var toSha1 = $"{serial}{ecid}{bluetoothMac}{wifiMac}";
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(toSha1));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        #endregion


        #region Device

        public static string UDID
        {
            get
            {
                if (string.IsNullOrEmpty(SettingsService.Instance.UDID)) SettingsService.Instance.UDID = RandomHex(40);
                return SettingsService.Instance.UDID;
            }
        }

        public static int NextInclusive(this Random rnd, int min, int max = int.MinValue)
        {
            return max == int.MinValue ? rnd.Next(min + 1) : rnd.Next(min, max + 1);
        }

        public static string RandomHex(int length)
        {
            const string chars = "ABCDEF0123456789";
            return RandomString(chars, length);
        }

        public static string RandomMac()
        {
            return $"{RandomHex(2)}:{RandomHex(2)}:{RandomHex(2)}:{RandomHex(2)}:{RandomHex(2)}:{RandomHex(2)}";
        }

        public static string RandomNum(int length)
        {
            const string chars = "0123456789";
            return RandomString(chars, length);
        }

        public static string RandomString(string chars, int length)
        {
            var stringChars = new char[length];

            for (var i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[Random.Next(chars.Length)];

            return new string(stringChars);
        }

        public static T EnsureRange<T>(T value, T min, T max) where T : IComparable
        {
            var res = min.CompareTo(max);
            if (res == 0)
                return min;

            if (res > 0)
            {
                var temp = max;
                max = min;
                min = temp;
            }

            return value.CompareTo(min) < 0 ? min : (value.CompareTo(max) > 0 ? max : value);
        }

        // TODO: check for iPhone
        public static string FirmwareBrand = "10";

        // TODO: check for iPhone
        public static string FirmwareType = "13";

        #endregion

        #region LocationFix

        public static string Provider = "fused";

        public static string Latitude = $"{GameClient.Geoposition.Coordinate.Point.Position.Latitude}";

        public static string Longitude = $"{GameClient.Geoposition.Coordinate.Point.Position.Longitude}";

        public static string Altitude = $"{GameClient.Geoposition.Coordinate.Point.Position.Altitude}";

        // TODO: why 3? need more infos.
        public static string Floor = "3";

        // TODO: why 1? need more infos.
        public static string LocationType = "1";

        #endregion

        #region Sensors

        private static readonly Accelerometer Accelerometer = Accelerometer.GetDefault();

        private static Compass _compass = Compass.GetDefault();

        private static readonly Magnetometer Magnetometer = Magnetometer.GetDefault();

        private static readonly Gyrometer _gyrometer = Gyrometer.GetDefault();

        // TODO: if no accelerometer/compass (e.g. on desktop) what should we do?
        public static string AccelNormalizedX => $"{Accelerometer.GetCurrentReading().AccelerationX}";

        public static string AccelNormalizedY => $"{Accelerometer.GetCurrentReading().AccelerationY}";

        public static string AccelNormalizedZ => $"{Accelerometer.GetCurrentReading().AccelerationZ}";

        public static string TimestampSnapshot = ""; //(ulong)(ElapsedMilliseconds - 230L) = TimestampSinceStart - 30L

        public static string MagnetometerX => $"{Magnetometer.GetCurrentReading().MagneticFieldX}";

        public static string MagnetometerY => $"{Magnetometer.GetCurrentReading().MagneticFieldY}";

        public static string MagnetometerZ => $"{Magnetometer.GetCurrentReading().MagneticFieldZ}";

        public static string AccelRawX => $"{_gyrometer.GetCurrentReading().AngularVelocityX}";

        public static string AccelRawY => $"{_gyrometer.GetCurrentReading().AngularVelocityY}";

        public static string AccelRawZ => $"{_gyrometer.GetCurrentReading().AngularVelocityZ}";

        // TODO: missing values!
        //AngleNormalizedX = 17.950439453125,
        //AngleNormalizedY = -23.36273193359375,
        //AngleNormalizedZ = -48.8250732421875,

        //GyroscopeRawX = 7.62939453125E-05,
        //GyroscopeRawY = -0.00054931640625,
        //GyroscopeRawZ = 0.0024566650390625,

        public static string AccelerometerAxes = "3";

        #endregion

    }
}
