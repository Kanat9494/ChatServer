namespace ChatServer.Models;

public class Message
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; }
}
