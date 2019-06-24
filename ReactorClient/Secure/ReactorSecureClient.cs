using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ReactorClient.Utils.Aes;
using ReactorClient.Utils.Json;
using ReactorClient.Utils.Keygen;
using ReactorClient.Utils.Pem;

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
        protected byte[] SessionSalt;
        protected byte[] SessionKey;

        protected string fusionKey { get; set; }
        protected string fusionKeyClient { get; set; }

        protected RSACryptoServiceProvider CryptoService;

        public ReactorSecureClient() : base()
        {
            
        }
        
        public override void HandlePacket(byte[] data)
        {
            // Handle the data, the client received
            JsonObject d = JsonObject.readFrom(Encoding.Unicode.GetString(data));
            switch (d.get("reactor").asObject().get("type").asString())
            {
                case "REG":
                    HandleReg(d.asObject());
                    break;
                case "FUSION":
                    HandleFusion(d.asObject());
                    break;
                case "MELT":
                    HandleMelt(d.asObject());
                    break;
                case "DATA":
                    // Handle data
                    var raw = d.get("reactor").asObject().get("packet").asString();
                    var decrypted = AesUtil.AES_Decrypt(Convert.FromBase64String(raw), SessionKey, SessionSalt);
                    HandleSecurePacket(decrypted);
                    break;
            }
        }

        #region Handlers

        protected void HandleReg(JsonObject o)
        {
            this.Id = o.get("reactor").asObject().get("receiver").asString();
            this.ServerId = o.get("reactor").asObject().get("sender").asString();
            this.CryptoService = Crypto.DecodeX509PublicKey(o.get("reactor").asObject().get("key").asString());
            // Send the Auth Packet
            SendAuth();
        }

        protected void HandleFusion(JsonObject o)
        {
            var data = o.get("reactor").asObject().get("content").asString();
            var decrypted = Encoding.Unicode.GetString(AesUtil.AES_Decrypt(Convert.FromBase64String(data), SessionKey, SessionSalt));
            JsonObject kvps = JsonObject.readFrom(decrypted);
            fusionKey = kvps.get("core").asString();
            fusionKeyClient = kvps.get("core-client").asString();
        }

        protected void HandleMelt(JsonObject o)
        {
            var data = o.get("reactor").asObject().get("content").asString();
            var decrypted = Encoding.Unicode.GetString(AesUtil.AES_Decrypt(Convert.FromBase64String(data), SessionKey, SessionSalt));
            if (decrypted == fusionKeyClient)
            {
                Stop();
            }
        }

        protected virtual void HandleSecurePacket(byte[] data)
        {

        }

        #endregion


        #region Sender

        protected void SendAuth()
        {
            this.SessionKey = Keygen.GetUniqueBytes(256);
            this.SessionSalt = Keygen.GetUniqueBytes(128);

            var session = Convert.ToBase64String(CryptoService.Encrypt(this.SessionKey,true));
            var salt = Convert.ToBase64String(CryptoService.Encrypt(this.SessionSalt, true));

            JsonObject content = new JsonObject();
            content.add("type", "AUTH");
            content.add("sender", Id);
            content.add("receiver", ServerId);
            content.add("session",session);
            content.add("salt",salt);

            JsonObject jsonObject = new JsonObject().add("reactor", content);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendData(byte_packet);
        }

        protected void SendMelt()
        {
            JsonObject content = new JsonObject();
            content.add("type", "MELT");
            content.add("sender", Id);
            content.add("receiver", ServerId);

            content.add("control", Convert.ToBase64String(AesUtil.AES_Encrypt(Encoding.Unicode.GetBytes(this.fusionKey), this.SessionKey, this.SessionSalt)));

            JsonObject jsonObject = new JsonObject().add("reactor", content);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendData(byte_packet);
        }

        public void SendSecurePacket(byte[] data)
        {
            JsonObject content = new JsonObject();
            content.add("type", "DATA");
            content.add("sender", Id);
            content.add("receiver", ServerId);

            content.add("packet", Convert.ToBase64String(AesUtil.AES_Encrypt(data, this.SessionKey, this.SessionSalt)));

            JsonObject jsonObject = new JsonObject().add("reactor", content);
            byte[] byte_packet = Encoding.Unicode.GetBytes(jsonObject.ToString());
            SendData(byte_packet);
        }

        #endregion

        public override void SendRequestDisconnect()
        {
            // Send the request to tell the serve
            // the client wants to close the connection
            SendMelt();
        }

        protected override void Connected()
        {
            // The client is connected to the server
        }

        protected override void Disconnected()
        {
            // Client connection is disconnected
        }
    }
}
