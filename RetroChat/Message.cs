using System.Text.Json;
namespace RetroChat;

public class Message
{
    public string Text { get; set; }
    public User User { get; set; }
    public DateTime TimeStamp { get; set; }

    public Message(string text, User user)
    {
        Text = text;
        User = user;
        TimeStamp = DateTime.Now;
    }

    public async Task SendMessage(User user, string inMessage, string eventName)
    {
        string jsonMessage = JsonSerializer.Serialize(this);
        string outMessage = $"\n{user.Name} [{TimeStamp}]: \n{inMessage}";

        await SocketManager.Client.EmitAsync(eventName, jsonMessage);
        Console.WriteLine(outMessage);
    }

    public static async Task ReceiveMessage(string message)
    {
        Console.WriteLine(message);
    }
}