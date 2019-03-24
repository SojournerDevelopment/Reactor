using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reactor.Crypto;
using Reactor.Networking.Data;
using Reactor.Networking.Exceptions;

namespace Reactor.Networking.Server
{
    public delegate void ClientConnected(ReactorVirtualClient c);

    public delegate void ClientDisconnected(ReactorVirtualClient c);

    public delegate void ClientCrashed(ReactorVirtualClient c);

    public delegate void ClientConnectionSecured(ReactorVirtualClient c);

    public delegate void ClientPacketReceived(ReactorVirtualClient c,byte[] data);


    public class ReactorServer
    {
        public static string Id { get; set; }
        public static string Address { get; set; }
        public static int Port { get; set; }

        protected static Dictionary<string, ReactorVirtualClient> Clients;

        protected static Socket ServerSocket { get; set; }
        protected static Thread ServerSocketThread { get; set; }

        public static event ClientConnected ClientConnectedEvent;
        public static event ClientDisconnected ClientDisconnectedEvent;
        public static event ClientCrashed ClientCrashedEvent;
        public static event ClientConnectionSecured ClientConnectionSecuredEvent;
        public static event ClientPacketReceived ClientPacketReceivedEvent;

        private ReactorServer() { }

        public static void Start(string address, int port)
        {
            try
            {
                Id = Identification.Value();
                Clients = new Dictionary<string, ReactorVirtualClient>();

                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

                ServerSocket.Bind(ipEndPoint);
                ServerSocketThread = new Thread(HandleListen);
                ServerSocketThread.Start();
            }
            catch(Exception ex)
            {
                throw new Exception("Could not start server",ex);
            }
        }

        public static void Stop()
        {
            try
            {
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Disconnect(false);
                ServerSocket.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while shutdown", ex);
            }
            finally
            {
                ServerSocket.Dispose();
                ServerSocketThread.Abort();
                ServerSocket = null;
                ServerSocketThread = null;
            }
        }

        public static void StopGracefully()
        {
            foreach (var client in Clients)
            {
                ClientDisconnectedEvent?.Invoke(client.Value);
                client.Value.SendTerminatedClient();
            }
            Stop();
        }

        public static void DisconnectClient(ReactorVirtualClient client)
        {
            // Send disconnect
            ClientDisconnectedEvent?.Invoke(client);
            lock (Clients)
            {
                Clients.Remove(client.Id);
            }
            client.SendTerminatedClient();
        }

        #region Thread Handler

        public static void HandleListen()
        {
            while (ServerSocket.IsBound)
            {
                ServerSocket.Listen(20);
                ReactorVirtualClient client = new ReactorVirtualClient();
                client.Start(ServerSocket.Accept());
                Clients.Add(client.Id, client);
                ClientConnectedEvent?.Invoke(client);
            }
        }

        public static void HandleData(object vc)
        {
            ReactorVirtualClient client = (ReactorVirtualClient) vc;
            Socket clientSocket = client.socket;

            try
            {
                byte[] buffer;
                int readBytes;

                while (clientSocket.Connected && !client.quitThread)
                {
                    buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(buffer);

                    if (readBytes > 0)
                    {
                        // Perform the interpretion
                        CorePacket packet = new CorePacket(buffer);
                        HandlePacket(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                // Crashed - Terminate Client + INVOKE CRASH EVENT
                //var entry = (from c in Clients where c.Value.socket == clientSocket select c).FirstOrDefault();
                //ReactorVirtualClient client = entry.Value;
                ClientCrashedEvent?.Invoke(client);
                DisconnectClient(client);
            }

        }

        #endregion

        #region Implemented Methods

        public static void HandlePacket(CorePacket p)
        {
            ReactorVirtualClient client;

            if (Clients.TryGetValue(p.Sender, out client))
            {
                switch (p.Type)
                {
                    case CorePacketType.AuthII:
                        DoAuthII(p,client);
                        break;
                    case CorePacketType.Packet:
                        DoPacket(p,client);
                        break;
                    case CorePacketType.PacketResponse:
                        DoPacketResponse(p,client);
                        break;
                    case CorePacketType.PacketTest:
                        DoPacketTest(p,client);
                        break;
                    case CorePacketType.Ping:
                        DoPing(p,client);
                        break;
                    case CorePacketType.Terminate:
                        DoTerminate(p,client);
                        break;
                    default:
                        throw new BadPacketException("Unknown packet received",p.ToBytes());
                }
            }
        }


        protected static void DoAuthII(CorePacket p,ReactorVirtualClient c)
        {
            c.ClientPk = p.Data[0];
            c.ClientIv = p.Data[1];
            ClientConnectionSecuredEvent?.Invoke(c);
            c.SendAuthIII();
        }

        protected static void DoPacket(CorePacket p, ReactorVirtualClient c)
        {
            // Invoke Packet Received event
            ClientPacketReceivedEvent?.Invoke(c, Encryption.AES_Decrypt(p.Data[0], c.Key, c.Salt));
        }
        protected static void DoPacketTest(CorePacket p, ReactorVirtualClient c)
        {
            // ...?!
        }

        protected static void DoPacketResponse(CorePacket p, ReactorVirtualClient c)
        {
            // ...?!
        }

        protected static void DoPing(CorePacket p, ReactorVirtualClient c)
        {
            // ...?!
        }

        protected static void DoTerminate(CorePacket p, ReactorVirtualClient c)
        {
            DisconnectClient(c);
        }

        #endregion

    }
}
