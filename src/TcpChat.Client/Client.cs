using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpChat.Client
{
    public class Client
    {
        private TcpClient _tcpClient;
        private IPAddress _serverIp;
        private readonly int _serverPort;

        public Client(int port)
        {
            _serverPort = port;
        }

        public bool SetServerIP(string ipStr)
        {
            if (!IPAddress.TryParse(ipStr, out IPAddress ipAddress))
            {
                Console.WriteLine("Invalid IP!");
                return false;
            }

            _serverIp = ipAddress;
            return true;
        }

        public async Task ConnectToServer(string name)
        {
            _tcpClient ??= new TcpClient();

            try
            {
                await _tcpClient.ConnectAsync(_serverIp, _serverPort);

                Console.WriteLine($"Connected to server {_serverIp.ToString()}:{_serverPort}. Type <EXIT> to exit.");
                var connectionString = $"!new_connection-{name}";
                var streamWriter = new StreamWriter(_tcpClient.GetStream())
                {
                    AutoFlush = true
                };

                await streamWriter.WriteAsync(connectionString);
                ReadDataAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while trying to connect the server: {e.Message}");
            }
        }

        private async Task ReadDataAsync()
        {
            try
            {
                var reader = new StreamReader(_tcpClient.GetStream());
                var buffer = new char[64];

                while (true)
                {
                    var receivedByteCount = await reader.ReadAsync(buffer, 0, buffer.Length);

                    if (receivedByteCount <= 0)
                    {
                        Console.WriteLine("Disconnected from server!");
                        _tcpClient.Close();
                        break;
                    }

                    Console.WriteLine(new string(buffer));
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while trying to read the data: {e.Message}");
            }
        }

        public async Task SendDataToServerAsync(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                Console.WriteLine("Invalid Input");
                return;
            }

            if (_tcpClient is { Connected: true })
            {
                var streamWriter = new StreamWriter(_tcpClient.GetStream())
                {
                    AutoFlush = true
                };

                await streamWriter.WriteAsync(userInput);
            }
        }

        public void CloseAndDisconnect()
        {
            if (_tcpClient is { Connected: true })
            {
                _tcpClient.Close();
            }
        }
    }
}