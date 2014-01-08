using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    interface ISocketClient
    {
        private Socket cSocket;

        public void Initialization(Socket client);
        public int Read(byte[] buffer);
        public int Write(byte[] msg);
        public void Close();
    }
}
