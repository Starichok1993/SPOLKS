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

        private byte packageNumberWrite = (byte)0;

        private byte packageNumberRead = (byte)255;

        private EndPoint endPoint;

        public EndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                endPoint = value;
            }
        }

        public void Initialization(Socket client)
        {
            cSocket = client;
        }

        public void Connect(IPEndPoint remoteEP)
        {
            endPoint = remoteEP;
  //          cSocket.Connect(endPoint);
        }

        public int Read(byte[] buffer)
        {
            byte[] answer = new byte[1025];
            int answerLength = 0;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    answerLength = cSocket.ReceiveFrom(answer, ref endPoint);
                    byte currentPackageNumber = answer[0];
                    if (currentPackageNumber == packageNumberRead)
                    {
                        cSocket.SendTo(Encoding.UTF8.GetBytes("ASK"), endPoint);
                        continue;
                    }
                    if (currentPackageNumber != packageNumberRead)
                    {
                        cSocket.SendTo(Encoding.UTF8.GetBytes("ASK"), endPoint);
                        packageNumberRead++;
                        answer.Skip(1).ToArray().CopyTo(buffer,0);
                        return answerLength - 1;
                    }
                    
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return 0;

        }

        public int Write(byte[] msg)
        {
            byte[] package = new byte[msg.Length + 1];
            package[0] = packageNumberWrite;
            msg.CopyTo(package, 1);
            
            byte[] answer = new byte[1025];
            int answerLength = 0;

            for (int i = 0; i < 5; i++)
            {
                try
                {                  
                    cSocket.SendTo( package, endPoint);
                    answerLength = cSocket.ReceiveFrom(answer, ref endPoint);
                    if (answerLength != 3 || Encoding.UTF8.GetString(answer, 0, 3) != "ASK" )
                    {
                        continue;
                    }

                    if (packageNumberWrite < byte.MaxValue)
                    {
                        packageNumberWrite++;
                    }
                    else
                    {
                        packageNumberWrite = 0;
                    }

                    return 1;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return 0;
        }

        public void Close()
        {
            cSocket.Close();
        }
    }
}
