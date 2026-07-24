using System.Text.Json;
using GitHubProjectDiscovery.Models;

namespace GitHubProjectDiscovery.Persistence;

public class FavoritesRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public FavoritesRepository()
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "favorites.json");
    }

    public async Task<List<FavoriteRepository>> LoadAsync()
    {
        if (!File.Exists(_filePath)) return [];
        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<FavoriteRepository>>(json, _options) ?? [];
        }
        catch (JsonException)
        {
            string backup = _filePath + $".corrupt-{DateTime.Now:yyyyMMddHHmmss}";
            File.Move(_filePath, backup, true);
            return [];
        }
    }

    public async Task SaveAsync(List<FavoriteRepository> favorites)
    {
        string json = JsonSerializer.Serialize(favorites.OrderBy(x => x.FullName), _options);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
