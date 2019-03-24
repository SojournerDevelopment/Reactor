using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Reactor.Networking.Data
{
    [Serializable]
    public class CorePacket
    {

        public string Sender { get; set; }

        public CorePacketType Type { get; set; }
        
        public List<byte[]> Data { get; set; }


        public CorePacket()
        {
            Data = new List<byte[]>();
        }

        public CorePacket(byte[] bytes)
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(bytes);

            CorePacket p = (CorePacket) b.Deserialize(ms);
            this.Sender = p.Sender;
            this.Type = p.Type;
            this.Data = p.Data;
        }



        public byte[] ToBytes()
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            b.Serialize(ms,this);
            byte[] bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }

        public override string ToString()
        {
            return "CorePacket { " + Sender + " " + Type+" }";
        }

    }
}
