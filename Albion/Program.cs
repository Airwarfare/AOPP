using Newtonsoft.Json;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
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
            }
        }

        private static void PacketHandler(Packet packet)
        {
            try
            {
                string ip = packet.Ethernet.IpV4.Source.ToString();
                if (IPs.Contains(ip)) //Find more generic way to get the server ip's to parse from
                {
                    Parser.PacketParse(packet.Buffer);
                }
            } catch(Exception ex)
            {
                return; //Just means that the packet was null, can ignore this, (UDP anyway, not like its going to be great)
            }
        }

        
    }

    
}
