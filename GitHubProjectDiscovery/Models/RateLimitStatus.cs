using System.Text.Json.Serialization;

namespace GitHubProjectDiscovery.Models;

public class RateLimitResponse
{
    [JsonPropertyName("resources")] public RateLimitResources Resources { get; set; } = new();
}
public class RateLimitResources
{
    [JsonPropertyName("core")] public RateLimitBucket Core { get; set; } = new();
    [JsonPropertyName("search")] public RateLimitBucket Search { get; set; } = new();
}
public class RateLimitBucket
{
    [JsonPropertyName("limit")] public int Limit { get; set; }
    [JsonPropertyName("remaining")] public int Remaining { get; set; }
    [JsonPropertyName("used")] public int Used { get; set; }
    [JsonPropertyName("reset")] public long ResetUnix { get; set; }
    public DateTimeOffset ResetAt => DateTimeOffset.FromUnixTimeSeconds(ResetUnix).ToLocalTime();
}
