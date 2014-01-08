using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    class UDPSocketClient: ISocketClient
    {
        private Socket cSocket;

        private EndPoint endPoint;

        public void Initialization(Socket client)
        {
            cSocket = client;
        }

        public int Read(byte[] buffer)
        {
            try
            {
                int bytesRec = cSocket.ReceiveFrom(buffer, ref endPoint);
                return bytesRec;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0; 
            }

        }

        public int Write(byte[] msg)
        {
            try
            {
                cSocket.SendTo(msg, endPoint);
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
            cSocket.Close();
        }
    }
}
