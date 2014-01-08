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
            try
            {
                TCPClient client = new TCPClient();
                client.Initialization("192.168.11.25", 12000, 1);
                client.StartUploadUDP("Puppy..mkv");
                //client.StartUpload("Puppy..mkv");
                client.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
