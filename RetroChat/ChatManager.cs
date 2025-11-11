using System.Text.Json;

namespace RetroChat;

public class ChatManager
{
    private static List<User> _storedUsers = new List<User>();
    public static User? LoggedInUser { get; private set; }
    
    private static readonly string DataFilePath = "./localData/";
    private static readonly string UserFilePath = DataFilePath + "users.json";
    
    #region StartUp
    
    public User StartUp()
    {
        InitializeUserDirectoryAndLoadCache();

        string username = PromptForUsername();
        
        User user = _storedUsers.Any(u => u.Name == username) ? LoadExistingUser(username) : CreateNewUser(username);
        
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(false);
        return user;
    }

    #region Private Methods for StartUp
    
    private static void InitializeUserDirectoryAndLoadCache()
    {
        if(!Directory.Exists(UserFilePath)) Directory.CreateDirectory(DataFilePath);

        if (File.Exists(UserFilePath))
        {
            string json = File.ReadAllText(UserFilePath);
            _storedUsers = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
            Console.WriteLine($"Loaded {_storedUsers.Count} users from cache");
        }
        else Console.WriteLine("No users found in cache");
    }

    private static string PromptForUsername()
    {
        Console.WriteLine("Enter your username: ");
        string? username = Console.ReadLine();

        while (username == null)
        {
            Console.WriteLine("Invalid input! Username cannot be empty");
            Console.WriteLine("Please, enter your username: ");
            username = Console.ReadLine(); 
        }
        Console.WriteLine($"{username} is logged in");
        
        return username;
    }

    private User LoadExistingUser(string username)
    {
        User user = _storedUsers.First(u => u.Name == username);
        LoggedInUser = user;
        Console.WriteLine("User already exists. Loading user...");
        return user;
    }

    private User CreateNewUser(string username)
    {
        Console.WriteLine("User does not exist. Creating new user...");
        
        User user = new User(username);
        LoggedInUser = user;
        
        string usersJson = JsonSerializer.Serialize(_storedUsers, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        File.WriteAllText(UserFilePath, usersJson);
        return user;
    }
    
    #endregion
    #endregion
    
    #region Main Menu
    
    // TODO: Implement menu display and navigation methods here.
    
    #endregion
    
    #region Handle Messages

    public static async Task HandleUserMessage(User user)
    {
        while (true)
        {
            if (SocketManager.Client.Connected)
            {
                Console.Write("Enter your message: ");
                string? input = Console.ReadLine();
                
                if(string.IsNullOrEmpty(input)) continue;

                if (input.ToLower() == "/quit" || input.ToLower() == "/exit") Environment.Exit(0);

                try
                {
                    Message message = new Message(input, user);
                    await message.SendMessage(user, input, SocketManager.GeneralChatEvent);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending message: {e.Message}");
                }
            }
            else await AttemptReconnectToServer();
        }
    }
    
    private static async Task AttemptReconnectToServer()
    {
        Console.WriteLine("Not connected to the server. Attempting to reconnect...");
        try
        {
            await SocketManager.Connect();
            await Task.Delay(1000);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Reconnection failed: {e.Message}");
            await Task.Delay(5000); 
        }
    }
    
    #endregion
}