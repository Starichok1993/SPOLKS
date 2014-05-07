using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    class TCPSocketClient: ISocketClient
    {

        private Socket cSocket;

        public void Initialization(Socket client)
        {
            cSocket = client;
        }

        public int Read(byte[] buff)
        {
            byte[] localBuff = new byte[1024];
            try
            {
                if (cSocket.Poll(1, SelectMode.SelectError))
                {
                    try
                    {
                        int MS = cSocket.ReceiveTimeout;
                        cSocket.ReceiveTimeout = 1;
                        byte[] bt = new byte[1];
                       int bytesRec = cSocket.Receive(bt, SocketFlags.OutOfBand);
                       Console.WriteLine(Encoding.UTF8.GetString(localBuff, 0, 14));
                       cSocket.ReceiveTimeout = MS;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    return -1;
                }
                return cSocket.Receive(buff);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public int Write(byte[] msg, SocketFlags fl = SocketFlags.None)
        {
            try
            {
                cSocket.Send(msg, fl);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0; 
            }
        }

        public void Close()
        {
            cSocket.Shutdown(SocketShutdown.Both);
            cSocket.Close(); 
        }
    }
}
