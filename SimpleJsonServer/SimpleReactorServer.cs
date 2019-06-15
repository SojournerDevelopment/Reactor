using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactor.Networking.Data;
using Reactor.Networking.Server;
using Reactor.Util;

namespace SimpleJsonServer
{
    class SimpleReactorServer
    {

        public SimpleReactorServer()
        {
            ReactorServer.TransmissionType = TransmissionType.Json;

            ReactorServer.ClientConnectedEvent += ReactorServerOnClientConnectedEvent;
            ReactorServer.ClientDisconnectedEvent += ReactorServerOnClientDisconnectedEvent;
            ReactorServer.ClientCrashedEvent += ReactorServerOnClientCrashedEvent;
            ReactorServer.ClientConnectionSecuredEvent += ReactorServerOnClientConnectionSecuredEvent;
            ReactorServer.ClientPacketReceivedEvent += ReactorServerOnClientPacketReceivedEvent;
        }

        public void Start()
        {
            ReactorServer.Start(IPTool.GetLocalIPAddress(), 8172);
        }

        public void Stop()
        {
            ReactorServer.StopGracefully();
        }


        private void ReactorServerOnClientPacketReceivedEvent(ReactorVirtualClient c, byte[] data)
        {
            // Received data from client
            string time = Encoding.Unicode.GetString(data);
            Console.WriteLine(DateTime.Now.ToString() + " | SERVER =>    Received:" + time);
        }

        private void ReactorServerOnClientConnectionSecuredEvent(ReactorVirtualClient c)
        {
            Console.WriteLine(DateTime.Now.ToString() + " | SERVER =>    Connection secured to " + c.Address);
        }

        private void ReactorServerOnClientCrashedEvent(ReactorVirtualClient c)
        {
            Console.WriteLine(DateTime.Now.ToString() + " | SERVER =>    Client Crashed - Connection reset " + c.Id);
        }

        private void ReactorServerOnClientDisconnectedEvent(ReactorVirtualClient c)
        {
            Console.WriteLine(DateTime.Now.ToString() + " | SERVER =>    Client Disconnected - " + c.Id);
        }

        private void ReactorServerOnClientConnectedEvent(ReactorVirtualClient c)
        {
            Console.WriteLine(DateTime.Now.ToString() + " | SERVER =>    Client Connected - " + c.Id);
        }

    }
}
