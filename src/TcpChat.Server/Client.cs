using System.Net.Sockets;

namespace TcpChat.Server
{
    public class Client
    {
        public TcpClient TcpClient;
        public string Name;

        public Client(TcpClient tcpClient, string name)
        {
            TcpClient = tcpClient;
            Name = name;
        }
    }
}