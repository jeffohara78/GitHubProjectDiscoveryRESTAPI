using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class ContributorSummary
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
    [JsonPropertyName("contributions")] public int Contributions { get; set; }
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
}
