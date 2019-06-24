using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;
using ReactorServer.Utils.Pem;

namespace ReactorServer.Secure
{
    public class ReactorSecureServer : Core.ReactorServer
    {

        public Dictionary<string, ReactorSecureVirtualClient> ClientDictionary
        {
            get
            {
                Dictionary<string, ReactorSecureVirtualClient> clients = new Dictionary<string, ReactorSecureVirtualClient>();
                foreach (var e in base.Clients)
                {
                    clients.Add(e.Key, (ReactorSecureVirtualClient)e.Value);
                }
                return clients;
            }
        }

        protected RSACryptoServiceProvider RSACSP = new RSACryptoServiceProvider(2048);

        /// <summary>
        /// The public key is exported in the X509PEM format
        /// </summary>
        public string PublicKeyExport
        {
            get { return Crypto.ExportPublicKeyToX509PEM(RSACSP); }
        }
        
        /// <summary>
        /// Thhe private key is exported in the SAPEM format
        /// </summary>
        protected string PrivateKey
        {
            get { return Crypto.ExportPrivateKeyToRSAPEM(RSACSP); }
        }
        


        public ReactorSecureServer() : base()
        {
            Id = Guid.NewGuid().ToString();
        }

        protected override ReactorVirtualClient AcceptVirtualClient()
        {
            return new ReactorSecureVirtualClient(this);
        }

        #region Overrides

        protected override void ClientConnected(ReactorVirtualClient client)
        {
            // base.ClientConnected(client);
        }

        protected override void ClientCrashed(ReactorVirtualClient client)
        {
            // base.ClientCrashed(client);
        }

        protected override void ClientDisconnected(ReactorVirtualClient client)
        {
            // base.ClientDisconnected(client);
        }

        #endregion

        public static byte[] Encrypt(byte[] data,ReactorSecureServer server)
        {
            return server.RSACSP.Encrypt(data,true);
        }

        public static byte[] Decrypt(byte[] data, ReactorSecureServer server)
        {
            return server.RSACSP.Decrypt(data, true);
        }

    }
}
