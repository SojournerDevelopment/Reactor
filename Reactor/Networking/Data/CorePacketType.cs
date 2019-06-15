using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reactor.Networking.Data
{

    /// <summary>
    /// CorePacket Authentication Types
    /// </summary>
    public enum CorePacketType
    {
        // -------------------------
        AuthI,
        AuthII,
        AuthIII,
        // -------------------------
        Packet,
        PacketTest,
        PacketResponse,
        // -------------------------
        Ping,
        // -------------------------
        Terminate,
        TerminatedClient
    }
}
