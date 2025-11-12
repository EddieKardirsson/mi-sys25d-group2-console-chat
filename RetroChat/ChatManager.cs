using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroChat;

public class ChatManager
{
    private static List<User> _storedUsers = new List<User>();
    public static User? LoggedInUser { get; private set; }
    
    public static Chat Chat { get; } = new Chat();
    
    public static readonly string DataFilePath = "./localData/";
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
        _storedUsers.Add(user);
        
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
    public static void DisplayMenu()
    {
        bool showMainMenu = true;

        while (showMainMenu)
        {
            Console.Clear();
            Console.WriteLine("Menu");
            Console.WriteLine("1. Send message");
            Console.WriteLine("2. Select chat room");
            Console.WriteLine("3 Send private message");
            Console.WriteLine("Q. Exit");
            Console.Write("Enter your choice: ");

            char choice = Char.ToLower(Console.ReadKey(true).KeyChar);

            switch (choice)
            {
                case '1':
                    Console.Clear();
                    Console.WriteLine("Entering General Chat..."); 
                    // Något för att kunna komma in i Genereal chat
                    WaitForReturnTomenu();
                    break;


                case '2':
                    DisplayChatRoomMenu();
                    break;

                case '3':
                    Console.Clear();
                    Console.WriteLine("Send DM");
                    WaitForReturnTomenu();
                    break;

                case 'q':
                    Console.WriteLine("Exiting application...");
                    showMainMenu = false;
                    break; 

                default:
                    Console.WriteLine("Invalid input/choice. Try again.");
                    Thread.Sleep(1000);
                    break;


            }
        }
    } 

//ChatRoom submenu
    private static void DisplayChatRoomMenu()
    {

        bool showRoom = true;

        while (showRoom)
        {
            Console.Clear();
            Console.WriteLine(" Select chat room");
            Console.WriteLine("1.");
            Console.WriteLine("2.");
            Console.WriteLine("R. Return nack to menu");
            Console.Write("Select a chat room: ");

            char choice = char.ToLower(Console.ReadKey(true).KeyChar);

            switch (choice)
            {
                case '1':
                    EnterChatRoom("Room 1");
                    break;

                case '2':
                    EnterChatRoom("Room 2");
                    break;
                
                case 'r':
                    showRoom = false;
                    break;

                default:
                    Console.WriteLine("Invalid input/choice. Try again.");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    private static void EnterChatRoom(string roomName)
    {
        Console.Clear();
        Console.WriteLine($"You are in room {roomName}");
        Console.WriteLine("Press 'B' to go back to choose room");

        bool inRoom = true;
        
        while (inRoom)
        {
            char key = char.ToLower(Console.ReadKey(true).KeyChar);
            if (key == 'b')
            {
                inRoom = false;
            }
        }
    }

    private static void WaitForReturnTomenu()
    {
        Console.WriteLine("\nPress 'M' to return to Main Menu");
        while (true)
        {
            char key = char.ToLower(Console.ReadKey(true).KeyChar);
            if (key == 'm')
                break;
        }
    }
    
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

                if (input.ToLower() == "/quit" || input.ToLower() == "/exit") {Environment.Exit(0);}

                try
                {
                    Message message = new Message(input, user);
                    await message.SendMessage(user, input, SocketManager.GeneralChatEvent);
                    
                    // Just for testing, remove it later when fully implementing the chat.
                    Chat.StoreMessage(message);
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

    public static async Task SendJoinMessageEvent(User? user)
    {
        if (user != null && SocketManager.Client.Connected)
        {
            await SocketManager.Client.EmitAsync(SocketManager.UserJoinedEvent, user.Name);
        }
    }
    
    #endregion
}