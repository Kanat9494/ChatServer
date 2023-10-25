namespace ChatServer.Client;

internal class Client
{
    internal Client(TcpClient tcpClient, Server server)
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
    Server _server;



    //Процесс
    public async Task Process()
    {
        try
        {
            Stream = _tcpClient.GetStream();
            string jsonMessage = await GetMessageAsync();
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);
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
}
