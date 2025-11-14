using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroChat;

public class ChatManager
{
    private static List<User> _storedUsers = new List<User>();
    public static User? LoggedInUser { get; private set; }
    
    public static Chat? Chat { get; private set; }

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
    public static async Task<bool> DisplayMenu(User user)
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
                Chat = new Chat(user);
                await SocketManager.Connect();
                await SendLeaveJoinMessageEvent(user, SocketManager.UserJoinedEvent, SocketManager.GeneralChatEvent);
                await HandleUserMessage(user);
                return false;
            case '2':
                bool inChatRoomMenu = true;
                while (inChatRoomMenu)
                    inChatRoomMenu = await DisplayChatRoomMenu(user);
                break;

            case '3':
                Console.Clear();
                Console.WriteLine("Send DM");
                return false;

            case 'Q':
            case 'q':
                await SendLeaveJoinMessageEvent(user, SocketManager.UserLeftEvent, SocketManager.GeneralChatEvent);
                await DisconnectAndExit();
                return false; 

            default:
                Console.WriteLine("Invalid input/choice. Try again.");
                Thread.Sleep(1000);
                break;
        }
        return true;
    } 

//ChatRoom submenu
    private static async Task<bool> DisplayChatRoomMenu(User user)
    {

        Console.Clear();
        Console.WriteLine(" Select chat room");
        Console.WriteLine("1.");
        Console.WriteLine("2.");
        Console.WriteLine("R. Return back to menu");
        Console.Write("Select a chat room: ");

        char choice = char.ToLower(Console.ReadKey(true).KeyChar);

        switch (choice)
        {
            case '1':
                await EnterChatRoom(SocketManager.Rooms[0], user);
                return false;

            case '2':
                await EnterChatRoom(SocketManager.Rooms[1], user);
                return false;
                
            case 'r':
                return false;

            default:
                Console.WriteLine("Invalid input/choice. Try again.");
                Thread.Sleep(1000);
                break;
        }

        return true;
    }

    private static async Task EnterChatRoom(string roomName, User user)
    {
        Console.Clear();
        Console.WriteLine($"You are in room {roomName}\n");
    
        // Convert room name to event name format (e.g., "Room 1" -> "/room1")
        string eventName = ConvertRoomNameToEvent(roomName);
    
        Chat = new Chat(user, roomName);
        await SocketManager.Connect(eventName); // Use the event name, not room name
        await SendLeaveJoinMessageEvent(user, SocketManager.UserJoinedEvent, eventName);
        await HandleUserMessage(user, eventName); // Pass event name here too
    }
    
    private static string ConvertRoomNameToEvent(string roomName)
    {
        // Convert "Room 1" to "/room1", "Room 2" to "/room2", etc.
        return "/" + roomName.ToLower().Replace(" ", "");
    }

    private static bool WaitForReturnToMenu()
    {
        Console.WriteLine("\nPress 'M' to return to Main Menu or any key to continue");
        while (true)
        {
            char key = char.ToLower(Console.ReadKey(true).KeyChar);
            if (key == 'm')
            {
                return true;
            }
            return false;
        }
    }
    
    #endregion
    
    #region Handle Messages

    public static async Task HandleUserMessage(User user, string eventName = SocketManager.GeneralChatEvent)
    {
        string inputBuffer = string.Empty;
        int lastMessageCount = Chat!.Messages.Count;
        
        Chat.UpdateInput(inputBuffer);
        Chat.DisplayChat();

        while (true)
        {
            if (SocketManager.Client.Connected)
            {
                bool needsRefresh = false;
                
                if (Console.KeyAvailable)
                {
                    (inputBuffer, needsRefresh) = await HandleUserInput(user, eventName, inputBuffer, needsRefresh);
                }
                
                if (Chat!.Messages.Count != lastMessageCount)
                {
                    lastMessageCount = Chat!.Messages.Count;
                    needsRefresh = true;
                }
                
                if (needsRefresh)
                {
                    Chat!.DisplayChat();
                }

                await Task.Delay(50);
            }
            else
            {
                await AttemptReconnectToServer();

                if (SocketManager.Client.Connected)
                {
                    Chat!.DisplayChat();
                }
            }
        }
    }

    private static async Task<(string inputBuffer, bool needsRefresh)> 
        HandleUserInput(User user, string eventName, string inputBuffer, bool needsRefresh)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            if (!string.IsNullOrEmpty(inputBuffer))
            {
                if (CheckForExitCommand(inputBuffer))
                {
                    await SendLeaveJoinMessageEvent(user, SocketManager.UserLeftEvent, eventName);
                    await DisconnectAndExit();
                }

                if (CheckForLeaveChatCommand(inputBuffer))
                {
                    await SendLeaveJoinMessageEvent(user, SocketManager.UserLeftEvent, eventName);
                    await SocketManager.Disconnect();
                    return (inputBuffer, needsRefresh);
                }

                try
                {
                    Message message = new Message(inputBuffer, user);
                    await message.SendMessage(user, inputBuffer, eventName);
                    Chat!.StoreMessage(message);
                }
                catch (Exception e)
                {
                    // Silent error handling to not disrupt display, needs to be empty
                }

                inputBuffer = string.Empty;
                Chat!.UpdateInput(inputBuffer);
                needsRefresh = true;
            }
        }
        else if (key.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
        {
            inputBuffer = inputBuffer[..^1];
            Chat!.UpdateInput(inputBuffer);
            needsRefresh = true;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            inputBuffer += key.KeyChar;
            Chat!.UpdateInput(inputBuffer);
            needsRefresh = true;
        }

        return (inputBuffer, needsRefresh);
    }

    private static bool CheckForLeaveChatCommand(string input)
    {
        return SocketManager.LeaveChatCommands.Any(c => input.ToLower() == c);
    }

    private static bool CheckForExitCommand(string input)
        {
            bool bIsExitCommand = false;
            SocketManager.ExitCommands.ForEach(c =>
            {
                 if(input.ToLower() == c) bIsExitCommand = true;               
            });
            return bIsExitCommand;
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

    public static async Task SendLeaveJoinMessageEvent(User? user, string eventName, string roomEvent)
    {
        if(user == null) return;
        
        if (SocketManager.Client != null && SocketManager.Client.Connected)
        {
            string roomSpecificEvent = eventName + roomEvent;
            await SocketManager.Client.EmitAsync(roomSpecificEvent, user.Name);
        }
    }
    
    #endregion
    
    public static async Task DisconnectAndExit()
    {
        LoggedInUser = null;
        if (SocketManager.Client != null)
        {
            Console.WriteLine("Disposing client...");
            await SocketManager.Disconnect();
        }
        Environment.Exit(0);
    }
}