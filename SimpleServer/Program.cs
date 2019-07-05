using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            
            SimpleReactorServer server = new SimpleReactorServer();
            server.Start("127.0.0.1",5001);

            bool quit = false;
            while (!quit)
            {
                Console.WriteLine("Enter q to QUIT: ");
                string x = Console.ReadLine();
                if (x == "q")
                {
                    quit = true;
                }
            }

            server.Stop();

        }
    }
}
