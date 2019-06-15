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
    /// <summary>
    /// Serializable Core Packet
    /// </summary>
    [Serializable]
    public class CorePacket
    {
        /// <summary>
        /// Sender of the packet
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Type of the packet
        /// </summary>
        public CorePacketType Type { get; set; }
        
        /// <summary>
        /// Data in the packet
        /// </summary>
        public List<byte[]> Data { get; set; }

        /// <summary>
        /// New core packet
        /// </summary>
        public CorePacket()
        {
            Data = new List<byte[]>();
        }

        /// <summary>
        /// New core packet from bytes
        /// </summary>
        /// <param name="bytes"></param>
        public CorePacket(byte[] bytes)
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(bytes);

            CorePacket p = (CorePacket) b.Deserialize(ms);
            this.Sender = p.Sender;
            this.Type = p.Type;
            this.Data = p.Data;
        }

        /// <summary>
        /// Serialize to bytes
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            BinaryFormatter b = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            b.Serialize(ms,this);
            byte[] bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }
        
        /// <summary>
        /// ToString()
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "CorePacket { " + Sender + " " + Type+" }";
        }

    }
}
