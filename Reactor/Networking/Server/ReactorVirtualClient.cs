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
using Reactor.Util;

namespace Reactor.Networking.Server
{
    /// <summary>
    /// Virtual client - (server side)
    /// </summary>
    public class ReactorVirtualClient
    {
        public string Id { get; set; }
        public string Address { get; set; }
        
        public object Tag { get; set; }

        public byte[] ClientIv { get; set; } 
        public byte[] ClientPk { get; set; }
        public Encryption ServerEncryption { get; set; }

        public byte[] Key { get; set; }
        public byte[] Salt { get; set; }

        public Socket socket;
        public Thread socketThread;

        // Close
        public bool quitThread = false;
        
        public ReactorVirtualClient() { }

        public void Start(Socket s)
        {
            quitThread = false;
            ServerEncryption = new Encryption();
            this.socket = s;
            this.Id = Guid.NewGuid().ToString();
            this.socketThread = new Thread(ReactorServer.HandleData);
            this.socketThread.Start(this);
            this.Address = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            if(ReactorServer.TransmissionType == TransmissionType.Serialized)
                SendAuthI();
            else
                SendJsonAuthI();
        }

        public void Stop()
        {
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
            catch(Exception ex)
            {
                throw new Exception("Error while shutdown", ex);
            }
            finally
            {
                socket = null;
                socketThread = null;
            }
        }

        #region SendFunctions

        public void SendAuthI()
        {
            CorePacket p = new CorePacket();
            p.Type = CorePacketType.AuthI;
            p.Sender = ReactorServer.Id;
            p.Data.Add(Encoding.Unicode.GetBytes(Id));                             // [0] = ID
            p.Data.Add(Encoding.Unicode.GetBytes(ReactorServer.Id));               // [1] = SERVER_HWID
            p.Data.Add(ServerEncryption.PublicKey);                                // [2] = SERVER_PK
            p.Data.Add(ServerEncryption.IV);                                       // [3] = SERVER_IV
            socket.Send(p.ToBytes());
        }

        public void SendJsonAuthI()
        {
            JsonPacket p = new JsonPacket();
            p.Type = "AuthOne";
            p.Sender = ReactorServer.Id;
            p.Data.Add("client_id",Id);
            p.Data.Add("server_hwid",ReactorServer.Id);
            p.Data.Add("server_pk",Convert.ToBase64String(ServerEncryption.PublicKey));
            p.Data.Add("server_iv", Convert.ToBase64String(ServerEncryption.IV));
            socket.Send(p.ToByte());
        }

        public void SendAuthIII()
        {
            this.Key = Encoding.Unicode.GetBytes(DataGenerator.Generate(32));
            this.Salt = Encoding.Unicode.GetBytes(DataGenerator.Generate(16));
            CorePacket p = new CorePacket();
            p.Type = CorePacketType.AuthIII;
            p.Sender = ReactorServer.Id;
            p.Data.Add(ServerEncryption.Encrypt(ClientPk, this.Key));           // [0] = PASSWORD
            p.Data.Add(ServerEncryption.Encrypt(ClientPk, this.Salt));          // [1] = SALT
            socket.Send(p.ToBytes());
        }

        public void SendJsonAuthIII()
        {
            this.Key = Encoding.Unicode.GetBytes(DataGenerator.Generate(32));
            this.Salt = Encoding.Unicode.GetBytes(DataGenerator.Generate(16));
            JsonPacket p = new JsonPacket();
            p.Type = "AuthThree";
            p.Sender = ReactorServer.Id;
            p.Data.Add("key",Convert.ToBase64String(ServerEncryption.Encrypt(ClientPk,this.Key)));
            p.Data.Add("salt",Convert.ToBase64String(ServerEncryption.Encrypt(ClientPk,this.Salt)));
            socket.Send(p.ToByte());
        }

        public void SendPacket(byte[] data)
        {
            CorePacket p = new CorePacket();
            p.Sender = ReactorServer.Id;
            p.Type = CorePacketType.Packet;
            p.Data.Add(Encryption.AES_Encrypt(data, Key, Salt));
            socket.Send(p.ToBytes());
        }

        public void SendJsonPacket(byte[] data)
        {
            JsonPacket p = new JsonPacket();
            p.Sender = ReactorServer.Id;
            p.Type = "DataPacket";
            p.Data.Add("carrier",Convert.ToBase64String(Encryption.AES_Encrypt(data,Key,Salt)));
        }


        public void SendTerminatedClient()
        {
            CorePacket p = new CorePacket();
            p.Sender = ReactorServer.Id;
            p.Type = CorePacketType.TerminatedClient;
            if (socket != null && socket.Connected)
            {
                socket.Send(p.ToBytes());
            }
            // Send the termination packet
            Stop();
        }

        public void SendJsonTerminatedClient()
        {
            JsonPacket p = new JsonPacket();
            p.Sender = ReactorServer.Id;
            p.Type = "Terminate";
            if (socket != null && socket.Connected)
                socket.Send(p.ToByte());
            Stop();
        }

        #endregion

    }
}
