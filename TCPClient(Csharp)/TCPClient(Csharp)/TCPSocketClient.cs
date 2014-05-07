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
            try
            {
                if (cSocket.Poll(1, SelectMode.SelectError))
                {
                    try
                    {
                        cSocket.Receive(buff, SocketFlags.OutOfBand);
                    }
                    catch(Exception ex)
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

        public void Connect(System.Net.IPEndPoint remoteEP)
        {
            try
            {
                cSocket.Connect(remoteEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
