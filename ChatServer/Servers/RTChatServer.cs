﻿namespace ChatServer.Servers;

internal class RTChatServer
{
    static TcpListener? _tcpListener;
    List<ChatClient> _clients = new List<ChatClient>();

    protected internal void AddConnection(ChatClient client)
    {
        _clients.Add(client);
        Console.WriteLine($"Количество подключенных пользователей: {_clients.Count}. " +
            $"Подключен пользователь с userId: {client.UserId}");
    }

    protected internal void RemoveConnection(int userId)
    {

        if (_clients.Count > 0)
        {
            if (_clients != null)
            {
                ChatClient? client = _clients.Find(c => c.UserId == userId);

                if (client != null)
                {
                    _clients.Remove(client);


                    Console.WriteLine($"Количество подключенных пользователей: {_clients.Count}. " +
                        $"Пользователь с userId {client?.UserId} отключен.");
                }
            }
                

        }

        

       

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

    protected internal async Task BroadcastMessage(string jsonMessage)
    {
        try
        {
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);
            byte[] data = Encoding.UTF8.GetBytes(message?.Content + _clients.Count ?? "Тело сообщения пустое");

            for (int i = 0; i < _clients.Count; i++)
            {
                //if (_clients[i].ClientId == clientId)
                //    _clients[i].Stream.Write(data, 0, data.Length);

                await _clients[i].Stream.WriteAsync(data, 0, data.Length);
            }
        }
        catch { }
    }

    protected internal async Task SendMessageToUser(string jsonMessage)
    {
        try
        {
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage?.ToString() ?? "Тело сообщения пустое");

            //var client = _clients.Find(c => c.UserId == message?.ReceiverId);

            //if (client != null)
            //    await client?.Stream.WriteAsync(data, 0, data.Length);

            //Для клиентов, которые вошли с разных телефонов или 1 клиент открыл много подключений
            var clients = _clients.Where(c => c.UserId == message?.ReceiverId).ToList();
            if (clients != null)
            {
                for (int i = 0; i < clients.Count; i++)
                    await clients[i].Stream.WriteAsync(data, 0, data.Length);
            }
        }
        catch { }
    }

    protected internal void Disconnect()
    {
        _tcpListener?.Stop();

        for (int i = 0; i < _clients.Count; i++)
            _clients[i].Close();

        Environment.Exit(0);
    }
}
