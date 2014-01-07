using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPClient_Csharp_
{
    class TCPClient
    {
        private Socket clientSocket;
        private IPEndPoint ipEndPoint;

        public TCPClient(string hostNameOrAdress, int port)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(hostNameOrAdress);
            IPAddress ipAddr = ipHost.AddressList[2];
            ipEndPoint = new IPEndPoint(ipAddr, port);
            clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Close()
        {
            clientSocket.Close();
        }

        public void StartUpload(string fileName)
        {
            Console.WriteLine("Upload start");
            int bytesRec;
            byte[] bytes = new byte[1024];
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            clientSocket.Connect(ipEndPoint);

            clientSocket.Send(Encoding.UTF8.GetBytes(fileName));

            bytesRec = clientSocket.Receive(bytes);

            if (bytesRec != 0)
            {
                long position = BitConverter.ToInt64(bytes, 0);

                Console.WriteLine("Start position" + position);
                fs.Seek(position, SeekOrigin.Begin);

                while (clientSocket.Connected && fs.Position != fs.Length)
                {
                    Console.WriteLine(fs.Position);
                    int realRead = fs.Read(bytes, 0, bytes.Length);
                    byte[] msg = new byte[realRead];
                    msg = bytes.Take(realRead).ToArray();
                    clientSocket.Send(msg);
                }

            }
            fs.Close();
        }



    }
}
