using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class RepositorySummary
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("full_name")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("stargazers_count")] public int Stars { get; set; }
    [JsonPropertyName("forks_count")] public int Forks { get; set; }
    [JsonPropertyName("open_issues_count")] public int OpenIssues { get; set; }
    [JsonPropertyName("watchers_count")] public int Watchers { get; set; }
    [JsonPropertyName("size")] public int SizeKb { get; set; }
    [JsonPropertyName("default_branch")] public string DefaultBranch { get; set; } = string.Empty;
    [JsonPropertyName("archived")] public bool Archived { get; set; }
    [JsonPropertyName("fork")] public bool IsFork { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
    [JsonPropertyName("pushed_at")] public DateTimeOffset? PushedAt { get; set; }
    [JsonPropertyName("owner")] public RepositoryOwner Owner { get; set; } = new();
    [JsonPropertyName("license")] public RepositoryLicense? License { get; set; }
    [JsonPropertyName("topics")] public List<string> Topics { get; set; } = [];
}

public class RepositoryOwner
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
}

public class RepositoryLicense
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("spdx_id")] public string? SpdxId { get; set; }
}
