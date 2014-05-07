using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MulticastChat
{
    class Program
    {
        static void Main(string[] args)
        {
            var multicastClient = new MulticastClient("225.4.5.6", 33333);
            var listner = new Thread(multicastClient.Listen);
            listner.Start();
            Console.WriteLine("Host IP: " + multicastClient.HostIpEndPoint.Address);
            Console.WriteLine("Multicast Address: " + multicastClient.MulticastIpEndPoint.Address);
            Console.WriteLine("Port: " + multicastClient.Port);
            Console.WriteLine("\"--exit\" for exit; \"--ping\" for check online user");
            while (true)
            {
                var message = Console.ReadLine();
                if (message == "--exit")
                {
                    break;
                }

                multicastClient.Send(message);
            }
            listner.Abort();
        }
    }
}
