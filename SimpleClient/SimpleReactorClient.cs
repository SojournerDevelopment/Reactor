using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleClient
{
    /// <summary>
    /// A simple client that creates a connection to the server running on the
    /// local host. After securing the connection, the client will send a Message
    /// with the current time to the server.
    /// </summary>
    public class SimpleReactorClient : ReactorClient.Core.ReactorClient
    {

        public SimpleReactorClient()
        {
            base.ConnectedEvent += OnConnectedEvent;
            base.DisconnectedEvent += OnDisconnectedEvent;
            base.CrashedEvent += OnCrashedEvent;
            base.PacketReceivedEvent += OnPacketReceivedEvent;
        }

        public override void HandlePacket(byte[] data)
        {
            string packet = Encoding.Unicode.GetString(data);
            SendData(Encoding.Unicode.GetBytes(DateTime.Now.ToString()));
            //base.HandlePacket(data);
        }

        public void CrashHandle()
        {
            Stop();
            b:
            try
            {
                Console.WriteLine(DateTime.Now.ToString() + " | CLIENT =>    Reconnecting attempt....");
                Reconnect();
            }
            catch (Exception ex)
            {
                Thread.Sleep(500);
                goto b;
            }
        }
      

        private void OnSecuredEvent()
        {
            Console.WriteLine(DateTime.Now.ToString()+" | CLIENT =>    Connection Secured");
            byte[] currentTime = Encoding.Unicode.GetBytes("TEST");
            SendData(currentTime);
        }

        private void OnPacketReceivedEvent(byte[] data)
        {
            Console.WriteLine(DateTime.Now.ToString() + " | CLIENT =>    Received Packet");
        }

        private void OnCrashedEvent()
        {
            Console.WriteLine(DateTime.Now.ToString() + " | CLIENT =>    Crash....");
            CrashHandle();
        }

        private void OnDisconnectedEvent()
        {
            // Console.WriteLine(DateTime.Now.ToString() + " | CLIENT =>    Connection Secured");
        }

        private void OnConnectedEvent()
        {
            Console.WriteLine(DateTime.Now.ToString() + " | CLIENT =>    Connected to "+ServerId+"@"+Address.ToString()+":"+Port);
        }
    }
}
