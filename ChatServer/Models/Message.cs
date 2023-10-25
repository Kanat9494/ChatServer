namespace ChatServer.Models;

public class Message
{
    public string SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public string Content { get; set; }
}
