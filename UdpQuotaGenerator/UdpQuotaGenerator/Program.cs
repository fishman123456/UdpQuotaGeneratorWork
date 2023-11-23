using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using UdpQuotaGenerator.Service;

namespace UdpQuotaGenerator
{
    internal class Program
    {
        // RunUdpQuotaGeneratorServer - процедура запуска сервера, работает согласно алгоритму
        static void RunUdpQuotaGeneratorServer(IPEndPoint serverEnpoint, IQuotaGenerator quotaGenerator)
        {

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            server.Bind(serverEnpoint);


            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while (true)
            {
                try
                {
                    bytesRead = server.Receive(buffer);
                    string clientEPStr = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    IPEndPoint clientEP = ParseIpEndPoint(clientEPStr);
                    server.SendTo(Encoding.UTF8.GetBytes(quotaGenerator.GetRandomQuota()), clientEP);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("server> Произошла ошибка: " + ex.Message);
                }
                
            }

        }

        // RunUdpQoutaGeneratorClient - процедура запуска клиента, работает согласно алгоритму
        static void RunUdpQoutaGeneratorClient(IPEndPoint serverEnpoint, IPEndPoint clientEndPoint)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            client.Bind(clientEndPoint);

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            bool isEnd = false;
            while (!isEnd)
            {
                Console.WriteLine("1 - Получить цитату");
                Console.WriteLine("2 - Выйти");
                string choice = Console.ReadLine();
                if (choice == "1")
                {
                    client.SendTo(Encoding.UTF8.GetBytes(clientEndPoint.ToString()), serverEnpoint);
                    bytesRead = client.Receive(buffer);
                    string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"client> Ответ от сервера : {serverResponse}");
                    Console.ReadLine();
                }
                else if (choice == "2")
                {
                    isEnd = true;
                } else
                {
                    Console.WriteLine("Некорректный ввод");
                }
                Console.Clear();
            }
        }

        // ReadAndParseIPEndPoint - вспомогательная процедура парсинга ep в виде <ip адрес>:<порт>
        static IPEndPoint ReadAndParseIPEndPoint(string message)
        {
            // 127.0.0.1:1024
            Console.Write(message);
            string str = Console.ReadLine();
            return ParseIpEndPoint(str);
        }

        static IPEndPoint ParseIpEndPoint(string epStr)
        {
            // 127.0.0.1:1024
            string[] strs = epStr.Split(":".ToCharArray());
            string ipStr = strs[0];
            int port = Convert.ToInt32(strs[1]);
            return new IPEndPoint(IPAddress.Parse(ipStr), port);
        }


        static void Main(string[] args)
        {
            // Задача: написать простой udp-сервер генератора цитат
            // который принимает входящие сообщения и отправляет в ответ случайную цитату

            // Написать простой клиент, который делает запрос к серверу и выводит ответ

            try
            {
                Console.WriteLine("1 - запустить сервер");
                Console.WriteLine("2 - запустить клиент");
                Console.Write("Ввод: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        {
                            IPEndPoint serverEndPoint = ReadAndParseIPEndPoint("Введите ip:port сервера: ");
                            RunUdpQuotaGeneratorServer(serverEndPoint, new PlugQuotaGenerator());
                        }
                        break;
                    case "2":
                        {
                            IPEndPoint serverEndPoint = ReadAndParseIPEndPoint("Введите ip:port сервера: ");
                            IPEndPoint clientEndPoint = ReadAndParseIPEndPoint("Введите ip:port клиента: ");
                            RunUdpQoutaGeneratorClient(serverEndPoint, clientEndPoint);
                        }
                        break;
                    default:
                        Console.WriteLine("Недопустимый ввод");
                        break;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
