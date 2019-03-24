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


    public class ReactorClient
    {
        public string Id { get; set; }
        public string IdServer { get; set; }

        protected byte[] ServerIv { get; set; }
        protected byte[] ServerPk { get; set; }
        protected Encryption ClientEncryption { get; set; }

        protected byte[] Key { get; set; }
        protected byte[] Salt { get; set; }

        protected Socket socket = null;
        protected Thread socketThread = null;

        protected IPAddress address;
        protected int port;

        public event Connected ConnectedEvent;
        public event Terminating TerminatingEvent;
        public event Disconnected DisconnectedEvent;
        public event Crashed CrashedEvent;
        public event Secured SecuredEvent;
        public event PacketReceived PacketReceivedEvent;

        public bool quitThread = false;


        public ReactorClient() { }


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
                    throw new Exception("No server found. Could not connect");
                }
            }
        }

        public void Stop()
        {
            // closes the connection and the socket + dispose + stop thread and dispose
            try
            {
                quitThread = true;
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
                socket.Dispose();
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

        public void StopGracefully()
        {
            // sends the terminate
            SendTerminate();
        }

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
                        HandlePacket(new CorePacket(buffer));
                    }
                }
            }
            catch(Exception ex)
            {
                CrashedEvent?.Invoke();
                Stop();
            }
        }

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

        protected virtual void DoPacketAuthI(CorePacket p)
        {
            this.Id = Encoding.Unicode.GetString(p.Data[0]);
            this.IdServer = Encoding.Unicode.GetString(p.Data[1]);
            this.ServerPk = p.Data[2];
            this.ServerIv = p.Data[3];
            SendAuthII();
        }

        protected virtual void DoPacketAuthIII(CorePacket p)
        {
            this.Key = ClientEncryption.Decrypt(ServerPk, p.Data[0], ServerIv);
            this.Salt = ClientEncryption.Decrypt(ServerPk, p.Data[1], ServerIv);
            SecuredEvent?.Invoke();
        }

        protected virtual void DoPacket(CorePacket p)
        {
            byte[] data = Encryption.AES_Decrypt(p.Data[0], Key, Salt);
            PacketReceivedEvent?.Invoke(data);
        }

        protected virtual void DoPacketTest(CorePacket p)
        {
            // TODO: ...?!
        }

        protected virtual void DoPacketResponse(CorePacket p)
        {
            // TODO: ...?!
        }

        protected virtual void DoPing(CorePacket p)
        {
            // TODO: ...?!
        }

        protected virtual void DoTerminatedClient(CorePacket p)
        {
            DisconnectedEvent?.Invoke();
            Stop();
        }

        #endregion


        #region Send Methods

        protected void SendAuthII()
        {
            CorePacket p = new CorePacket();
            p.Sender = this.Id;
            p.Type = CorePacketType.AuthII;
            p.Data.Add(ClientEncryption.PublicKey);
            p.Data.Add(ClientEncryption.IV);
            socket.Send(p.ToBytes());
        }
    
        protected void SendTerminate()
        {
            TerminatingEvent?.Invoke();
            CorePacket p = new CorePacket();
            p.Sender = Id;
            p.Type = CorePacketType.Terminate;
            socket.Send(p.ToBytes());
        }

        public void SendPacket(byte[] data)
        {
            CorePacket p = new CorePacket();
            p.Sender = Id;
            p.Type = CorePacketType.Packet;
            p.Data.Add(Encryption.AES_Encrypt(data, Key, Salt));
            socket.Send(p.ToBytes());
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
