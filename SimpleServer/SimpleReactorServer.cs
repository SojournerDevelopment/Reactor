using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;

namespace SimpleServer
{
    public class SimpleReactorServer : ReactorServer.Core.ReactorServer
    {

        protected override void ClientCrashed(ReactorVirtualClient client)
        {
            Console.WriteLine("Client crashed: " + client.Id);
        }

        protected override ReactorVirtualClient AcceptVirtualClient()
        {
            return new SimpleReactorVirtualClient(this);
        }

        protected override void ClientConnected(ReactorVirtualClient client)
        {
            Console.WriteLine("Client connected: "+client.Id);
        }

        protected override void ClientDisconnected(ReactorVirtualClient client)
        {
            Console.WriteLine("Client disconnected: "+client.Id);
        }

    }
}
