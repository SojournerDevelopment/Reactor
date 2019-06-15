using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Reactor.Crypto;
using Reactor.Networking.Data;
using Reactor.Networking.Exceptions;

namespace Reactor.Networking.Server
{
    
    /// <summary>
    /// Client connected to the server
    /// </summary>
    /// <param name="c">client</param>
    public delegate void ClientConnected(ReactorVirtualClient c);

    /// <summary>
    /// Client disconnected
    /// </summary>
    /// <param name="c">client</param>
    public delegate void ClientDisconnected(ReactorVirtualClient c);

    /// <summary>
    /// Client crashed
    /// </summary>
    /// <param name="c">client</param>
    public delegate void ClientCrashed(ReactorVirtualClient c);

    /// <summary>
    /// Client now secured
    /// </summary>
    /// <param name="c">client</param>
    public delegate void ClientConnectionSecured(ReactorVirtualClient c);

    /// <summary>
    /// Client sent packet
    /// </summary>
    /// <param name="c">client</param>
    /// <param name="data">data</param>
    public delegate void ClientPacketReceived(ReactorVirtualClient c,byte[] data);

    /// <summary>
    /// Reactor Server implementation
    /// </summary>
    public class ReactorServer
    {
        public static TransmissionType TransmissionType = TransmissionType.Serialized;

        /// <summary>
        /// ID
        /// </summary>
        public static string Id { get; set; }

        /// <summary>
        /// Network IP Address
        /// </summary>
        public static string Address { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public static int Port { get; set; }

        /// <summary>
        /// Dictionary - All clients
        /// </summary>
        protected static Dictionary<string, ReactorVirtualClient> Clients;

        /// <summary>
        /// Server Socket
        /// </summary>
        protected static Socket ServerSocket { get; set; }

        /// <summary>
        /// Server socket thread
        /// </summary>
        protected static Thread ServerSocketThread { get; set; }

        public static event ClientConnected ClientConnectedEvent;
        public static event ClientDisconnected ClientDisconnectedEvent;
        public static event ClientCrashed ClientCrashedEvent;
        public static event ClientConnectionSecured ClientConnectionSecuredEvent;
        public static event ClientPacketReceived ClientPacketReceivedEvent;
        
        private static bool quitServerThread = false;

        private ReactorServer() { }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="address">address</param>
        /// <param name="port">port</param>
        public static void Start(string address, int port)
        {
            try
            {
                quitServerThread = false;
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

        /// <summary>
        /// Stop the server
        /// </summary>
        public static void Stop()
        {
            try
            {
                quitServerThread = true;
                if (ServerSocket.IsBound && ServerSocket != null)
                {
                    ServerSocket.Shutdown(SocketShutdown.Both);
                    ServerSocket.Disconnect(false);
                    ServerSocket.Close();
                    ServerSocket.Dispose();
                } 
            }
            catch (Exception ex)
            {
                throw new Exception("Error while shutdown", ex);
            }
            finally
            {
                ServerSocket = null;
                ServerSocketThread = null;
            }
        }

        /// <summary>
        /// Stop the server gracefully and inform all
        /// clients.
        /// </summary>
        public static void StopGracefully()
        {
            foreach (var client in Clients)
            {
                ClientDisconnectedEvent?.Invoke(client.Value);
                if (TransmissionType == Data.TransmissionType.Serialized)
                    client.Value.SendTerminatedClient();
                else
                    client.Value.SendJsonTerminatedClient();
            }
            Stop();
        }

        /// <summary>
        /// Client wants disconnect
        /// </summary>
        /// <param name="client">client</param>
        public static void DisconnectClient(ReactorVirtualClient client)
        {
            // Send disconnect
            ClientDisconnectedEvent?.Invoke(client);
            lock (Clients)
            {
                Clients.Remove(client.Id);
            }
            if (TransmissionType == Data.TransmissionType.Serialized)
                client.SendTerminatedClient();
            else
                client.SendJsonTerminatedClient();
        }

        #region Thread Handler

        /// <summary>
        /// Listener handler
        /// </summary>
        public static void HandleListen()
        {
            while (ServerSocket.IsBound && !quitServerThread)
            {
                ServerSocket.Listen(20);
                ReactorVirtualClient client = new ReactorVirtualClient();
                client.Start(ServerSocket.Accept());
                Clients.Add(client.Id, client);
                ClientConnectedEvent?.Invoke(client);
            }
        }

        /// <summary>
        /// Handle data
        /// </summary>
        /// <param name="vc"></param>
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
                        // Interpret
                        if (TransmissionType == TransmissionType.Serialized)
                        {
                            CorePacket packet = new CorePacket(buffer);
                            HandlePacket(packet);
                        }
                        else
                        {
                            JsonPacket packet = new JsonPacket(buffer);
                            HandleJsonPacket(packet);
                        }
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

        /// <summary>
        /// Handle new Packet received
        /// </summary>
        /// <param name="p"></param>
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

        public static void HandleJsonPacket(JsonPacket p)
        {
            ReactorVirtualClient client;

            if (Clients.TryGetValue(p.Sender, out client))
            {
                switch (p.Type)
                {
                    case "AuthTwo":
                        DoJsonAuthII(p, client);
                        break;
                    case "DataPacket":
                        DoJsonPacket(p, client);
                        break;
                    case "Terminating":
                        DoJsonTerminate(p, client);
                        break;
                    default:
                        throw new BadPacketException("Unknown packet received", p.ToByte());
                }
            }
        }

        /// <summary>
        /// Received authentication packet II
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        protected static void DoAuthII(CorePacket p,ReactorVirtualClient c)
        {
            c.ClientPk = p.Data[0];
            c.ClientIv = p.Data[1];
            ClientConnectionSecuredEvent?.Invoke(c);
            c.SendAuthIII();
        }

        protected static void DoJsonAuthII(JsonPacket p, ReactorVirtualClient c)
        {
            c.ClientPk = Convert.FromBase64String(p.Data["client_pk"]);
            c.ClientIv = Convert.FromBase64String(p.Data["client_iv"]);
            ClientConnectionSecuredEvent?.Invoke(c);
            c.SendJsonAuthIII();
        }

        /// <summary>
        /// Received (symmetric encrypted) packet
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        protected static void DoPacket(CorePacket p, ReactorVirtualClient c)
        {
            // Invoke Packet Received event
            ClientPacketReceivedEvent?.Invoke(c, Encryption.AES_Decrypt(p.Data[0], c.Key, c.Salt));
        }

        protected static void DoJsonPacket(JsonPacket p, ReactorVirtualClient c)
        {
            ClientPacketReceivedEvent?.Invoke(c, Encryption.AES_Decrypt(Convert.FromBase64String(p.Data["carrier"]),c.Key,c.Salt));
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

        protected static void DoJsonTerminate(JsonPacket p, ReactorVirtualClient c)
        {
            DisconnectClient(c);
        }

        #endregion

    }
}
