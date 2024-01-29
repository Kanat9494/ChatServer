namespace ChatServer.Client;

internal class ChatClient
{
    internal ChatClient(TcpClient tcpClient, RTChatServer server)
    {
        _tcpClient = tcpClient;
        _server = server;
    }

    protected internal NetworkStream Stream { get; private set; }
    protected internal int UserName { get; set; }
    protected internal int ReceiverName { get; set; }
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
            UserName = message?.SenderName ?? 1;
            ReceiverName = message?.ReceiverName ?? 2;

            message = new Message
            {
                SenderName = UserName,
                Content = "",
                ReceiverName = ReceiverName,
            };

            _server.AddConnection(this);


            var sendingMessage = JsonSerializer.Serialize(message);
            //_server.BroadcastMessage(sendingMessage, this.ClientId);
            //Console.WriteLine(message.Content + "\nА получатель должен быть " + ReceiverName);


            while (true)
            {
                try
                {
                    jsonMessage = await GetMessageAsync();
                    message = JsonSerializer.Deserialize<Message>(jsonMessage);
                    //await _server.BroadcastMessage(jsonMessage);

                    await _server.SendMessageToUser(jsonMessage);
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
            _server.RemoveConnection(UserName);
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
