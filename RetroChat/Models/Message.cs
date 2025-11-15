using RetroChat.Services;

namespace RetroChat.Models;

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
    
    public virtual bool IsSystemMessage => false;

    public async Task SendMessage(User user, string inMessage, string eventName)
    {
        await SocketManager.Client.EmitAsync(eventName, this);
    }

    public static async Task ValidateMessage(Message message)
    {
        try
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
        }
        
        catch (Exception e)
        {
            Console.WriteLine($"Error deserializing message: {e.Message}");
            Console.WriteLine(message);
        }
    }
}