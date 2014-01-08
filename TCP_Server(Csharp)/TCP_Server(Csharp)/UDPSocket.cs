using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    class UDPSocket
    {
        public Socket Socket;
        private IPEndPoint ipEndPoint;

        public UDPSocket (string hostNameOrAdress, int port)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(hostNameOrAdress);
            IPAddress ipAddr = ipHost.AddressList[2];
            ipEndPoint = new IPEndPoint(ipAddr, port);
            Socket = new Socket(ipAddr.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Bind()
        {
            Socket.Bind(ipEndPoint);
            ipEndPoint = (IPEndPoint)Socket.LocalEndPoint;
        }

        public void Close()
        {
            Socket.Close();
        }

        public void SetReciveTimeOut(int timeOut)
        {
            Socket.ReceiveTimeout = timeOut;
        }

        public IPEndPoint GetIpEndPoint()
        {
            return ipEndPoint;
        }
    }
}
