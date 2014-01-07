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
            sListner = new TCPListner(hostNameOrAdress, port);
            sThread = new Thread(new ThreadStart(ServerStart));
            sThread.Start();
        }

        public void Close()
        {
            sListner.Stop();
            sThread.Abort();
        }

        private void ServerStart()
        {
            Socket clientSocket;

            Console.WriteLine("Echo server " + sListner.GetIpEndPoint());
            
            sListner.Start(queryLength);
            while (true)
            {
                Console.WriteLine("Client number \n" + nclient);
                clientSocket = sListner.AcceptSocket();
                nclient++;
                Thread clientThread = new Thread(ClientService);
                clientThread.Start(clientSocket);
            }           
        }

        private void ClientService(object clientSocket)
        {
            Socket cSocket = (Socket) clientSocket;
            int bytesRec;
            string data;
            string fileName = null;
            byte[] bytes = new byte[1024];           

            bytesRec = cSocket.Receive(bytes);
            if (bytesRec != 0)
            {
                fileName = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                string name = fileName.Substring(0, fileName.IndexOf('\0'));
                FileStream fs = new FileStream( name, FileMode.Append, FileAccess.Write);

                cSocket.Send(BitConverter.GetBytes(fs.Position));

                while ((bytesRec = cSocket.Receive(bytes)) != 0)
                {
                    byte[] msg = bytes.Take(bytesRec).ToArray();
                    //cSocket.Send(bytes);
                    Console.WriteLine(fs.Position);
                    fs.Write(msg, 0, msg.Length);
                }
                fs.Close();
            }

            
            cSocket.Shutdown(SocketShutdown.Both);
            cSocket.Close();
            nclient--;
        }
    }
}
