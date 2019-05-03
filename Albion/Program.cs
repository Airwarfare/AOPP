using Newtonsoft.Json;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion
{
    class Program
    {
        static List<string> IPs = new List<string> //Automatically get ips later
        {
            "185.218.131.87",
            "185.218.131.75"
        };

        static void Main(string[] args)
        {
            List<TestClass> testClasses = JsonConvert.DeserializeObject<List<TestClass>>(File.ReadAllText(@"C:\Users\Jordan\source\repos\Albion\Albion\test.json"));
            Dictionary<string, TestClass> valuePairs = testClasses.ToDictionary(x => x.Index, x => x);
            Console.WriteLine(valuePairs["0004"].UniqueName);
            /*
            IList<LivePacketDevice> devices = LivePacketDevice.AllLocalMachine;

            if(devices.Count == 0)
            {
                Console.WriteLine("No interfaces");
                return;
            }

            PacketDevice selectedDevice = devices[1]; //Automatically get correct interface later

            using(PacketCommunicator communicator = selectedDevice.Open(65536, PacketDeviceOpenAttributes.DataTransferUdpRemote, 10000))
            {
                communicator.ReceivePackets(0, PacketHandler); //Recieve packets on interface
            }*/
        }

        private static void PacketHandler(Packet packet)
        {
            try
            {
                string ip = packet.Ethernet.IpV4.Source.ToString();
                if (Regex.Match(ip, @"(185\.218\.131\..{2})").Success) //Use regex to match the ip's
                {
                    Parser.PacketParse(packet.Buffer);
                }
            } catch(Exception ex)
            {
                return; //Just means that the packet was null, can ignore this, (UDP anyway, not like its going to be great)
            }
        }

        
    }

    class TestClass
    {
        public string Index { get; set; }
        public string UniqueName { get; set; }
    }
}
