using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactorServer.Core;
using ReactorServer.Secure;
using ReactorServer.Utils.Pem;

namespace ReactorSecureServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            ReactorSecureServer server = new ReactorSecureServer();
            server.ClientConnectedEvent += ServerOnClientConnectedEvent;
            server.ClientDisconnectedEvent += ServerOnClientDisconnectedEvent;
            server.Start("127.0.0.1",5555);
            Console.WriteLine("Server started...");

            while (true)
            {
                //Console.Clear();
                Console.WriteLine("\n------------ CONNECTED CLIENTS -------------");
                foreach (var x in server.ClientDictionary)
                {
                    Console.WriteLine("Client: "+x.Key);
                }
                Console.WriteLine("\n--------------------------------------------");
                Thread.Sleep(50000);
            }
            
        }

        private static void ServerOnClientDisconnectedEvent(ReactorVirtualClient c)
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Client disconnected: " + c.Id);
            Console.WriteLine("From IP: " + c.Address);
            Console.WriteLine("--------------------------------------------");
        }

        private static void ServerOnClientConnectedEvent(ReactorVirtualClient c)
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Client connected: "+c.Id);
            Console.WriteLine("From IP: "+c.Address);
            Console.WriteLine("--------------------------------------------");
        }
    }
}
