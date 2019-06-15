using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactor.Networking.Data;

namespace Reactor.Networking.Exceptions
{
    /// <summary>
    /// BadPacket Received exception
    /// </summary>
    public class BadPacketException : Exception
    {
        /// <summary>
        /// Byte data of the packet
        /// </summary>
        public byte[] PacketData { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="data">Data</param>
        public BadPacketException(string message, byte[] data) : base(message)
        {
            this.PacketData = data;
        }
    }
}
