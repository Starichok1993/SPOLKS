using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPClient_Csharp_
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPClient client = new TCPClient("192.168.11.25", 11000);
            client.StartUpload("Puppy..mkv");
            client.Close();
        }
    }
}
