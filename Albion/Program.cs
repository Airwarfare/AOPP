using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using Newtonsoft.Json;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Albion
{
    class Program
    {
        static List<string> IPs = new List<string>
        {
            "185.218.131.87",
            "185.218.131.75"
        };

        static string packetstring = "";
        static bool packetrecieve = false;

        static Dictionary<Int64, MarketplaceOrder> MarketOrders = new Dictionary<long, MarketplaceOrder>();

        static string[] previouspacket;
        static string[] currentpacket;

        static string previousstring;
        static string currentstring;

        [STAThread]
        static void Main(string[] args)
        {

            
            IList<LivePacketDevice> devices = LivePacketDevice.AllLocalMachine;

            if(devices.Count == 0)
            {
                Console.WriteLine("No interfaces");
                return;
            }

            PacketDevice selectedDevice = devices[1];

            using(PacketCommunicator communicator = selectedDevice.Open(65536, PacketDeviceOpenAttributes.DataTransferUdpRemote, 10000))
            {
                communicator.ReceivePackets(0, PacketHandler);
            }
        }

        private static void PacketHandler(Packet packet)
        {
            try
            {
                string ip = packet.Ethernet.IpV4.Source.ToString();
                if (IPs.Contains(ip))
                {
                    PacketParse(packet.Buffer);
                }
            } catch(Exception ex)
            {
                return;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static void PacketParse(byte[] packet)
        {
            string test = ByteArrayToString(packet);
            string[] bytes = Enumerable.Range(0, test.Length).Where(x => x % 2 == 0).Select(x => test.Substring(x, 2)).ToArray();
            previouspacket = currentpacket;
            currentpacket = bytes;   
            if (bytes.Length > 95)
            {
                int l = bytes.Length - 1;
                if (bytes[91] == "2a" && bytes[93] == "03" && packetrecieve == false)
                { 
                    packetstring += System.Text.Encoding.UTF8.GetString(packet.Subsegment(101, packet.Length - 101).ToArray());
                    packetrecieve = true;
                    
                }
                else if(packet.Length != 1242 && packetrecieve == true)
                {
                    packetstring += System.Text.Encoding.UTF8.GetString(packet.Subsegment(84, packet.Length - 89).ToArray());
                    packetstring = TrimNonAscii(packetstring);
                    //packetstring = packetstring.Replace("}", "},");
                    packetstring = packetstring.Remove(packetstring.Length - 1, 1);
                    packetrecieve = false;
                    Task.Run(() =>
                    {
                        string[] json = new Regex(@"(?<=[}])").Split(packetstring);
                        foreach (var x in json)
                        {
                            try
                            {
                                if (x != "")
                                {
                                    MarketplaceOrder order = JsonConvert.DeserializeObject<MarketplaceOrder>(x);
                                    MarketOrders.Add(order.Id, order);
                                }
                            } catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            Console.WriteLine("??");
                        }
                        packetstring = "";
                    });

                } else if(packetrecieve)
                {
                    packetstring += System.Text.Encoding.UTF8.GetString(packet.Subsegment(86, packet.Length - 86).ToArray());
                }
            }

        }

        static string TrimNonAscii(string value)
        {
            string pattern = "[^ -~]+";
            Regex reg_exp = new Regex(pattern);
            return reg_exp.Replace(value, "");
        }

        
    }

    class MarketplaceOrder
    {
        public Int64 Id { get; set; }
        public Int64 UnitPriceSilver { get; set; }
        public Int64 TotalPriceSilver { get; set; }
        public int Amount { get; set; }
        public int Tier { get; set; }
        public bool IsFinished { get; set; }
        public string AuctionType { get; set; }
        public bool HasBuyerFetched { get; set; }
        public bool HasSellerFetched { get; set; }
        public string SellerCharacterId { get; set; }
        public string SellerName { get; set; }
        public object BuyerCharacterId { get; set; }
        public object BuyerName { get; set; }
        public string ItemTypeId { get; set; }
        public string ItemGroupTypeId { get; set; }
        public int EnchantmentLevel { get; set; }
        public int QualityLevel { get; set; }
        public DateTime Expires { get; set; }

    }
}
