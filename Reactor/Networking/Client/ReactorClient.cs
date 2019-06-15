using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Reactor.Crypto;
using Reactor.Networking.Data;
using Reactor.Networking.Exceptions;

namespace Reactor.Networking.Client
{
    public delegate void Connected();

    public delegate void Terminating();

    public delegate void Disconnected();

    public delegate void Crashed();

    public delegate void Secured();

    public delegate void PacketReceived(byte[] data);

    /// <summary>
    /// ReactorClient class
    /// </summary>
    public class ReactorClient
    {
        public TransmissionType TransmissionType = TransmissionType.Serialized;
 
        /// <summary>
        /// Id of the client
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the server
        /// </summary>
        public string IdServer { get; set; }

        /// <summary>
        /// Server IV
        /// </summary>
        protected byte[] ServerIv { get; set; }

        /// <summary>
        /// Server PK
        /// </summary>
        protected byte[] ServerPk { get; set; }

        /// <summary>
        /// Encryption class instance
        /// </summary>
        protected Encryption ClientEncryption { get; set; }

        /// <summary>
        /// Key (symmetric)
        /// </summary>
        protected byte[] Key { get; set; }

        /// <summary>
        /// Salt (symmetric)
        /// </summary>
        protected byte[] Salt { get; set; }

        /// <summary>
        /// Socket
        /// </summary>
        protected Socket socket = null;

        /// <summary>
        /// Socket-Thread
        /// </summary>
        protected Thread socketThread = null;

        /// <summary>
        /// IP-Address of the connection
        /// </summary>
        protected IPAddress address;

        /// <summary>
        /// Port connected to
        /// </summary>
        protected int port;

        public event Connected ConnectedEvent;
        public event Terminating TerminatingEvent;
        public event Disconnected DisconnectedEvent;
        public event Crashed CrashedEvent;
        public event Secured SecuredEvent;
        public event PacketReceived PacketReceivedEvent;

        public bool quitThread = false;
        

        public ReactorClient() { }

        /// <summary>
        /// Connect to the desired IP over PORT
        /// </summary>
        /// <param name="ip">Internet Protocol Address</param>
        /// <param name="port">Port</param>
        public void Start(IPAddress ip, int port)
        {
            quitThread = false;
            this.address = ip;
            this.port = port;

            if (socket != null && socket.Connected)
            {
                // Already connected
                throw new Exception("Already connected");
            }

            // connect and start thread
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(address,port);
            ClientEncryption = new Encryption();

            try
            {
                socket.Connect(ipEndPoint);
                socketThread = new Thread(HandleData);
                socketThread.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("No server found. Could not connect");
            }

        }

