using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorClient.Secure
{
    /// <summary>
    /// ReactorSecureClient
    ///
    /// A reactor client must include the following
    /// handler functionality:
    /// 
    ///     The handle packet, must comply and accept the registration packet,
    ///     the server automatically sends to the clients.
    ///
    ///     The disconnect packet from the server must be accepted. 
    /// 
    /// </summary>
    public class ReactorSecureClient : Core.ReactorClient
    {

        public ReactorSecureClient() : base()
        {
            
        }
        
        public override void HandlePacket(byte[] data)
        {
            // Ignore
        }

        public override void SendRequestDisconnect()
        {
            // base.SendRequestDisconnect();
        }

    }
}
