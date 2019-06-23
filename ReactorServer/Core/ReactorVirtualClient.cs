﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactorServer.Core
{
    /// <summary>
    /// Client crashed
    /// </summary>
    /// <param name="c">client</param>
    public delegate void ClientCrashed(ReactorVirtualClient c);

    /// <summary>
    /// Client sent packet
    /// </summary>
    /// <param name="c">client</param>
    /// <param name="data">data</param>
    public delegate void ClientPacketReceived(ReactorVirtualClient c, byte[] data);

    public abstract class ReactorVirtualClient
    {
        public string Id { get; set; }
        public string Address { get; set; }

        public object Tag { get; set; }

        public Socket Socket;
        public Thread SocketThread;

        public bool QuitThread = false;

        protected ReactorServer Server;

        public ReactorVirtualClient(ReactorServer server)
        {
            this.Server = server;
        }

        public ClientCrashed ClientCrashedEvent;
        public ClientPacketReceived ClientPacketReceivedEvent;

        /// <summary>
        /// Start the client with a socket. This method is called by the
        /// reactor server base implementation.
        /// </summary>
        /// <param name="s"></param>
        public void Start(Socket s)
        {
            QuitThread = false;
            this.Socket = s;
            this.Id = GenerateId();
            this.SocketThread = new Thread(ReactorVirtualClient.HandleData);
            this.SocketThread.Start(this);
            this.Address = (Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString();
            SendRegistration();
        }

        /// <summary>
        /// Stop a virtual client
        /// </summary>
        public void Stop()
        {
            try
            {
                QuitThread = true;
                if (Socket != null && Socket.Connected)
                {
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Disconnect(false);
                    Socket.Close();
                    Socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while shutdown", ex);
            }
            finally
            {
                Socket = null;
                SocketThread = null;
            }
        }

        #region Thread Handler

        /// <summary>
        /// Handle data
        /// </summary>
        /// <param name="vc"></param>
        public static void HandleData(object vc)
        {
            ReactorVirtualClient client = (ReactorVirtualClient)vc;
            Socket clientSocket = client.Socket;

            try
            {
                byte[] buffer;
                int readBytes;

                while (clientSocket.Connected && !client.QuitThread)
                {
                    buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(buffer);
                    byte[] packet = new byte[readBytes];
                    Array.Copy(buffer, packet, readBytes);

                    if (readBytes > 0)
                    {
                        HandlePacket(packet,client);
                    }
                }
            }
            catch (Exception ex)
            {
                client.Server.RemoveClient(client);
                client.ClientCrashed();
                client.ClientCrashedEvent?.Invoke(client);
                client.Stop();
            }
        }

        #endregion

        #region Implemented Methods

        /// <summary>
        /// Handle new Packet received
        /// </summary>
        /// <param name="p"></param>
        private static void HandlePacket(byte[] data,ReactorVirtualClient client)
        {
            lock (client)
            {
                client.Handle(data);
                client.ClientPacketReceivedEvent?.Invoke(client,data);
            }
        }

        /// <summary>
        /// Send data with this method
        /// </summary>
        /// <param name="data"></param>
        protected void SendPacket(byte[] data)
        {
            Socket.Send(data);
        }

        #endregion

        protected virtual string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        protected virtual void Handle(byte[] data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ClientCrashed()
        {
            throw new NotImplementedException();
        }

        // TODO: SEND METHODS

        public virtual void SendRegistration()
        {
            throw new NotImplementedException();
        }
        
        public virtual void SendDisconnect()
        {
            throw new NotImplementedException();
        }
        
    }
}