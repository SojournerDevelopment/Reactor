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
    // STATIC EVENT WHICH IS CALLED WHEN A CLIENT IS RISING AN EXCEPTION AND DISCONNECTS


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
            SendAuthI();
        }

        public void Stop()
        {
            try
            {
                quitThread = true;
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
                socket.Dispose();
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

        public void SendPacket(byte[] data)
        {
            CorePacket p = new CorePacket();
            p.Sender = ReactorServer.Id;
            p.Type = CorePacketType.Packet;
            p.Data.Add(Encryption.AES_Encrypt(data, Key, Salt));
            socket.Send(p.ToBytes());
        }


        public void SendTerminatedClient()
        {
            CorePacket p = new CorePacket();
            p.Sender = ReactorServer.Id;
            p.Type = CorePacketType.TerminatedClient;
            socket.Send(p.ToBytes());
            // Send the termination packet
            Stop();
        }

        #endregion

    }
}
