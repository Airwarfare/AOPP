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

        public static List<PhotonCommand> photonCommands = new List<PhotonCommand>();

        static void Main(string[] args)
        {
            List<TestClass> testClasses = JsonConvert.DeserializeObject<List<TestClass>>(File.ReadAllText(@"C:\Users\Jordan\source\repos\Albion\Albion\test.json"));
            Dictionary<string, TestClass> valuePairs = testClasses.ToDictionary(x => x.Index, x => x);
            Console.WriteLine(valuePairs["0004"].UniqueName);
            
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
                if (Regex.Match(ip, @"(185\.218\.131\..{2})").Success) //Use regex to match the ip's
                {
                    PhotonLayer photon = new PhotonLayer();
                    int offset = 42;
                    photon.PeerID = packet.Buffer.ReadUShort(ref offset, Endianity.Big);
                    photon.CrcEnabled = unchecked((sbyte)packet.Buffer.ReadByte(ref offset));
                    photon.CommandCount = unchecked((sbyte)packet.Buffer.ReadByte(ref offset));
                    photon.Timestamp = packet.Buffer.ReadUInt(ref offset, Endianity.Big);
                    photon.Challenge = packet.Buffer.ReadInt(offset, Endianity.Big);
                    offset += 4;

                    PhotonCommand[] commands = new PhotonCommand[photon.CommandCount];

                    for (int i = 0; i < photon.CommandCount; i++)
                    {
                        PhotonCommand command = new PhotonCommand();

                        command.Type = unchecked((sbyte)packet.Buffer.ReadByte(ref offset));
                        command.ChannelID = unchecked((sbyte)packet.Buffer.ReadByte(ref offset));
                        command.Flags = unchecked((sbyte)packet.Buffer.ReadByte(ref offset));
                        command.ReservedByte = unchecked((sbyte)packet.Buffer.ReadByte(ref offset));
                        command.Length = packet.Buffer.ReadInt(offset, Endianity.Big);
                        offset += 4;
                        command.ReliableSequenceNumber = packet.Buffer.ReadInt(offset, Endianity.Big);
                        offset += 4;

                        command.Data = packet.Buffer.ReadBytes(ref offset, command.Length - 12);
                        command.debug = Parser.ByteArrayToString(command.Data);
                        
                        commands[i] = command;
                        
                    }

                    photonCommands.AddRange(commands);
                    //packet.Buffer
                    //Parser.PacketParse(packet.Buffer);
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

    struct PhotonLayer
    {
        public UInt16 PeerID { get; set; }
        public sbyte CrcEnabled { get; set; } //Uint8 = sbyte
        public sbyte CommandCount { get; set; }
        public UInt32 Timestamp { get; set; }
        public Int32 Challenge { get; set; }

        public PhotonCommand[] Commands { get; set; }

        public byte[] contents { get; set; }
        public byte[] payload { get; set; }

        public string debug { get; set; }
    }

    struct PhotonCommand
    {
        public sbyte Type { get; set; }
        public sbyte ChannelID { get; set; }
        public sbyte Flags { get; set; }
        public sbyte ReservedByte { get; set; }
        public Int32 Length { get; set; }
        public Int32 ReliableSequenceNumber { get; set; }

        public byte[] Data { get; set; }

        public string debug { get; set; }
    }
}
