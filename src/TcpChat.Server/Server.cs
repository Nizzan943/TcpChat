using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpChat.Server
{
    public class Server
    {
        private IPAddress _ipAddress;
        private int _port;
        private TcpListener _listener;
        private readonly List<Client> _connectedClients = new();
        private bool _keepRunning;
        private readonly Dictionary<char, int> _lettersDictionary = new();
        
        public async Task StartServer(IPAddress ipAddress = null, int port = 23000)
        {
            _ipAddress = ipAddress ?? IPAddress.Any;
            _port = port;
            _listener ??= new TcpListener(_ipAddress, _port);

            try
            {
                _listener.Start();
                Console.WriteLine($"Server started ({_ipAddress}:{_port}). Type <CLOSE> to close!");
                _keepRunning = true;

                while (_keepRunning)
                {
                    TcpClient connectedTcpClient = await _listener.AcceptTcpClientAsync();
                    var client = new Client(connectedTcpClient, _connectedClients.Count.ToString());
                    NetworkStream stream = client.TcpClient.GetStream();
                    var reader = new StreamReader(stream);
                    var buffer = new char[64];
                    await reader.ReadAsync(buffer, 0, buffer.Length);
                    var msg = new string(buffer);
                    client.Name = msg.Split("-")[1].Replace("\0", string.Empty);
                    _connectedClients.Add(client);
                    Console.WriteLine($"Client connected: {client.TcpClient.Client.LocalEndPoint}");

                    HandleClient(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while trying to start the server: {e.Message}");
            }
        }

        private async Task HandleClient(Client client)
        {
            TcpClient tcpClient = client.TcpClient;

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                var reader = new StreamReader(stream);
                var buffer = new char[64];

                while (_keepRunning)
                {
                    var sentBytesCount = await reader.ReadAsync(buffer, 0, buffer.Length);
                    Console.WriteLine($"Client sent {sentBytesCount} bytes this message");

                    if (sentBytesCount <= 0)
                    {
                        RemoveClient(client);
                        break;
                    }

                    var msg = new string(buffer);
                    SumLetters(msg);

                    if (msg.IsPrivateMessage())
                    {
                        SendPrivateMessage(msg, client);
                    }
                    else
                    {
                        SendMessageForAllClients(msg, client);
                    }
                    
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                RemoveClient(client);
                Console.WriteLine($"Error while trying to handle client: {e.Message}");
            }
        }

        private void SumLetters(string msg)
        {
            var messageBody = msg.IsPrivateMessage() ? msg.Split("-")[1] : msg;

            Console.WriteLine($"Letters appears that appear in previous messages: {_lettersDictionary.Count}");
            var stringBuilder = new StringBuilder();

            foreach (var key in _lettersDictionary.Keys)
            {
                stringBuilder.Append($"{key}({_lettersDictionary[key]}),");
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            Console.WriteLine(stringBuilder.ToString());

            foreach (var letter in messageBody.ToLower().Where(letter => (letter >= 'a' && letter <= 'z')))
            {
                if (_lettersDictionary.ContainsKey(letter))
                {
                    _lettersDictionary[letter]++;
                }
                else
                {
                    _lettersDictionary.Add(letter, 1);
                }
            }
        }

        private void RemoveClient(Client client)
        {
            if (!_connectedClients.Contains(client))
            {
                return;
            }

            _connectedClients.Remove(client);
            Console.WriteLine($"Client {client.TcpClient.Client.RemoteEndPoint} disconnected! Connected clients count: {_connectedClients.Count}");
        }

        private async Task SendMessageForAllClients(string message, Client owner)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var messageDetails = $"{DateTime.Now:dd/mm/yyyy hh:mm:ss}, {owner.Name}";

            try
            {
                foreach (Client client in _connectedClients.Where(client => client != owner))
                {
                    var buffer = Encoding.ASCII.GetBytes($"{messageDetails} - {message}");
                    await client.TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while trying to send message to all clients: {e.Message}");
            }
        }

        private async Task SendPrivateMessage(string message, Client owner)
        {
            var messageDetails = $"{DateTime.Now:dd/mm/yyyy hh:mm:ss}, [PM: {owner.Name}] ";
            var messageBody = message.Split("-")[1];
            var privateClientName = message.Split("To: ")[1].Split("-")[0];
            var privateMessage = $"{messageDetails}- {messageBody}";
            var privateClients = _connectedClients.Where(client => client.Name == privateClientName);

            foreach (Client client in privateClients)
            {
                var buffer = Encoding.ASCII.GetBytes(privateMessage);
                await client.TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}