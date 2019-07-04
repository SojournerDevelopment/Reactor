using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactorServer.Core
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


    public abstract class ReactorServer
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Network IP Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Dictionary - All clients
        /// </summary>
        protected Dictionary<string, ReactorVirtualClient> Clients;

        /// <summary>
        /// Server Socket
        /// </summary>
        protected Socket ServerSocket { get; set; }

        /// <summary>
        /// Server socket thread
        /// </summary>
        protected Thread ServerSocketThread { get; set; }

        #region Events

        public event ClientConnected ClientConnectedEvent;
        public event ClientDisconnected ClientDisconnectedEvent;

        #endregion

        /// <summary>
        /// Controls the listener thread
        /// </summary>
        private bool _quitServerThread = false;

        /// <summary>
        /// Constructor of the server
        /// </summary>
        protected ReactorServer() { }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="address">address</param>
        /// <param name="port">port</param>
        public void Start(string address, int port)
        {
            try
            {
                _quitServerThread = false;
                Clients = new Dictionary<string, ReactorVirtualClient>();

                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

                ServerSocket.Bind(ipEndPoint);
                ServerSocketThread = new Thread(HandleListen);
                ServerSocketThread.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not start server", ex);
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            try
            {
                _quitServerThread = true;
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
        public void StopGracefully()
        {
            foreach (var client in Clients)
            {
                DisconnectClient(client.Value);
                ClientDisconnected(client.Value);
                ClientDisconnectedEvent?.Invoke(client.Value);
            }
            Stop();
        }

        /// <summary>
        /// Client wants disconnect
        /// </summary>
        /// <param name="client">client</param>
        public void DisconnectClient(ReactorVirtualClient client)
        {
            // Send disconnect
            ClientDisconnectedEvent?.Invoke(client);
            lock (Clients)
            {
                Clients.Remove(client.Id);
            }
            client.SendDisconnect();
        }

        /// <summary>
        /// Listener handler
        /// </summary>
        public void HandleListen()
        {
            while (ServerSocket.IsBound && !_quitServerThread)
            {
                ServerSocket.Listen(20);
                ReactorVirtualClient client = AcceptVirtualClient();
                client.Start(ServerSocket.Accept());
                Clients.Add(client.Id, client);
                ClientConnected(client);
                ClientConnectedEvent?.Invoke(client);
            }
        }
        
        /// <summary>
        /// Add custom clients via this method, simple return your own client class
        /// </summary>
        /// <returns></returns>
        protected virtual ReactorVirtualClient AcceptVirtualClient()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when a new client connected to the server
        /// </summary>
        /// <param name="client"></param>
        protected virtual void ClientConnected(ReactorVirtualClient client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when a client disconnected to the server
        /// </summary>
        /// <param name="client"></param>
        protected virtual void ClientDisconnected(ReactorVirtualClient client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when a client has crashed
        /// </summary>
        /// <param name="client"></param>
        protected virtual void ClientCrashed(ReactorVirtualClient client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a client from the client collection, the server has
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(ReactorVirtualClient client)
        {
            lock (Clients)
            {
                Clients.Remove(client.Id);
            }
        }

    }
}
