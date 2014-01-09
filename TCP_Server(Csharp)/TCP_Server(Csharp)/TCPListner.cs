using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    class TCPListner
    {
        private Socket serverListner;
        private IPEndPoint ipEndPoint;

        public TCPListner(string hostNameOrAdress, int port)
        {            
            IPAddress ipAddr = Dns.Resolve(hostNameOrAdress).AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddr, port);
            serverListner = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(int backlog)
        {
            serverListner.Bind(ipEndPoint);
            serverListner.Listen(backlog);
        }

        public Socket AcceptSocket()
        {
            return serverListner.Accept();
        }

        public void Stop()
        {
            serverListner.Close();
        }

        public IPEndPoint GetIpEndPoint()
        {
            return ipEndPoint;
        }

    }
}
