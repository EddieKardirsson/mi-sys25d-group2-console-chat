namespace RetroChat;

class Program
{
    static async Task Main(string[] args)
    {
        Console.CursorTop++;
        
        ChatManager chatManager = new ChatManager();
        User user = chatManager.StartUp();
        Console.WriteLine($"Connected as {user.Name}");
        
        bool showMenu = true;
        while (showMenu)
        {
            showMenu = await ChatManager.DisplayMenu(user);
        }
    }
}