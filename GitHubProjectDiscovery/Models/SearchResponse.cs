using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class SearchResponse
{
    [JsonPropertyName("total_count")] public int TotalCount { get; set; }
    [JsonPropertyName("incomplete_results")] public bool IncompleteResults { get; set; }
    [JsonPropertyName("items")] public List<RepositorySummary> Items { get; set; } = [];
}
