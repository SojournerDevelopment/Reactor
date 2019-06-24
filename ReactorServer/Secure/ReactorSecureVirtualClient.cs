using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;
using ReactorServer.Utils.Aes;
using ReactorServer.Utils.Json;
using ReactorServer.Utils.Keygen;
using ReactorServer.Utils.Pem;

namespace ReactorServer.Secure
{
    public class ReactorSecureVirtualClient : ReactorVirtualClient
    {

        public ReactorSecureVirtualClient(Core.ReactorServer server) : base(server) {}

        protected byte[] SessionKey { get; set; }
        protected byte[] SessionSalt { get; set; }

        protected string fusionKey { get; set; }
        protected string fusionKeyClient { get; set; }

        #region Overrides

        protected override void ClientCrashed()
        {
            // Possible reconnect
        }

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        protected override void Handle(byte[] data)
        {
            string received = Encoding.Unicode.GetString(data);
            JsonParser parser = new JsonParser(received);

            var value = parser.parse();
            switch (value.asObject().get("reactor").asObject().get("type").asString())
            {
                case "AUTH":
                    HandleAuth(value.asObject());
                    break;
                case "DATA":
                    byte[] content =
                        Convert.FromBase64String(value.asObject().get("reactor").asObject().get("packet").asString());
                    byte[] decrypted = AesUtil.AES_Decrypt(content, this.SessionKey, this.SessionSalt);
                    HandleSecurePacket(decrypted);
                    break;
                case "MELT":
                    HandleMelt(value.asObject());
                    break;
                default:
                    // ignore
                    break;
            }
        }

        /// <summary>
        /// Sends the initial registration packet with the public key of the
        /// server to the client.
        /// </summary>
        public override void SendRegistration()
        {
            JsonObject content = new JsonObject();
            content.add("type", "REG");
            content.add("sender", Server.Id);
            content.add("receiver", this.Id);
            content.add("key",((ReactorSecureServer)Server).PublicKeyExport);

            JsonObject jsonObject = new JsonObject().add("reactor",content);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendPacket(byte_packet);
        }

        public override void SendDisconnect()
        {
            SendMelt();
        }

        #endregion

        #region HandlePackets

        protected void HandleAuth(JsonObject packet)
        {
            var sessionkey = packet.get("reactor").asObject().get("session").asString();
            var salt = packet.get("reactor").asObject().get("salt").asString();

            // Decrypt the session key
            this.SessionKey = ReactorSecureServer.Decrypt(Convert.FromBase64String(sessionkey), (ReactorSecureServer)this.Server);
            this.SessionSalt = ReactorSecureServer.Decrypt(Convert.FromBase64String(salt), (ReactorSecureServer)this.Server);
            SendFusion();
        }

        protected void HandleMelt(JsonObject packet)
        {
            string meltkey = Encoding.Unicode.GetString(AesUtil.AES_Decrypt(
                Convert.FromBase64String(packet.get("reactor").asObject().get("control").asString()), this.SessionKey,
                this.SessionSalt));
            // Disconnect the client  and send melt
            if (meltkey == fusionKey)
            {
                SendMelt();
                Stop();
            }
        }

        #endregion

        #region SendPackets

        protected void SendFusion()
        {
            // generate fusion keys
            this.fusionKey = Keygen.GetUniqueKey(25);
            this.fusionKeyClient = Keygen.GetUniqueKey(25); ;

            JsonObject core = new JsonObject();
            core.add("core", fusionKey);
            core.add("core-client", fusionKeyClient);
            var content_core = Convert.ToBase64String((AesUtil.AES_Encrypt(Encoding.Unicode.GetBytes(core.ToString()),
                this.SessionKey, this.SessionSalt)));
            
            JsonObject content = new JsonObject();
            content.add("type", "FUSION");
            content.add("sender", Server.Id);
            content.add("receiver", this.Id);
            
            content.add("content", content_core);
            
            JsonObject jsonObject = new JsonObject().add("reactor", content);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendPacket(byte_packet); 
        }

        protected void SendMelt()
        {
            JsonObject content = new JsonObject();
            content.add("type", "MELT");
            content.add("sender", Server.Id);
            content.add("receiver", this.Id);

            content.add("control",Convert.ToBase64String(AesUtil.AES_Encrypt(Encoding.Unicode.GetBytes(this.fusionKeyClient),this.SessionKey,this.SessionSalt)));

            JsonObject jsonObject = new JsonObject().add("reactor", content);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendPacket(byte_packet);
        }

        #endregion

        #region SecureClient Functions

        protected virtual void SendSecurePacket(byte[] content)
        {
            JsonObject packet = new JsonObject();
            packet.add("type", "DATA");
            packet.add("sender", Server.Id);
            packet.add("receiver", this.Id);

            packet.add("packet", Convert.ToBase64String(AesUtil.AES_Encrypt(content, this.SessionKey, this.SessionSalt)));

            JsonObject jsonObject = new JsonObject().add("reactor", packet);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendPacket(byte_packet);
        }

        protected virtual void HandleSecurePacket(byte[] content)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
