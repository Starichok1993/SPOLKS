using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server_Csharp_
{
    public interface ISocketClient
    {
        void Connect(IPEndPoint remoteEP);
        void Initialization(Socket client);
        int Read(byte[] buffer);
        int Write(byte[] msg, SocketFlags fl);
        void Close();
    }
}
