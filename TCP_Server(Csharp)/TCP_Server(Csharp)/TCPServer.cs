using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    class TCPServer
    {
        private TCPListner sListner;
        private Thread sThread;
        private int queryLength;
        private int nclient = 0;

        public TCPServer(string hostNameOrAdress, int port, int qlength)
        {
            queryLength = qlength;
            sListner = new TCPListner(hostNameOrAdress, port);      //инициализация прослушивателя
            sThread = new Thread(new ThreadStart(ServerStart));     //запуск сервера в отдельном потоке
            sThread.Start();
        }

        public void Close()
        {
            sListner.Stop();
            sThread.Abort();
        }

        private void ServerStart()                      //метод-поток для обслуживания очереди подключений
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

                while ((bytesRec = cSocket.Read(bytes)) != 0)
                {
                    byte[] msg = bytes.Take(bytesRec).ToArray();
                    Console.WriteLine(fs.Position);
                    fs.Write(msg, 0, msg.Length);
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }
                
            cSocket.Close();                
            nclient--;
        }
    }
}
