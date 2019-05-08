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
        public static Dictionary<string, Cluster> valuePairs = new Dictionary<string, Cluster>();

        static void Main(string[] args)
        {
            valuePairs = JsonConvert.DeserializeObject<List<Cluster>>(File.ReadAllText(@"C:\Users\Jordan\source\repos\XMLParse\XMLParse\world.json")).ToDictionary(x => x.ID, x => x);

            Task.Run(async () =>
            {
                while (true)
                {
                    Upload.UploadPings();
                    await Task.Delay(1000);
                }
            });


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
                    //Photon Layer parsing
                    PhotonLayer photon = new PhotonLayer();
                    int offset = 42;
                    photon.PeerID = packet.Buffer.ReadUShort(ref offset, Endianity.Big);
                    photon.CrcEnabled = packet.Buffer.ReadByte(ref offset);
                    photon.CommandCount = packet.Buffer.ReadByte(ref offset);
                    photon.Timestamp = packet.Buffer.ReadUInt(ref offset, Endianity.Big);
                    photon.Challenge = packet.Buffer.ReadInt(offset, Endianity.Big);
                    offset += 4;

                    //Get the amount of commands and then parse through them to send to the correct parser
                    PhotonCommand[] commands = new PhotonCommand[photon.CommandCount];

                    for (int i = 0; i < photon.CommandCount; i++)
                    {
                        PhotonCommand command = new PhotonCommand();

                        command.Type = packet.Buffer.ReadByte(ref offset);
                        command.ChannelID = packet.Buffer.ReadByte(ref offset);
                        command.Flags = packet.Buffer.ReadByte(ref offset);
                        command.ReservedByte = packet.Buffer.ReadByte(ref offset);
                        command.Length = packet.Buffer.ReadInt(offset, Endianity.Big);
                        offset += 4;
                        command.ReliableSequenceNumber = packet.Buffer.ReadInt(offset, Endianity.Big);
                        offset += 4;

                        command.Data = packet.Buffer.ReadBytes(ref offset, command.Length - 12);
                        command.debug = Parser.ByteArrayToString(command.Data); //Just string version of data (easy debug) REMOVE LATER

                        //Get Command Type for parsing
                        if ((CommandTypes)(command.Type & 0xff) == CommandTypes.SendReliableType)
                        {
                            commands[i] = command;
                            Parser.ParseSendReliableType(command);
                            continue;
                        }
                        if((CommandTypes)(command.Type & 0xff) == CommandTypes.SendReliableFragmentType)
                        {
                            commands[i] = command;
                            Parser.ParseSendReliableFragmentType(command);
                            continue;
                        }
                        
                        
                        commands[i] = command;
                        
                    }
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
        public byte CrcEnabled { get; set; } //Uint8 = sbyte
        public byte CommandCount { get; set; }
        public UInt32 Timestamp { get; set; }
        public Int32 Challenge { get; set; }

        public PhotonCommand[] Commands { get; set; }

        public byte[] contents { get; set; }
        public byte[] payload { get; set; }

        public string debug { get; set; }
    }

    struct PhotonCommand
    {
        public byte Type { get; set; }
        public byte ChannelID { get; set; }
        public byte Flags { get; set; }
        public byte ReservedByte { get; set; }
        public Int32 Length { get; set; }
        public Int32 ReliableSequenceNumber { get; set; }

        public byte[] Data { get; set; }

        public string debug { get; set; }
    }

    struct PhotonReliableMessage
    {
        public byte Signature { get; set; }
        public byte Type { get; set; }
        public byte OperationCode { get; set; }
        public byte EventCode { get; set; }
        public UInt16 OperationResponseCode { get; set; }
        public byte OperationDebugByte { get; set; }
        public Int16 ParamaterCount { get; set; }
        public byte[] Data { get; set; }
    }

    enum CommandTypes
    {
        AcknowledgeType = 1,
        ConnectType = 2,
        VerifyConnectType = 3,
        DisconnectType = 4,
        PingType = 5,
        SendReliableType = 6,
        SendUnreliableType = 7,
        SendReliableFragmentType = 8,
        OperationRequest = 2,
        otherOperationResponse = 3,
        EventDataType = 4,
        OperationResponse = 7
    }

    struct PhotonReliableFragment
    {
        public Int32 SequenceNumber { get; set; }
        public Int32 FragmentCount { get; set; }
        public Int32 FragmentNumber { get; set; }
        public Int32 TotalLength { get; set; }
        public Int32 FragmentOffset { get; set; }

        public byte[] Data { get; set; }
    }
}

public static class X
{
    //Extension Class to make this easier (because it was messy)
    public static int ReadInt(this byte[] ob, ref int offset, Endianity endianity)
    {
        int a = ob.ReadInt(offset, endianity);
        offset += 4;
        return a;
    }
}
