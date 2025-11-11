namespace RetroChat;

public class Chat : IChat
{
    public List<Message> Messages { get; set; } = new List<Message>();
    
    public virtual void StoreMessage(Message message)
    {
        Messages.Add(message);
        Console.WriteLine("Message stored");
    }

    protected virtual void RetrieveMessagesFromCache()
    {
        // TODO: Implement cache retrieval
    }

    public virtual void DisplayMessages()
    {
        // TODO: Implement message display
    }

    public void DisplayChat()
    {
        // TODO: Implement chat display
    }
}