using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ReactorClient.Secure;

namespace ReactorSecureClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ReactorSecureClient client = new ReactorSecureClient();
            client.ConnectedEvent += ClientOnConnectedEvent;
            client.CrashedEvent += ClientOnCrashedEvent;
            client.DisconnectedEvent += ClientOnDisconnectedEvent;
            client.TerminatingEvent += ClientOnTerminatingEvent;
            client.PacketReceivedEvent += ClientOnPacketReceivedEvent;
            client.Start(IPAddress.Parse("127.0.0.1"), 5555);

            bool quit = false;

            while (!quit)
            {
                Console.Write("");
                string input = Console.ReadLine();
            }
        }

        private static void ClientOnPacketReceivedEvent(byte[] data)
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Data Received");
            Console.WriteLine(Encoding.Unicode.GetString(data));
            Console.WriteLine("--------------------------------------------");
        }

        private static void ClientOnTerminatingEvent()
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Client Terminated");
            Console.WriteLine("--------------------------------------------");
        }

        private static void ClientOnDisconnectedEvent()
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Client Disconnected");
            Console.WriteLine("--------------------------------------------");
        }

        private static void ClientOnCrashedEvent()
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Client crashed");
            Console.WriteLine("--------------------------------------------");
        }

        private static void ClientOnConnectedEvent()
        {
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("Client connected");
            Console.WriteLine("--------------------------------------------");
        }
    }
}
