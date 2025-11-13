namespace RetroChat;

using SocketIOClient;

public class SocketManager
{
    private static SocketIO _client = null!;
    private static bool _isConnected = false;
    
    public static bool IsConnected => _isConnected;
    public static SocketIO Client => _client;

    private const string Uri = "wss://api.leetcode.se";
    private const string Path = "/sys25d";
    
    public const string GeneralChatEvent = "/general";
    public const string UserJoinedEvent = "/userJoined";
    public const string UserLeftEvent = "/userLeft";
    public static List<string> Rooms = ["Room 1", "Room 2"];

    public static readonly List<string> ExitCommands = ["/quit", "/exit"];
    public static readonly List<string> LeaveChatCommands = ["/l", "/lc", "/leave"];

    public static async Task Connect(string eventName = GeneralChatEvent)
    {
        _client = new SocketIO(Uri, new SocketIOOptions
        {
            Path = Path
        });
        
        // Just for testing purposes, remove later when fully implementing the chat.
        ChatManager.Chat.RetrieveMessagesFromCache();
        ChatManager.Chat.DisplayMessages();
        // end of testing section.
        
        HandleError();
        HandleReceivedMessage(eventName);
        HandleConnection();
        HandleDisconnection();
        HandleUserJoinedEvent(UserJoinedEvent);
        HandleUserLeftEvent(UserLeftEvent);
        
        await EstablishConnectionAsync();
        
    }
    
    private static void HandleError() =>
        _client.OnError += (sender, error) => Console.WriteLine(error);

    private static void HandleReceivedMessage(string eventName)
    {
        _client.On(eventName, response =>
        {
            Console.WriteLine($"Received message event {eventName}: ");
            try
            {
                Message receivedMessage = response.GetValue<Message>();
                _ = Message.ReceiveMessage(receivedMessage);
                ChatManager.Chat!.StoreMessage(receivedMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error parsing message: {e.Message}");
                Console.WriteLine($"Raw response: {response}");
            }
        });
    }
    
    private static void HandleConnection() =>
        _client.OnConnected += (sender, eventArgs) =>
        {
            _isConnected = true;
            Console.WriteLine("Connected to server");
        };

    private static void HandleDisconnection() =>
        _client.OnDisconnected += (sender, eventArgs) =>
        {
            _isConnected = false;
            Console.WriteLine("Disconnected from server");
        };

    private static async Task EstablishConnectionAsync()
    {
        try
        {
            await _client.ConnectAsync();
            
            await Task.Delay(1000);
            
            if(!_isConnected) Console.WriteLine("Not connected to server.");
            
            Console.WriteLine($"Connected status: {_isConnected}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection failed: {e.Message}");
            throw;
        }
    }
    
    private static void HandleUserJoinedEvent(string eventName) => _client.On(eventName, response =>
    {
        string userJoined = response.GetValue<string>();
        Console.WriteLine($"User {userJoined} joined the chat.");
    });

    private static void HandleUserLeftEvent(string eventName) => _client.On(eventName, response =>
    {
        string userLeft = response.GetValue<string>();
        Console.WriteLine($"User {userLeft} left the chat.");
    });
    
    public static Task Disconnect()
    {
        Console.WriteLine("Disconnecting from server...");
        _client.Dispose();
        return Task.CompletedTask;
    }
}