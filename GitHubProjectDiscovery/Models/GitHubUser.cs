using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class GitHubUser
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("bio")] public string? Bio { get; set; }
    [JsonPropertyName("company")] public string? Company { get; set; }
    [JsonPropertyName("location")] public string? Location { get; set; }
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("public_repos")] public int PublicRepos { get; set; }
    [JsonPropertyName("followers")] public int Followers { get; set; }
    [JsonPropertyName("following")] public int Following { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}
