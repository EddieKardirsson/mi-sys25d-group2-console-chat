namespace RetroChat;

using SocketIOClient;

public class SocketManager
{
    private static SocketIO _client;
    private static bool _isConnected = false;
    
    public static bool IsConnected => _isConnected;
    public static SocketIO Client => _client;

    public const string GeneralChatEvent = "/general";
    
    private const string Uri = "wss://api.leetcode.se";
    private const string Path = "/sys25d";

    public static async Task Connect()
    {
        _client = new SocketIO(Uri, new SocketIOOptions
        {
            Path = Path
        });
        
        HandleError();
        HandleReceivedMessage(GeneralChatEvent);
        HandleConnection();
        HandleDisconnection();
        
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
                string receivedMessage = response.GetValue<string>();
                _ = Message.ReceiveMessage(receivedMessage);
                Console.WriteLine($"{receivedMessage}");
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
}