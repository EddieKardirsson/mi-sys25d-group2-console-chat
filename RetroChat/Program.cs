namespace RetroChat;

class Program
{
    static void Main(string[] args)
    {
        ChatManager chatManager = new ChatManager();
        User user = chatManager.StartUp();
        Console.WriteLine($"Connected as {user.Name}");
        
        // TODO: DisplayMenu
        
        // TODO: Connect to server
        
        // TODO: Handle messages
    }
}