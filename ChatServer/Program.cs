class Program
{
    static RTChatServer _server;
    static Thread _listenThread;

    static void Main(string[] args)
    {
        try
        {
            _server = new RTChatServer();
            _listenThread = new Thread(_server.Listen);
            _listenThread.Start();
        }
        catch (Exception ex)
        {
            _server.Disconnect();
            Console.WriteLine($"Сервер остановлен. Причина: {ex.Message}");
        }
    }
}