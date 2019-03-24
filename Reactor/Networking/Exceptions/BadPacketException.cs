using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactor.Networking.Data;

namespace Reactor.Networking.Exceptions
{
    public class BadPacketException : Exception
    {
        public byte[] PacketData { get; set; }

        public BadPacketException(string message, byte[] data) : base(message)
        {
            this.PacketData = data;
        }
    }
}
