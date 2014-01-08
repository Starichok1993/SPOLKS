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
                nclient++;
                Thread clientThread = new Thread(ClientService);    //запускаем отдельный поток для обслуживания клиента
                clientThread.Start(clientSocket);
            }
        }

        private void ClientService(object clientSocket)             //метод-поток для обслуживания клиента
        {
            Socket cSocket = (Socket)clientSocket;
            int bytesRec;
            string fileName = null;
            byte[] bytes = new byte[1024];

            try
            {
                bytesRec = cSocket.Receive(bytes);              // 1) получаем от клиента имя файла
                if (bytesRec != 0)
                {
                    FileStream fs = null;
                    try
                    {
                        fileName = Encoding.UTF8.GetString(bytes, 0, bytesRec);         
                        
                        fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);       
                        cSocket.Send(BitConverter.GetBytes(fs.Position));                   //2) отправляем позицию в файле с которой нужно отправлять данные

                        while ((bytesRec = cSocket.Receive(bytes)) != 0)
                        {
                            byte[] msg = bytes.Take(bytesRec).ToArray();
                            Console.WriteLine(fs.Position);
                            fs.Write(msg, 0, msg.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }
            finally
            {
                cSocket.Shutdown(SocketShutdown.Both);
                cSocket.Close();
                nclient--;
            }
        }
    }
}
