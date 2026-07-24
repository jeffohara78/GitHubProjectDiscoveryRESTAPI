namespace GitHubProjectDiscovery.Utilities;

public static class InputHelper
{
    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value)) return value;
            Console.WriteLine("Please enter a value.");
        }
    }

    public static string? ReadOptional(string prompt)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static int ReadMenuChoice(int min, int max)
    {
        while (true)
        {
            Console.Write("Choose an option: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max) return choice;
            Console.WriteLine($"Enter a number from {min} to {max}.");
        }
    }

    public static int ReadSelection(int count)
    {
        while (true)
        {
            Console.Write($"Select a result (1-{count}) or 0 to cancel: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 0 && choice <= count) return choice;
            Console.WriteLine("That selection is not valid.");
        }
    }

    public static (string Owner, string Repo)? ReadRepositoryName()
    {
        string input = ReadRequired("Enter repository as owner/name: ");
        string[] parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            Console.WriteLine("Use the format owner/name, for example dotnet/runtime.");
            return null;
        }
        return (parts[0], parts[1]);
    }

    public static void Pause()
    {
        Console.WriteLine("\nPress Enter to return to the menu.");
        Console.ReadLine();
    }
}
