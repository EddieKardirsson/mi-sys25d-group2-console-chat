using System.Text.Json;
using Spectre.Console;

namespace RetroChat;

public class Chat : IChat
{
    public List<Message> Messages { get; private set; } = new List<Message>();
    private static string? _dataFilePath;
    private static string? _chatFilePath;
    public const string DefaultChatId = "general";

    public string ChatId { get; set; } = DefaultChatId;

    private const int MaxDisplayMessages = 15;
    private string _currentInput = string.Empty;

    public Chat(User user, string chatEventName = DefaultChatId)
    {
        ChatId = chatEventName;
        _dataFilePath = $"{ChatManager.DataFilePath}{user.Name}/chats/";
    }

    public virtual void StoreMessage(Message message)
    {
        Messages.Add(message);
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
        AnsiConsole.Clear();

        var layout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Messages"),
                new Layout("Input").Size(3)
            );
        
        UpdateHeaderPanel(layout);
        UpdateMessagesPanel(layout);
        UpdateInputPanel(layout);

        AnsiConsole.Write(layout);
    }

    private void UpdateHeaderPanel(Layout layout)
    {
        layout["Header"].Update(
            new Panel(new Markup($"[bold cyan]RetroChat - {Markup.Escape(ChatId)}[/]"))
                .Border(BoxBorder.Double)
                .BorderColor(Color.Cyan)
                .Expand());
    }

    private void UpdateMessagesPanel(Layout layout)
    {
        layout["Messages"].Update(
            new Panel(RenderMessages())
                .Border(BoxBorder.Square)
                .BorderColor(Color.Cyan)
                .Expand());
    }

    private void UpdateInputPanel(Layout layout)
    {
        layout["Input"].Update(
            new Panel(new Markup($"[bold yellow]You:[/] {Markup.Escape(_currentInput)}[blink]|[/]"))
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Expand());
    }

    private Markup RenderMessages()
    {
        List<Message> messagesToShow = Messages.TakeLast(MaxDisplayMessages).ToList();

        if (messagesToShow.Count == 0)
            return new Markup("[dim]No messages yet. Start chatting![/]\n[dim]Type /quit or /exit to leave.[/]");

        List<string> lines = new List<string>();

        GenerateMessageLines(messagesToShow, lines);

        if (lines.Count > 0 && lines[^1] == "")
            lines.RemoveAt(lines.Count - 1);

        return new Markup(string.Join("\n", lines));
    }

    private static void GenerateMessageLines(List<Message> messagesToShow, List<string> lines)
    {
        messagesToShow.ForEach(msg =>
        {
            string timestamp = msg.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
            string username = Markup.Escape(msg.User.Name);
            string message = Markup.Escape(msg.Text);
            
            lines.Add($"[bold cyan]{username}[/] [dim][[{timestamp}]][/]:\n{message}\n");
        });
    }

    public void UpdateInput(string input) => _currentInput = input;
}