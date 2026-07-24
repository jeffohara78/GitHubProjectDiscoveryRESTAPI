using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class IssueSummary
{
    [JsonPropertyName("number")] public int Number { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
    [JsonPropertyName("comments")] public int Comments { get; set; }
    [JsonPropertyName("user")] public IssueUser User { get; set; } = new();
    [JsonPropertyName("pull_request")] public object? PullRequest { get; set; }
    public bool IsPullRequest => PullRequest is not null;
}

public class IssueUser
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
}
