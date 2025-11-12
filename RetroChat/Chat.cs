using System.Text.Json;

namespace RetroChat;

public class Chat : IChat
{
    public List<Message> Messages { get; private set; } = new List<Message>();
    private static readonly string DataFilePath = $"{ChatManager.DataFilePath}chats/";
    private static string? _chatFilePath;
    private const string DefaultChatId = "general";
    
    public virtual void StoreMessage(Message message, string chatId = DefaultChatId)
    {
        Messages.Add(message);
        Console.WriteLine("Message stored");
        SaveMessagesToCache(chatId);
    }

    public virtual void RetrieveMessagesFromCache(string chatId = DefaultChatId)
    {
        // TODO: Implement cache retrieval
        EnsureDirectoryExists();
        _chatFilePath = $"{DataFilePath}{chatId}.json";
        if (File.Exists(_chatFilePath))
        {
            string json = File.ReadAllText(_chatFilePath);
            try
            {
                Messages = JsonSerializer.Deserialize<List<Message>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving messages from cache: {e.Message}");
            }
        }
    }

    private void SaveMessagesToCache(string chatId)
    {
        EnsureDirectoryExists();
        
        _chatFilePath = $"{DataFilePath}{chatId}.json";
        try
        {
            string json = JsonSerializer.Serialize(Messages, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            File.WriteAllText(_chatFilePath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving messages to cache: {e.Message}");
        }
    }

    private static void EnsureDirectoryExists()
    {
        if (!Directory.Exists(DataFilePath)) Directory.CreateDirectory(DataFilePath);
    }

    public virtual void DisplayMessages()
    {
        // TODO: Implement message display
    }

    public void DisplayChat()
    {
        // TODO: Implement chat display
    }
}