using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reactor.Networking.Data;

namespace SimpleJsonClient
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonClient client = new JsonClient();
            client.TransmissionType = TransmissionType.Json;
            a:
            try
            {
                client.Start(IPAddress.Parse("127.0.0.1"), 8172);
            }
            catch
            {
                Thread.Sleep(500);
                goto a;
            }
            
            string read = Console.ReadLine();
        }
    }
}
