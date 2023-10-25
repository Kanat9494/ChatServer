namespace ChatServer.Client;

internal class ChatClient
{
    internal ChatClient(TcpClient tcpClient, RTChatServer server)
    {
        ClientId = Guid.NewGuid();
        _tcpClient = tcpClient;
        _server = server;
        _server.AddConnection(this);
    }

    protected internal Guid ClientId { get; private set; }
    protected internal NetworkStream Stream { get; private set; }
    string _userName;
    protected internal string UserName { get; set; }
    protected internal string ReceiverName { get; set; }
    TcpClient _tcpClient;
    RTChatServer _server;



    // Процесс
    public async Task ProcessAsync()
    {
        try
        {
            Stream = _tcpClient.GetStream();
            string jsonMessage = await GetMessageAsync();
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);
            _userName = message.SenderName;
            UserName = message.SenderName;
            ReceiverName = message.ReceiverName;

            message = new Message
            {
                SenderName = _userName,
                Content = _userName + " вошел в чат",
                ReceiverName = ReceiverName,
            };

            var sendingMessage = JsonSerializer.Serialize(message);
            //_server.BroadcastMessage(sendingMessage, this.ClientId);
            Console.WriteLine(message.Content + "\nА получатель должен быть " + ReceiverName);


            while (true)
            {
                try
                {
                    jsonMessage = await GetMessageAsync();
                    message = JsonSerializer.Deserialize<Message>(jsonMessage);
                    _server.BroadcastMessage(jsonMessage, this.ClientId);
                }
                catch
                {
                    break;
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _server.RemoveConnection(ClientId);
            Close();
        }
    }

    // Обработка сообщения
    private async Task<string> GetMessageAsync()
    {
        byte[] data = new byte[1024];
        StringBuilder builder = new StringBuilder();
        int bytes = 0;
        do
        {
            builder.Length = 0;
            bytes = await Stream.ReadAsync(data, 0, data.Length);
            builder.Append(Encoding.UTF8.GetString(data, 0, bytes));

            if (bytes == data.Length)
                Array.Resize(ref data, data.Length * 2);
        } while (Stream.DataAvailable);

        return builder.ToString();
    }

    protected internal void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (_tcpClient != null)
            _tcpClient.Close();
    }
}
