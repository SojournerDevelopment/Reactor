using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleReactorClient client = new SimpleReactorClient();
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
