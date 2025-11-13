using System.Text.Json;

namespace RetroChat;

public class Chat : IChat
{
    public List<Message> Messages { get; private set; } = new List<Message>();
    private static string? _dataFilePath;
    private static string? _chatFilePath;
    public const string DefaultChatId = "general";
    
    public string ChatId { get; set; } = DefaultChatId;

    public Chat(User user, string chatEventName = DefaultChatId)
    {
        ChatId = chatEventName;
        _dataFilePath = $"{ChatManager.DataFilePath}{user.Name}/chats/";
    }
    
    public virtual void StoreMessage(Message message)
    {
        Messages.Add(message);
        Console.WriteLine("Message stored");
        SaveMessagesToCache();
    }
    
    private void SaveMessagesToCache()
    {
        EnsureDirectoryExists();
        
        _chatFilePath = $"{_dataFilePath}{ChatId}.json";
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

    public virtual void RetrieveMessagesFromCache()
    {
        EnsureDirectoryExists();
        _chatFilePath = $"{_dataFilePath}{ChatId}.json";
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