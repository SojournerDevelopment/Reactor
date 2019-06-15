using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reactor.Networking.Data
{
    public class JsonPacket
    {
        public string Sender { get; set; }
        
        public string Type { get; set; }

        public Dictionary<string,string> Data { get; set; }


        public JsonPacket()
        {
            Data = new Dictionary<string, string>(); 
        }

        public JsonPacket(byte[] data)
        {
            string json = System.Text.Encoding.Unicode.GetString(data);
            var packet = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Type = packet["type"];
            Sender = packet["sender"];
            Data = JsonConvert.DeserializeObject<Dictionary<string,string>>(packet["data"]);
            Console.WriteLine("Reading: "+this.ToString());
        }

        public byte[] ToByte()
        {
            Console.WriteLine("Packing: "+this.ToString());
            Dictionary<string,string> packet = new Dictionary<string, string>();
            packet.Add("sender",Sender);
            packet.Add("type",Type);
            var json_data = JsonConvert.SerializeObject(Data);
            packet.Add("data",json_data);
            var json_packet = JsonConvert.SerializeObject(packet);
            return System.Text.Encoding.Unicode.GetBytes(json_packet);
        }

        public override string ToString()
        {
            var toPrint = "{ Data: ";
            foreach (var x in Data)
            {
                toPrint += "(" + x.Key + "->"+x.Value+")";
            }

            toPrint += " }";
            return "JSON PACKET {"+ Sender + " | "+ Type + "|" + toPrint + " } ";
        }
    }
}
