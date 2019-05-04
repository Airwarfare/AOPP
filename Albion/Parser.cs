using Newtonsoft.Json;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albion
{
    static class Parser
    {
        static string packetstring = ""; //Holding Values as the data comes in
        static bool packetrecieve = false; 
        public static void PacketParse(byte[] packet)
        {
            string test = ByteArrayToString(packet);
            string[] bytes = Enumerable.Range(0, test.Length).Where(x => x % 2 == 0).Select(x => test.Substring(x, 2)).ToArray(); //split in twos, shorthand for bytes
            if (bytes.Length > 95)
            {
                if (bytes[91] == "2a" && bytes[93] == "03" && packetrecieve == false) //Start Condidtion
                {
                    packetstring += System.Text.Encoding.UTF8.GetString(packet.Subsegment(101, packet.Length - 101).ToArray()); //Offset array
                    packetrecieve = true;
                }
                else if (packet.Length != 1242 && packetrecieve == true) //End Packet
                {
                    packetstring += System.Text.Encoding.UTF8.GetString(packet.Subsegment(84, packet.Length - 89).ToArray()); //Offset array
                    packetstring = TrimNonAscii(packetstring);
                    packetstring = packetstring.Remove(packetstring.Length - 1, 1);
                    packetrecieve = false;

                    Task.Run(() => PacketToJsonToDictionary());//Convert to json then to the dictionary for uploading
                }
                else if (packetrecieve) //Middle Packet
                {
                    packetstring += System.Text.Encoding.UTF8.GetString(packet.Subsegment(86, packet.Length - 86).ToArray()); //Offset array
                }
            }

        }

        //Parse Send Reliable Type from Photon Protocol
        public static void ParseSendReliableType(PhotonCommand command)
        {
            int offset = 0;
            PhotonReliableMessage message = new PhotonReliableMessage();
            message.Signature = command.Data.ReadByte(ref offset);
            message.Type = command.Data.ReadByte(ref offset);

            switch ((CommandTypes)(message.Type & 0xff))
            {
                case CommandTypes.OperationRequest:
                    message.OperationCode = command.Data.ReadByte(ref offset);
                    break;
                case CommandTypes.EventDataType:
                    message.EventCode = command.Data.ReadByte(ref offset);
                    break;
                case CommandTypes.OperationResponse:
                case CommandTypes.otherOperationResponse:
                    message.OperationCode = command.Data.ReadByte(ref offset);
                    message.OperationResponseCode = command.Data.ReadUShort(ref offset, Endianity.Big);
                    message.OperationDebugByte = command.Data.ReadByte(ref offset);
                    break;
            }
        }

        public static void ParseSendReliableFragmentType(PhotonCommand command)
        {
            int offset = 0;
            PhotonReliableFragment fragment = new PhotonReliableFragment();
            fragment.SequenceNumber = command.Data.ReadInt(ref offset, Endianity.Big);
            fragment.FragmentCount = command.Data.ReadInt(ref offset, Endianity.Big);
            fragment.FragmentNumber = command.Data.ReadInt(ref offset, Endianity.Big);
            fragment.TotalLength = command.Data.ReadInt(ref offset, Endianity.Big);
            fragment.FragmentOffset = command.Data.ReadInt(ref offset, Endianity.Big);

            fragment.Data = command.Data.ReadBytes(offset, command.Data.Length - (4 * 5));

            Console.WriteLine("FRAGMENT");
        }

        public static string TrimNonAscii(string value)
        {
            string pattern = "[^ -~]+";
            Regex reg_exp = new Regex(pattern);
            return reg_exp.Replace(value, "");
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        static void PacketToJsonToDictionary()
        {
            string[] json = new Regex(@"(?<=[}])").Split(packetstring); //Get spliter without removing '}'
            foreach (var x in json)
            {
                try
                {
                    if (x != "")
                    {
                        MarketplaceOrder order = JsonConvert.DeserializeObject<MarketplaceOrder>(x);
                        Upload.MarketOrders.Add(order.Id, order);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message); //Important JSON.NET will not output error messages unless this is here
                }
            }
            packetstring = "";
        }
    }
}
