using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;

namespace ReactorServer.Secure
{
    public class ReactorSecureVirtualClient : ReactorVirtualClient
    {

        public ReactorSecureVirtualClient() { }

        #region Overrides

        protected override void ClientCrashed()
        {
            // Possible reconnect
        }

        protected override string GenerateId()
        {
            return base.GenerateId();
        }

        protected override void Handle(byte[] data)
        {
            string received = Encoding.Unicode.GetString(data);
            

        }

        public override void SendRegistration()
        {
            string packet = " {\r\n  \"reactor\": {\r\n    \"type\": \"REG\",\r\n    \"sender\": \"SENDER-ID\",\r\n    \"receiver\": \"RECEIVER-ID\",\r\n    \"key\": \"RSA PUBLIC KEY BASE 64\" \r\n  }\r\n} ";
            byte[] byte_packet = Encoding.Unicode.GetBytes(packet);
            SendPacket(byte_packet);
        }

        public override void SendDisconnect()
        {
            base.SendDisconnect();
        }

        #endregion

    }
}
