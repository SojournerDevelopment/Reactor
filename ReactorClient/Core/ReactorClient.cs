using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactorClient.Core
{
    public delegate void Connected();

    public delegate void Terminating();

    public delegate void Disconnected();

    public delegate void Crashed();

    public delegate void PacketReceived(byte[] data);


    public abstract class ReactorClient
    {
        /// <summary>
        /// Id of the client
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the server
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// Socket
        /// </summary>
        protected Socket Socket = null;

        /// <summary>
        /// Socket-Thread
        /// </summary>
        protected Thread SocketThread = null;

        /// <summary>
        /// IP-Address of the connection
        /// </summary>
        protected IPAddress Address;

        /// <summary>
        /// Port connected to
        /// </summary>
        protected int Port;

        public event Connected ConnectedEvent;
        public event Terminating TerminatingEvent;
        public event Disconnected DisconnectedEvent;
        public event Crashed CrashedEvent;
        public event PacketReceived PacketReceivedEvent;

        protected bool QuitThread = false;

        protected ReactorClient() { }

        /// <summary>
        /// Connect to the desired IP over PORT
        /// </summary>
        /// <param name="ip">Internet Protocol Address</param>
        /// <param name="port">Port</param>
        public void Start(IPAddress ip, int port)
        {
            QuitThread = false;
            this.Address = ip;
            this.Port = port;

            if (Socket != null && Socket.Connected)
            {
                // Already connected
                throw new Exception("Already connected");
            }

            // connect and start thread
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(Address, port);

            try
            {
                Socket.Connect(ipEndPoint);
                SocketThread = new Thread(HandleData);
                SocketThread.Start();
            }
            catch (Exception ex)
            {
                Disconnected();
                DisconnectedEvent?.Invoke();
                throw new Exception("No server found. Could not connect");
            }

        }

        /// <summary>
        /// Reconnect the client
        /// </summary>
        public void Reconnect()
        {
            if (Socket == null && SocketThread == null)
            {
                QuitThread = false;
                // connect and start thread
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(Address, Port);

                try
                {
                    Socket.Connect(ipEndPoint);
                    SocketThread = new Thread(HandleData);
                    SocketThread.Start();
                }
                catch (Exception ex)
                {
                    Stop();
                    throw new Exception("No server found. Could not connect");
                }
            }
        }

        /// <summary>
        /// Stop the client
        /// </summary>
        public void Stop()
        {
            // closes the connection and the socket + dispose + stop thread and dispose
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
                //TODO: CATCH
                throw new Exception("Error while shutdown", ex);
            }
            finally
            {
                Socket = null;
                SocketThread = null;
            }

        }

        /// <summary>
        /// Stop gracefully by sending Terminate message. Use this to shut down the
        /// client.
        /// </summary>
        public void StopGracefully()
        {
            // sends the terminate
            SendRequestDisconnect();
        }

        /// <summary>
        /// Handle incomming data
        /// </summary>
        public void HandleData()
        {
            try
            {
                ConnectedEvent?.Invoke();
                Connected();
                byte[] buffer;
                int readBytes;

                while (Socket != null && Socket.Connected && !QuitThread)
                {
                    byte[] sizeinfo = new byte[4];
                    //read the size of the message
                    int totalread = 0, currentread = 0;

                    currentread = totalread = Socket.Receive(sizeinfo);

                    while (totalread < sizeinfo.Length && currentread > 0)
                    {
                        currentread = Socket.Receive(sizeinfo,
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
                    currentread = totalread = Socket.Receive(data,
                        totalread, //offset into the buffer
                        data.Length - totalread, //max amount to read
                        SocketFlags.None);

                    //if we didn't get the entire message, read some more until we do
                    while (totalread < messagesize && currentread > 0)
                    {
                        currentread = Socket.Receive(data,
                            totalread, //offset into the buffer
                            data.Length - totalread, //max amount to read
                            SocketFlags.None);
                        totalread += currentread;
                    }

                    HandlePacket(data);
                    PacketReceivedEvent?.Invoke(data);

                    // buffer = new byte[Socket.SendBufferSize];
                    //readBytes = Socket.Receive(buffer);
                    //byte[] packet = new byte[readBytes];
                    //Array.Copy(buffer,packet,readBytes);

                    //if (readBytes > 0)
                    //{
                    //    HandlePacket(packet);
                    //    PacketReceivedEvent?.Invoke(packet);
                    //}
                }
            }
            catch (Exception ex)
            {
                CrashedEvent?.Invoke();
                Stop();
            }
        }

        /// <summary>
        /// Packet handler wrapper
        /// </summary>
        /// <param name="data"></param>
        public virtual void HandlePacket(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data)
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

        /// <summary>
        /// Send the request disconnect packet (alias: RequestMelt)
        /// </summary>
        public virtual void SendRequestDisconnect() { }

        protected virtual void Connected() { }

        protected virtual void Disconnected() { }

    }



}
