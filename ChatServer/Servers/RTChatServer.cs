namespace ChatServer.Servers;

internal class RTChatServer
{
    static TcpListener _tcpListener;
    List<ChatClient> _clients = new List<ChatClient>();

    protected internal void AddConnection(ChatClient client)
        => _clients.Add(client);

    protected internal void RemoveConnection(Guid clientId)
    {
        ChatClient client = _clients.FirstOrDefault(c => c.ClientId == clientId);

        if (client != null)
            _clients.Remove(client);
    }

    protected internal void Listen()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Any, 8888);
            _tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                TcpClient tcpClient = _tcpListener.AcceptTcpClient();

                ChatClient client = new ChatClient(tcpClient, this);

                Task.Run(async () =>
                {
                    await client.ProcessAsync();
                });
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }

    protected internal void BroadcastMessage(string jsonMessage, Guid clientId)
    {
        var message = JsonSerializer.Deserialize<Message>(jsonMessage);
        byte[] data = Encoding.UTF8.GetBytes(message?.Content + _clients.Count ?? "Тело сообщения пустое");

        for (int i = 0; i < _clients.Count; i++)
        {
            //if (_clients[i].ClientId == clientId)
            //    _clients[i].Stream.Write(data, 0, data.Length);

            _clients[i].Stream.Write(data, 0, data.Length);
        }
    }

    protected internal void SendMessageToUser(string jsonMessage, Guid clientId)
    {
        var message = JsonSerializer.Deserialize<Message>(jsonMessage);
        byte[] data = Encoding.UTF8.GetBytes(message?.Content ?? "Тело сообщения пустое");

        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);

        client?.Stream.Write(data, 0, data.Length);
    }

    protected internal void Disconnect()
    {
        _tcpListener.Stop();

        for (int i = 0; i < _clients.Count; i++)
            _clients[i].Close();

        Environment.Exit(0);
    }
}
