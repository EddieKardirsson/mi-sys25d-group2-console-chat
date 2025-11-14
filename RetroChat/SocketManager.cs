namespace RetroChat;

using SocketIOClient;

public class SocketManager
{
    private static SocketIO _client = null!;
    private static bool _isConnected = false;
    private static string? _currentEventName = null;

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
        if (_isConnected && _currentEventName == eventName)
        {
            ChatManager.Chat!.RetrieveMessagesFromCache();
            return;
        }
        
        if (_isConnected && _currentEventName != eventName)
        {
            await Disconnect();
        }

        _client = new SocketIO(Uri, new SocketIOOptions
        {
            Path = Path
        });

        _currentEventName = eventName;

        ChatManager.Chat!.RetrieveMessagesFromCache();

        HandleError();
        HandleReceivedMessage(eventName);
        HandleConnection();
        HandleDisconnection();
        
        HandleUserJoinedEvent(UserJoinedEvent + eventName);
        HandleUserLeftEvent(UserLeftEvent + eventName);

        await EstablishConnectionAsync();
    }

    private static void HandleError() =>
        _client.OnError += (sender, error) => Console.WriteLine(error);

    private static void HandleReceivedMessage(string eventName)
    {
        _client.On(eventName, response =>
        {
            try
            {
                Message receivedMessage = response.GetValue<Message>();
                
                if (_currentEventName == eventName)
                {
                    _ = Message.ReceiveMessage(receivedMessage);
                    ChatManager.Chat!.StoreMessage(receivedMessage);
                }
            }
            catch (Exception e)
            {
                // Silent error handling to not disrupt display
            }
        });
    }

    private static void HandleConnection() =>
        _client.OnConnected += (sender, eventArgs) => { _isConnected = true; };

    private static void HandleDisconnection() =>
        _client.OnDisconnected += (sender, eventArgs) =>
        {
            _isConnected = false;
            _currentEventName = null;
        };

    private static async Task EstablishConnectionAsync()
    {
        try
        {
            await _client.ConnectAsync();
            await Task.Delay(1000);
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
        
        Message systemMessage = Message.CreateSystemMessage($"{userJoined} joined the chat.");
        ChatManager.Chat?.StoreMessage(systemMessage);
    });

    private static void HandleUserLeftEvent(string eventName) => _client.On(eventName, response =>
    {
        string userLeft = response.GetValue<string>();
        
        Message systemMessage = Message.CreateSystemMessage($"{userLeft} left the chat.");
        ChatManager.Chat?.StoreMessage(systemMessage);
    });

    public static async Task Disconnect()
    {
        if (_client != null && _isConnected)
        {
            try
            {
                await _client.DisconnectAsync();
            }
            catch
            {
                // Ignore disconnect errors
            }
            finally
            {
                _client?.Dispose();
            }
        }

        _isConnected = false;
        _currentEventName = null;
    }
}