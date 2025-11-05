namespace RetroChat;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Enter your username: ");
        string? input = Console.ReadLine();

        while (input == null)
        {
            Console.WriteLine("Invalid input");
            Console.WriteLine("Enter your username: ");
            input = Console.ReadLine(); 
        }

        Console.WriteLine($"{input} is logged in");
    }
}