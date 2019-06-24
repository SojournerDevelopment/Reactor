using System;
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
                    byte[] sizeinfo = new byte[4];
                    //read the size of the message
                    int totalread = 0, currentread = 0;

                    currentread = totalread = clientSocket.Receive(sizeinfo);

                    while (totalread < sizeinfo.Length && currentread > 0)
                    {
                        currentread = clientSocket.Receive(sizeinfo,
                            totalread, //offset into the buffer
                            sizeinfo.Length - totalread, //max amount to read
                            SocketFlags.None);
                        totalread += currentread;
                    }

                    int messagesize = 0;

                    //could optionally call BitConverter.ToInt32(sizeinfo, 0);
                    messagesize |= sizeinfo[0];
                    messagesize |= (((int)sizeinfo[1]) << 8);
                    messagesize |= (((int)sizeinfo[2]) << 16);
                    messagesize |= (((int)sizeinfo[3]) << 24);

                    //create a byte array of the correct size
                    //note:  there really should be a size restriction on
                    //              messagesize because a user could send
                    //              Int32.MaxValue and cause an OutOfMemoryException
                    //              on the receiving side.  maybe consider using a short instead
                    //              or just limit the size to some reasonable value
                    byte[] data = new byte[messagesize];

                    //read the first chunk of data
                    totalread = 0;
                    currentread = totalread = clientSocket.Receive(data,
                        totalread, //offset into the buffer
                        data.Length - totalread, //max amount to read
                        SocketFlags.None);

                    //if we didn't get the entire message, read some more until we do
                    while (totalread < messagesize && currentread > 0)
                    {
                        currentread = clientSocket.Receive(data,
                            totalread, //offset into the buffer
                            data.Length - totalread, //max amount to read
                            SocketFlags.None);
                        totalread += currentread;
                    }

                    HandlePacket(data,client);
                    // buffer = new byte[clientSocket.SendBufferSize];
                    // readBytes = clientSocket.Receive(buffer);
                    
                    // byte[] packet = new byte[readBytes];

                    // Array.Copy(buffer, packet, readBytes);

                    // if (readBytes > 0)
                    // {
                    //    HandlePacket(packet,client);
                    // }
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
            byte[] sizeinfo = new byte[4];

            // could optionally call BitConverter.GetBytes(data.length);
            sizeinfo[0] = (byte)data.Length;
            sizeinfo[1] = (byte)(data.Length >> 8);
            sizeinfo[2] = (byte)(data.Length >> 16);
            sizeinfo[3] = (byte)(data.Length >> 24);

            Socket.Send(sizeinfo);
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
