using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCP_Server_Csharp_
{
    class Program
    {
        static void Main(string[] args)
        {
            Server serv = null;
            try
            {
                serv = new Server(args[0], Int32.Parse(args[1]), Int32.Parse(args[2]), Int32.Parse(args[3])); // 1) ip, 2)port TCP, 3) tcp query length, 4) udp port
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            while (true)
            {
                string command = Console.ReadLine();
                if (command == "quit")
                {
                    serv.Close();
                    return;
                }
            }
 //           serv.Close();
/*             // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(50);

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine(ipEndPoint.Address);
                    Console.WriteLine("Ожидаем соединение через порт " + ipEndPoint.Port.ToString());

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();


                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться
                    
                    byte[] bytes = new byte[1024];

                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);

                        data = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        // Показываем данные на консоли
                        Console.Write("Полученный текст: " + data + "\n\n");

                        // Отправляем ответ клиенту\
                        string reply = data;
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);

                        if (data == "q")
                        {
                            Console.WriteLine("Сервер завершил соединение с клиентом.");
                            break;
                        }
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
            */
        }
    }
}
