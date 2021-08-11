using System;

namespace TcpChat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.StartServer();
            Console.ReadKey();
        }
    }
}