namespace GitHubProjectDiscovery.Models;

public class FavoriteRepository
{
    public string FullName { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Language { get; set; }
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.Now;
}
