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
    /// <summary>
    /// ReactorSecure Server class
    /// </summary>
    public class ReactorSecureServer : Core.ReactorServer
    {
        /// <summary>
        /// A custom Dictionary, extending the clients collection by automatically
        /// converting all Clients to reactor secure virtual clients. (Example)
        /// </summary>
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

        /// <summary>
        /// RSA CryptoProvider
        /// </summary>
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
        
        /// <summary>
        /// ReactorSecureServer constructor
        /// </summary>
        public ReactorSecureServer() : base()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new VirtualClient as accept blueprint
        /// </summary>
        /// <returns></returns>
        protected override ReactorVirtualClient AcceptVirtualClient()
        {
            return new ReactorSecureVirtualClient(this);
        }

        #region Overrides

        protected override void ClientConnected(ReactorVirtualClient client)
        {
            // Client connected
        }

        protected override void ClientCrashed(ReactorVirtualClient client)
        {
            // Client crashed
        }

        protected override void ClientDisconnected(ReactorVirtualClient client)
        {
            // Client disconnected
        }

        #endregion

        /// <summary>
        /// This method helps you to encrypt data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data,ReactorSecureServer server)
        {
            return server.RSACSP.Encrypt(data,true);
        }

        /// <summary>
        /// This method helps to decrypt data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, ReactorSecureServer server)
        {
            return server.RSACSP.Decrypt(data, true);
        }

    }
}
