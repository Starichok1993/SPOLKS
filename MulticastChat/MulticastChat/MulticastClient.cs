using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MulticastChat
{
    public class MulticastClient
    {
        private UdpClient udpClient;
        public IPEndPoint MulticastIpEndPoint { get; private set; }
        public IPEndPoint HostIpEndPoint { get; private set; }
        public int Port { get; private set;}

        public MulticastClient(string multicastGroup, int port)
        {
            udpClient = new UdpClient(IPAddress.Parse(multicastGroup), port);
            MulticastIpEndPoint = new IPEndPoint(IPAddress.Parse(multicastGroup), port);
            HostIpEndPoint = new IPEndPoint(IPAddress.Any, port);
            Port = port;
        }

        public void Send(string message)
        {
            udpClient.Send(message, MulticastIpEndPoint);
        }

        public void Listen()
        {
            string message = "";
            var senderIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                if (udpClient.Receive(ref message, ref senderIpEndPoint))
                {
                    if (message == "--ping")
                    {
                        udpClient.Send("", senderIpEndPoint);
                        continue;
                    }
                    Console.WriteLine(senderIpEndPoint.Address + ":" + message);
                }
            }
        }
    }
}
