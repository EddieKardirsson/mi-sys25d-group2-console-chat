namespace RetroChat.Models;

public class SystemMessage : Message
{
    private static readonly User SystemUser = new User("System");
    public SystemMessage(string text) : base(text, SystemUser) { }
    
    public override bool IsSystemMessage => true;
    
    public new async Task SendMessage(User user, string inMessage, string eventName)
    {
        throw new InvalidOperationException("System messages cannot be sent over the network.");
    }
}
