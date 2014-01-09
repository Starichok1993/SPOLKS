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
                Client client = new Client();
                client.Initialization(args[0], Int32.Parse(args[1]), Int32.Parse(args[3]));
                if (Int32.Parse(args[3]) != 0)
                {
                    client.StartUploadUDP(args[2]);
                }
                else
                {
                    client.StartUpload("Puppy..mkv");
                }
                client.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
