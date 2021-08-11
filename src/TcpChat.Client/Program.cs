using System;

namespace TcpChat.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client(23000);

            Console.WriteLine("--- Welcome to TCP Chat ! ---");
            Console.Write("Provide the server IP: ");

            var ip = Console.ReadLine();

            if (!client.SetServerIP(ip))
            {
                Console.WriteLine("Invalid IP! Press any key to exit...");
                Console.ReadKey();

                return;
            }

            Console.Write("Provide your name: ");
            var name = Console.ReadLine();
            client.ConnectToServer(name);
            string userInput;

            do
            {
                userInput = Console.ReadLine();

                if (userInput?.Trim() != "<EXIT>")
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine(userInput + " << You");
                    client.SendDataToServerAsync($">> {userInput}");
                }
                else if (userInput.Equals("<EXIT>"))
                {
                    client.CloseAndDisconnect();
                }
            }
            while (userInput != "<EXIT>");
        }
    }
}