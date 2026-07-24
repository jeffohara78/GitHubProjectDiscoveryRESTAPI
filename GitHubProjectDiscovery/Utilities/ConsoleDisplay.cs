using GitHubProjectDiscovery.Models;

namespace GitHubProjectDiscovery.Utilities;

public static class ConsoleDisplay
{
    public static void Header(string title)
    {
        Console.Clear();
        Console.WriteLine(new string('=', 72));
        Console.WriteLine(title.ToUpperInvariant());
        Console.WriteLine(new string('=', 72));
    }

    public static void RepositoryList(IReadOnlyList<RepositorySummary> repositories)
    {
        if (repositories.Count == 0) { Console.WriteLine("No repositories were found."); return; }
        for (int i = 0; i < repositories.Count; i++)
        {
            RepositorySummary r = repositories[i];
            Console.WriteLine($"{i + 1,2}. {r.FullName}");
            Console.WriteLine($"    {Trim(r.Description, 88)}");
            Console.WriteLine($"    Language: {r.Language ?? "Unknown"} | Stars: {r.Stars:N0} | Forks: {r.Forks:N0} | Updated: {r.UpdatedAt:MMM d, yyyy}");
        }
    }

    public static void RepositoryDetails(RepositorySummary r)
    {
        Console.WriteLine($"Repository: {r.FullName}");
        Console.WriteLine($"Description: {r.Description ?? "No description provided"}");
        Console.WriteLine($"Owner: {r.Owner.Login}");
        Console.WriteLine($"Primary language: {r.Language ?? "Unknown"}");
        Console.WriteLine($"Stars: {r.Stars:N0} | Forks: {r.Forks:N0} | Watchers: {r.Watchers:N0}");
        Console.WriteLine($"Open issues/PRs reported by repository: {r.OpenIssues:N0}");
        Console.WriteLine($"License: {r.License?.Name ?? "Not specified"}");
        Console.WriteLine($"Default branch: {r.DefaultBranch}");
        Console.WriteLine($"Size: {r.SizeKb:N0} KB | Archived: {(r.Archived ? "Yes" : "No")} | Fork: {(r.IsFork ? "Yes" : "No")}");
        Console.WriteLine($"Created: {r.CreatedAt:MMM d, yyyy} | Last updated: {r.UpdatedAt:MMM d, yyyy}");
        Console.WriteLine($"Last push: {(r.PushedAt.HasValue ? r.PushedAt.Value.ToString("MMM d, yyyy h:mm tt") : "Unknown")}");
        Console.WriteLine($"Topics: {(r.Topics.Count > 0 ? string.Join(", ", r.Topics) : "None listed")}");
        Console.WriteLine($"URL: {r.HtmlUrl}");
    }

    public static string Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return "No description provided";
        value = value.Replace('\n', ' ').Replace('\r', ' ');
        return value.Length <= max ? value : value[..(max - 3)] + "...";
    }
}
