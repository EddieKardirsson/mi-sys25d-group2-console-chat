namespace RetroChat;

class Program
{
    static async Task Main(string[] args)
    {
        Console.CursorTop++;
        
        ChatManager chatManager = new ChatManager();
        User user = chatManager.StartUp();
        Console.WriteLine($"Connected as {user.Name}");
        
        // TODO: DisplayMenu
     
        await SocketManager.Connect();
        await TemporaryMenu(user);
    }
    
    static async Task TemporaryMenu(User user)
    {
        Console.Clear();
        Console.WriteLine("Menu");
        Console.WriteLine("1. Enter General Chat");
        Console.WriteLine("2. Select Chat Room");
        Console.WriteLine("3. Send Private Message");
        Console.WriteLine("Q. Exit");
        
        Console.WriteLine("Enter your choice: ");
        char choice = Console.ReadKey(false).KeyChar;

        switch (choice)
        {
            case '1':
                // TODO: Enter General Chat
                Console.WriteLine("\nEntering General Chat");
                await SocketManager.Connect();

                await ChatManager.SendJoinMessageEvent(user);
        
                await ChatManager.HandleUserMessage(user);
                break;
            case '2':
                // TODO: Enter Chat Room Selection
                Console.WriteLine("\nChat Room Selection");
                break;
            case '3':
                // TODO: Enter Private Chat (Connect with another user)
                Console.WriteLine("\nSending DM");
                break;
            case 'q':
            case 'Q':
                Environment.Exit(0);
                break;
            default:
                break;
        }
    }
}