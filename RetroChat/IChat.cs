namespace RetroChat;

public interface IChat
{
    List<Message> Messages { get; }
    
    void StoreMessage(Message message, string chatId = "general");
    void DisplayMessages();
    void DisplayChat();
}