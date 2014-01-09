using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCP_Server_Csharp_;


namespace TCPClient_Csharp_
{
    class Client
    {
        private ISocketClient socketClient;
        private IPEndPoint remoteIpEndPoint;

        public void Initialization(string hostNameOrAdress, int port, int socketType = 0) //SocketType 0-TCP, else-UDP
        {
            IPAddress ipAddr = Dns.Resolve(hostNameOrAdress).AddressList[0];
            remoteIpEndPoint = new IPEndPoint(ipAddr, port);

            Socket cSocket = null;

            if (socketType == 0)
            {
                cSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                cSocket.ReceiveTimeout = 10000;
                socketClient = new TCPSocketClient();
            }
            else 
            {
                cSocket = new Socket(ipAddr.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socketClient = new UDPSocketClient();        

            }
            cSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

            Console.WriteLine("Local End Point: " + cSocket.LocalEndPoint);
            socketClient.Initialization(cSocket);
            socketClient.Connect(remoteIpEndPoint);
        }

        public void StartUploadUDP(string fileName)
        {
            Console.WriteLine("Start upload file UDP");
            byte[] bytes = new byte[1024];

           // socketClient.Read


            if (socketClient.Write(Encoding.UTF8.GetBytes("Connet me")) == 0)
            {
                return;
            }

            Console.WriteLine("Remote End Point: " + ((UDPSocketClient)socketClient).RemoteEndPoint);

   //         if (socketClient.Read(bytes) == 0)
   //         {
    //           return;
    //        }
 //           Console.WriteLine(((UDPSocketClient)socketClient).RemoteEndPoint.ToString());
            ((UDPSocketClient)socketClient).ConnectUDP((IPEndPoint)((UDPSocketClient)socketClient).RemoteEndPoint);
            //Console.WriteLine(Encoding.UTF8.GetString(bytes));
            StartUpload(fileName);
        }


        public void Close()
        {
            socketClient.Close();
        }

        public void StartUpload(string fileName)
        {
            Console.WriteLine("Upload start");
            int bytesRec;
            byte[] bytes = new byte[1024];
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                bytesRec = socketClient.Write(Encoding.UTF8.GetBytes(fileName));
                if (bytesRec == 0)
                {
                    throw new Exception("File name can't send"); 
                }

                Console.WriteLine(((UDPSocketClient)socketClient).RemoteEndPoint);

                bytesRec = socketClient.Read(bytes);

                if (bytesRec != 0)
                {
                    long position = BitConverter.ToInt64(bytes, 0);

                    Console.WriteLine("Start position" + position);
                    fs.Seek(position, SeekOrigin.Begin);

                    while (fs.Position != fs.Length)
                    {
                        //                      Console.WriteLine(fs.Position);
                        int realRead = fs.Read(bytes, 0, bytes.Length);
                        byte[] msg = new byte[realRead];
                        msg = bytes.Take(realRead).ToArray();
                        bytesRec = socketClient.Write(msg);
                        if (bytesRec == 0)
                        {
                            throw new Exception("Connection lost");
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Finish position" + fs.Position);
                fs.Close();
            }
        }
    }
}
