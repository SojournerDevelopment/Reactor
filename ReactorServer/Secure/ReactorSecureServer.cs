using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;

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
        

        public ReactorSecureServer() : base() { }

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

    }
}