        /// <summary>
        /// Reconnect the client
        /// </summary>
        public void Reconnect()
        {
            if (socket == null && socketThread == null)
            {
                quitThread = false;
                // connect and start thread
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(address, port);
                ClientEncryption = new Encryption();

                try
                {
                    socket.Connect(ipEndPoint);
                    socketThread = new Thread(HandleData);
                    socketThread.Start();
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
                quitThread = true;
                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(false);
                    socket.Close();
                    socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                //TODO: CATCH
                throw new Exception("Error while shutdown", ex);
            }
            finally
            {
                socket = null;
                socketThread = null;
            }

        }

        /// <summary>
        /// Stop gracefully by sending Terminate message. Use this to shut down the
        /// client.
        /// </summary>
        public void StopGracefully()
        {
            // sends the terminate
            if (TransmissionType == TransmissionType.Serialized)
            {
                SendTerminate();
            }
            else
            {
                SendJsonTerminate();
            }
            
        }


        /// <summary>
        /// Handle incomming data
        /// </summary>
        public void HandleData()
        {
            try
            {
                ConnectedEvent?.Invoke();
                byte[] buffer;
                int readBytes;

                while (socket!= null && socket.Connected && !quitThread)
                {
                    buffer = new byte[socket.SendBufferSize];
                    readBytes = socket.Receive(buffer);

                    if (readBytes > 0)
                    {
                        if (TransmissionType == TransmissionType.Serialized)
                        {
                            HandlePacket(new CorePacket(buffer));
                        }
                        else
                        {
                            HandleJsonPacket(new JsonPacket(buffer));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                CrashedEvent?.Invoke();
                Stop();
            }
        }

        private void HandleJsonPacket(JsonPacket p)
        {
            switch (p.Type)
            {
                case "AuthOne":
                    DoJsonAuthI(p);
                    break;
                case "AuthThree":
                    DoJsonAuthIII(p);
                    break;
                case "DataPacket":
                    DoJsonPacket(p);
                    break;
                case "Terminate":
                    DoJsonTerminatedClient(p);
                    break;
                default:
                    throw new BadPacketException("A unknown json packet was received.", p.ToByte());
            }
        }

        /// <summary>
        /// Packet handler wrapper
        /// </summary>
        /// <param name="p"></param>
        public void HandlePacket(CorePacket p)
        {
            switch (p.Type)
            {
                case CorePacketType.AuthI:
                    DoPacketAuthI(p);
                    break;
                case CorePacketType.AuthIII:
                    DoPacketAuthIII(p);
                    break;
                case CorePacketType.Packet:
                    DoPacket(p);
                    break;
                case CorePacketType.PacketTest:
                    DoPacketTest(p);
                    break;
                case CorePacketType.PacketResponse:
                    DoPacketResponse(p);
                    break;
                case CorePacketType.Ping:
                    DoPing(p);
                    break;
                case CorePacketType.TerminatedClient:
                    DoTerminatedClient(p);
                    break;
                default:
                    throw new BadPacketException("A unknown packet was received.",p.ToBytes());
            }
        }

        #region Implemented Handlers

        /// <summary>
        /// Receive authentication packet I
        /// </summary>
        /// <param name="p">Packet</param>
        protected virtual void DoPacketAuthI(CorePacket p)
        {
            this.Id = Encoding.Unicode.GetString(p.Data[0]);
            this.IdServer = Encoding.Unicode.GetString(p.Data[1]);
            this.ServerPk = p.Data[2];
            this.ServerIv = p.Data[3];
            SendAuthII();
        }

        protected virtual void DoJsonAuthI(JsonPacket p)
        {
            this.Id = p.Data["client_id"];
            this.IdServer = p.Data["server_hwid"];
            this.ServerPk = System.Convert.FromBase64String(p.Data["server_pk"]);
            this.ServerIv = System.Convert.FromBase64String(p.Data["server_iv"]);
            SendJsonAuthII();
        }

        /// <summary>
        /// Receive authentication packet III. Connection is now secured.
        /// </summary>
        /// <param name="p">Packet</param>
        protected virtual void DoPacketAuthIII(CorePacket p)
        {
            this.Key = ClientEncryption.Decrypt(ServerPk, p.Data[0], ServerIv);
            this.Salt = ClientEncryption.Decrypt(ServerPk, p.Data[1], ServerIv);
            SecuredEvent?.Invoke();
        }

        protected virtual void DoJsonAuthIII(JsonPacket p)
        {
            this.Key = ClientEncryption.Decrypt(ServerPk, Convert.FromBase64String(p.Data["key"]), ServerIv);
            this.Salt = ClientEncryption.Decrypt(ServerPk, Convert.FromBase64String(p.Data["salt"]), ServerIv);
            SecuredEvent?.Invoke();
        }

        /// <summary>
        /// Receive an packet (decrypted symmetrical)
        /// </summary>
        /// <param name="p"></param>
        protected virtual void DoPacket(CorePacket p)
        {
            byte[] data = Encryption.AES_Decrypt(p.Data[0], Key, Salt);
            PacketReceivedEvent?.Invoke(data);
        }

        protected virtual void DoJsonPacket(JsonPacket p)
        {
            byte[] data = Encryption.AES_Decrypt(Convert.FromBase64String(p.Data["carrier"]), Key, Salt);
            PacketReceivedEvent?.Invoke(data);
        }

        /// <summary>
        /// Received a test packet
        /// </summary>
        /// <param name="p"></param>
        protected virtual void DoPacketTest(CorePacket p)
        {
            // TODO: ...?!
        }

        /// <summary>
        /// Received a response packet
        /// </summary>
        /// <param name="p"></param>
        protected virtual void DoPacketResponse(CorePacket p)
        {
            // TODO: ...?!
        }

        /// <summary>
        /// Received a ping packet
        /// </summary>
        /// <param name="p"></param>
        protected virtual void DoPing(CorePacket p)
        {
            // TODO: ...?!
        }

        /// <summary>
        /// Terminated client
        /// </summary>
        /// <param name="p"></param>
        protected virtual void DoTerminatedClient(CorePacket p)
        {
            DisconnectedEvent?.Invoke();
            Stop();
        }

        protected virtual void DoJsonTerminatedClient(JsonPacket p)
        {
            DisconnectedEvent?.Invoke();
            Stop();
        }

        #endregion


        #region Send Methods

        /// <summary>
        /// Send authentication packet II
        /// </summary>
        protected void SendAuthII()
        {
            CorePacket p = new CorePacket();
            p.Sender = this.Id;
            p.Type = CorePacketType.AuthII;
            p.Data.Add(ClientEncryption.PublicKey);
            p.Data.Add(ClientEncryption.IV);
            socket.Send(p.ToBytes());
        }

        protected void SendJsonAuthII()
        {
            JsonPacket p = new JsonPacket();
            p.Sender = this.Id;
            p.Type = "AuthTwo";
            p.Data.Add("client_pk",Convert.ToBase64String(ClientEncryption.PublicKey));
            p.Data.Add("client_iv",Convert.ToBase64String(ClientEncryption.IV));
            socket.Send(p.ToByte());
        }
    
    
        /// <summary>
        /// Send terminate
        /// </summary>
        protected void SendTerminate()
        {
            TerminatingEvent?.Invoke();
            CorePacket p = new CorePacket();
            p.Sender = Id;
            p.Type = CorePacketType.Terminate;
            if (socket.Connected && socket != null)
            {
                socket.Send(p.ToBytes());
            }
        }

        private void SendJsonTerminate()
        {
            TerminatingEvent?.Invoke();
            JsonPacket p = new JsonPacket();
            p.Sender = Id;
            p.Type = "Terminating";
            if (socket.Connected)
                socket?.Send(p.ToByte());
        }

        /// <summary>
        /// Send packet with byte data
        /// </summary>
        /// <param name="data"></param>
        public void SendPacket(byte[] data)
        {
            CorePacket p = new CorePacket();
            p.Sender = Id;
            p.Type = CorePacketType.Packet;
            p.Data.Add(Encryption.AES_Encrypt(data, Key, Salt));
            socket.Send(p.ToBytes());
        }

        /// <summary>
        /// Send the string to byte encoded data bytes encrypted to the server
        /// </summary>
        /// <param name="data">Data</param>
        public void SendJsonPacket(byte[] data)
        {
            JsonPacket p = new JsonPacket();
            p.Sender = Id;
            p.Type = "DataPacket";
            p.Data.Add("carrier",Convert.ToBase64String(Encryption.AES_Encrypt(data,Key,Salt)));
            socket.Send(p.ToByte());
        }

        protected void SendPacketResponse()
        {
            // TODO
        }

        protected void SendPacketTest()
        {
            // TODO
        }

        protected void SendPing()
        {
            // TODO
        }

        #endregion


    }
}
