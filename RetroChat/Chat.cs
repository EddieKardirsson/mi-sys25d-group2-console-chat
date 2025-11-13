using System.Text.Json;

namespace RetroChat;

public class Chat : IChat
{
    public List<Message> Messages { get; private set; } = new List<Message>();
    private static string? _dataFilePath;
    private static string? _chatFilePath;
    public const string DefaultChatId = "general";

    public Chat(User user)
    {
        _dataFilePath = $"{ChatManager.DataFilePath}{user.Name}/chats/";
    }
    
    public virtual void StoreMessage(Message message, string chatId = DefaultChatId)
    {
        Messages.Add(message);
        Console.WriteLine("Message stored");
        SaveMessagesToCache(chatId);
    }
    
    private void SaveMessagesToCache(string chatId)
    {
        EnsureDirectoryExists();
        
        _chatFilePath = $"{_dataFilePath}{chatId}.json";
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

    public virtual void RetrieveMessagesFromCache(string chatId = DefaultChatId)
    {
        EnsureDirectoryExists();
        _chatFilePath = $"{_dataFilePath}{chatId}.json";
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

    private static void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_dataFilePath)) Directory.CreateDirectory(_dataFilePath);
    }

    public virtual void DisplayMessages()
    {
        Messages.ForEach(message =>
        {
            Console.WriteLine($"\n{message?.User.Name} [{message?.TimeStamp}]: \n{message?.Text}");
        });
    }

    public void DisplayChat()
    {
        // TODO: Implement chat display
    }
}