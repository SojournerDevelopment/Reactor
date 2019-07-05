using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Core;

namespace SimpleServer
{
    class SimpleReactorVirtualClient : ReactorVirtualClient
    {

        public SimpleReactorVirtualClient(ReactorServer.Core.ReactorServer server) : base(server)
        {

        }

        protected override void ClientCrashed()
        {
            //
        }

        protected override string GenerateId()
        {
            return base.GenerateId();
        }

        protected override void Handle(byte[] data)
        {
            string received = Encoding.Unicode.GetString(data);
            Console.WriteLine("RECEIVED ["+Id+"]: "+received);
        }

        public override void ReportPacketSize(int size, int current)
        {
            base.ReportPacketSize(size, current);
        }

        public override void SendRegistration()
        {
            SendPacket(Encoding.Unicode.GetBytes("REG"));
        }

        public override void SendDisconnect()
        {
            SendPacket(Encoding.Unicode.GetBytes("BYE"));
        }

    }
}
