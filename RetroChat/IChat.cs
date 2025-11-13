namespace RetroChat;

public interface IChat
{
    List<Message> Messages { get; }
    
    void StoreMessage(Message message);
    void DisplayMessages();
    void DisplayChat();
}