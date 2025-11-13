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
        await SocketManager.Client.EmitAsync(eventName, this);
    }

    public static async Task ReceiveMessage(Message message)
    {

        try
        {
            // Message will be displayed by DisplayChat
            // Just handle errors here
            if (message == null) throw new ArgumentNullException(nameof(message));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deserializing message: {e.Message}");
            Console.WriteLine(message);
        }
    }
}