namespace RetroChat;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public override string ToString() => $"{Name} ({Id})";
    
    public User(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}