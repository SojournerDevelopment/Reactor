using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;
using ReactorServer.Secure;

namespace RemoteDesktopServer.Server
{
    public class RemoteServer : ReactorSecureServer
    {
        public MainWindow mw = null;

        public RemoteServer(MainWindow mw) : base()
        {
            this.mw = mw;
        }

        protected override ReactorVirtualClient AcceptVirtualClient()
        {
            return new RemoteVirtualClient(this);
        }

        protected override void ClientConnected(ReactorVirtualClient client)
        {
            base.ClientConnected(client);
        }

        protected override void ClientCrashed(ReactorVirtualClient client)
        {
            base.ClientCrashed(client);
        }

        protected override void ClientDisconnected(ReactorVirtualClient client)
        {
            base.ClientDisconnected(client);
        }

    }
}
