using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class CommitSummary
{
    [JsonPropertyName("sha")] public string Sha { get; set; } = string.Empty;
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("commit")] public CommitDetails Commit { get; set; } = new();
    [JsonPropertyName("author")] public CommitAccount? Author { get; set; }
}

public class CommitDetails
{
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("author")] public CommitAuthor Author { get; set; } = new();
}

public class CommitAuthor
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("date")] public DateTimeOffset Date { get; set; }
}

public class CommitAccount
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
}
