using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Windows.Networking.Connectivity;

namespace PokemonGo_UWP.Utils
{
    public static class UdidGenerator
    {
        private class AdapterInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public string Mac { get; set; }
        }

        private static class AdaptersHelper
        {
            private const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
            private const int ERROR_BUFFER_OVERFLOW = 111;
            private const int MAX_ADAPTER_NAME_LENGTH = 256;
            private const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
            private const int MIB_IF_TYPE_OTHER = 1;
            private const int MIB_IF_TYPE_ETHERNET = 6;
            private const int MIB_IF_TYPE_TOKENRING = 9;
            private const int MIB_IF_TYPE_FDDI = 15;
            private const int MIB_IF_TYPE_PPP = 23;
            private const int MIB_IF_TYPE_LOOPBACK = 24;
            private const int MIB_IF_TYPE_SLIP = 28;

            [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
            private static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            private struct IpAdapterInfo
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
                public IpAddrString IpAddressList;
                public IpAddrString GatewayList;
                public IpAddrString DhcpServer;
                public bool HaveWins;
                public IpAddrString PrimaryWinsServer;
                public IpAddrString SecondaryWinsServer;
                public Int32 LeaseObtained;
                public Int32 LeaseExpires;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            private struct IpAddrString
            {
                public IntPtr Next;
                public IpAddressString IpAddress;
                public IpAddressString IpMask;
                public Int32 Context;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            private struct IpAddressString
            {
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
                public string Address;
            }


            /// <summary>Gets the network adapters</summary>
            /// <returns>List of network adapters</returns>
            /// <exception cref="System.InvalidOperationException">GetAdaptersInfo failed:  + ret</exception>
            public static List<AdapterInfo> GetAdapters()
            {
                var adapters = new List<AdapterInfo>();

                long structSize = Marshal.SizeOf(typeof(IpAdapterInfo));
                var pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

                var ret = GetAdaptersInfo(pArray, ref structSize);

                if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
                {
                    // Buffer was too small, reallocate the correct size for the buffer.
                    pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                    ret = GetAdaptersInfo(pArray, ref structSize);
                }

                if (ret == 0)
                {
                    // Call Succeeded
                    var pEntry = pArray;

                    do
                    {
                        var adapter = new AdapterInfo();

                        // Retrieve the adapter info from the memory address
                        var entry = (IpAdapterInfo)Marshal.PtrToStructure(pEntry, typeof(IpAdapterInfo));

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
                        adapter.Mac = string.Join(":", Enumerable.Range(0, (int)entry.AddressLength).Select(s => string.Format("{0:X2}", entry.Address[s])));

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

        }


        // Suffixes for IPhones from:
        // https://pikeralpha.wordpress.com/2013/10/10/apple-serial-numbers-ending-with-f000-fzzz/
        // https://pikeralpha.wordpress.com/2014/06/11/apple-serial-numbers-ending-with-g000-gzzz/
        private static readonly string[] IphoneSerialSuffixes = {
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

        // Serial Generation: http://osxdaily.com/2012/01/26/reading-iphone-serial-number/
        private static string SerialGenerator()
        {


            var factoryAndMachineId = Utilities.RandomNum(2);
            var maxYear = DateTime.Now.Year;
            var yearManufactured = Utilities.Rand.NextInclusive(5, Utilities.EnsureRange(maxYear - 2010, 6, 9));
            var weekInYear = Utilities.Rand.NextInclusive(1, 52).ToString("D2");
            var uniqueIdentifier = Utilities.RandomHex(3);

            var len = IphoneSerialSuffixes.Length;
            var randSuffixIndex = Utilities.Rand.Next(len);
            var colorAndSize = IphoneSerialSuffixes[randSuffixIndex];

            return $"{factoryAndMachineId}{yearManufactured}{weekInYear}{uniqueIdentifier}{colorAndSize}";
        }

        // ECID Generation: https://www.theiphonewiki.com/wiki/ECID
        private static string EcidGenerator() =>
            Utilities.RandomHex(11);

        private static string RandomMac() =>
            $"{Utilities.RandomHex(2)}:{Utilities.RandomHex(2)}:{Utilities.RandomHex(2)}:{Utilities.RandomHex(2)}:{Utilities.RandomHex(2)}:{Utilities.RandomHex(2)}";

        /// <summary>Generates UDID via this algorithm: https://www.theiphonewiki.com/wiki/UDID</summary>
        /// <returns>UDID</returns>
        public static string GenerateUdid()
        {
            var adapters = AdaptersHelper.GetAdapters();

            var btMacTmp = adapters.FirstOrDefault(a => a.Name.Contains("Bluetooth"));
            var profile = NetworkInformation.GetInternetConnectionProfile();

            var serial = SerialGenerator();
            var ecid = EcidGenerator();
            var bluetoothMac = btMacTmp != null ? btMacTmp.Mac.ToLower() : RandomMac().ToLower();
            // var wifiMac = wfMacTmp != null ? wfMacTmp.MAC.ToLower() : RandomMac().ToLower();
            var wifiMac = profile?.NetworkAdapter.NetworkAdapterId.ToString() ?? RandomMac().ToLower();

            var toSha1 = $"{serial}{ecid}{bluetoothMac}{wifiMac}";
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(toSha1));
                var sb = new StringBuilder(hash.Length * 2);

                // can be "x2" if you want lowercase
                foreach (var b in hash)
                    sb.Append(b.ToString("X2"));

                return sb.ToString();
            }
        }
    }
}