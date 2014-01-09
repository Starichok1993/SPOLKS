using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;  
using System.Threading;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    class Server
    {
        private TCPListner sListner;
        private Thread sThreadTCP;
        private Thread sThreadUDP;
        private int queryLength;
        private int nclient = 0;
        private UDPSocket udpSocket;

        public Server(string hostNameOrAdress, int tcpPort, int qlength, int udpPort)
        {
            queryLength = qlength;
            sListner = new TCPListner(hostNameOrAdress, tcpPort);      //инициализация прослушивателя
            sThreadTCP = new Thread(new ThreadStart(ServerStartTCP));     //запуск сервера в отдельном потоке
            sThreadTCP.Start();
            
            udpSocket = new UDPSocket(hostNameOrAdress, udpPort);
            udpSocket.Bind();
            udpSocket.SetReciveTimeOut(-1);
            sThreadUDP = new Thread(new ThreadStart(ServerStartUDP));
            sThreadUDP.Start();

        }

        public void Close()
        {
            sListner.Stop();
            sThreadTCP.Abort();
            udpSocket.Close();
            sThreadUDP.Abort();
        }

        private void ServerStartTCP()                      //метод-поток для обслуживания очереди подключений
        {
            Socket clientSocket;

            Console.WriteLine("Server " + sListner.GetIpEndPoint());

            sListner.Start(queryLength);               

            while (true)
            {
                Console.WriteLine("Client number \n" + nclient);
                clientSocket = sListner.AcceptSocket();             //получаем сокет клиента
                TCPSocketClient socketClient = new TCPSocketClient();
                socketClient.Initialization(clientSocket);
                nclient++;

                Thread clientThread = new Thread(ClientService);    //запускаем отдельный поток для обслуживания клиента
                clientThread.Start(socketClient);
            }                
        }

        private void ServerStartUDP()
        {
            Console.WriteLine("UDP server start");

            int bytesRec;
            byte[] bytes = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    bytesRec = udpSocket.Socket.ReceiveFrom(bytes, ref remoteEP);
                    //udpSocket.Socket.SendTo(Encoding.UTF8.GetBytes("ASK"), remoteEP);
                    //Console.WriteLine(Encoding.UTF8.GetString(bytes));
                    Console.WriteLine("Remote EP: " + remoteEP);
                    Thread clientThread = new Thread(UDPClientService);    //запускаем отдельный поток для обслуживания клиента
                    clientThread.Start(remoteEP);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    udpSocket.Close();
                    return;
                }

            }
 
        }

        private void UDPClientService(object targetEP)
        {
            Console.WriteLine("Hello");
            UDPSocket clientSocket = new UDPSocket(udpSocket.GetIpEndPoint().Address.ToString(), 0);
            clientSocket.Bind();
            clientSocket.SetReciveTimeOut(5000);
            clientSocket.Socket.SendTo(Encoding.UTF8.GetBytes("ASK"), (IPEndPoint)targetEP);

            Console.WriteLine(clientSocket.GetIpEndPoint().ToString());

            UDPSocketClient client = new UDPSocketClient();
            client.Initialization(clientSocket.Socket);            
            client.RemoteEndPoint = (IPEndPoint)targetEP;

            //client.Write(Encoding.UTF8.GetBytes(client.EndPoint.ToString()));
            
            ClientService(client);
        }

        private void ClientService(object clientSocket)             //метод-поток для обслуживания клиента
        {
            ISocketClient cSocket = (ISocketClient)clientSocket;
            int bytesRec;
            string fileName = null;
            byte[] bytes = new byte[1024];

            bytesRec = cSocket.Read(bytes);              // 1) получаем от клиента имя файла
            if (bytesRec != 0)
            {
                FileStream fs = null;
                fileName = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                Console.WriteLine(fileName);
                try
                {
                    fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    cSocket.Close();
                    nclient--;                    
                }
                
                cSocket.Write(BitConverter.GetBytes(fs.Position));                   //2) отправляем позицию в файле с которой нужно отправлять данные
               
                Console.WriteLine(fs.Position);

                while ((bytesRec = cSocket.Read(bytes)) != 0)
                {
                    byte[] msg = bytes.Take(bytesRec).ToArray();
 //                   Console.WriteLine(fs.Position);
                    fs.Write(msg, 0, msg.Length);
                }

                if (fs != null)
                {
                    Console.WriteLine("Finish position" + fs.Position);
                    fs.Close();
                }
            }
                
            cSocket.Close();                
            nclient--;
        }
    }
}
