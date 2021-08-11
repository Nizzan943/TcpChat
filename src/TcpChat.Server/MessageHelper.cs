namespace TcpChat.Server
{
   public static class MessageHelper
    {
        public static bool IsPrivateMessage(this string message)
        {
            return  message.Split(">> ")[1].Split(" ")[0] == "To:";
        }
    }
}
