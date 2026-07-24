using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using GitHubProjectDiscovery.Models;

namespace GitHubProjectDiscovery.Services;

public class GitHubApiService : IDisposable
{
    private const string ApiVersion = "2026-03-10";
    private readonly HttpClient _client;
    private readonly MemoryCacheService _cache = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GitHubApiService()
    {
        _client = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubProjectDiscovery/1.0");
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        _client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", ApiVersion);

        string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
    }

    public Task<SearchResponse> SearchRepositoriesAsync(string query, string? language, string sort, int page = 1)
    {
        string qualified = query.Trim();
        if (!string.IsNullOrWhiteSpace(language)) qualified += $" language:\"{language.Trim()}\"";
        string sortQuery = string.IsNullOrWhiteSpace(sort) ? string.Empty : $"&sort={sort}&order=desc";
        string endpoint = $"search/repositories?q={Uri.EscapeDataString(qualified)}{sortQuery}&per_page=20&page={page}";
        return GetAsync<SearchResponse>(endpoint, TimeSpan.FromMinutes(5));
    }

    public Task<SearchResponse> DiscoverPopularAsync(string? language)
    {
        string createdAfter = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        string query = $"stars:>=100 created:>={createdAfter}";
        if (!string.IsNullOrWhiteSpace(language)) query += $" language:\"{language.Trim()}\"";
        string endpoint = $"search/repositories?q={Uri.EscapeDataString(query)}&sort=stars&order=desc&per_page=20";
        return GetAsync<SearchResponse>(endpoint, TimeSpan.FromMinutes(10));
    }

    public Task<RepositorySummary> GetRepositoryAsync(string owner, string repo) =>
        GetAsync<RepositorySummary>($"repos/{Escape(owner)}/{Escape(repo)}", TimeSpan.FromMinutes(10));

    public Task<GitHubUser> GetUserAsync(string username) =>
        GetAsync<GitHubUser>($"users/{Escape(username)}", TimeSpan.FromMinutes(10));

    public Task<List<RepositorySummary>> GetUserRepositoriesAsync(string username) =>
        GetAsync<List<RepositorySummary>>($"users/{Escape(username)}/repos?sort=updated&direction=desc&per_page=30", TimeSpan.FromMinutes(10));

    public Task<Dictionary<string, long>> GetLanguagesAsync(string owner, string repo) =>
        GetAsync<Dictionary<string, long>>($"repos/{Escape(owner)}/{Escape(repo)}/languages", TimeSpan.FromMinutes(10));

    public Task<List<CommitSummary>> GetCommitsAsync(string owner, string repo) =>
        GetAsync<List<CommitSummary>>($"repos/{Escape(owner)}/{Escape(repo)}/commits?per_page=10", TimeSpan.FromMinutes(5));

    public async Task<List<IssueSummary>> GetIssuesAsync(string owner, string repo)
    {
        List<IssueSummary> results = await GetAsync<List<IssueSummary>>($"repos/{Escape(owner)}/{Escape(repo)}/issues?state=open&sort=updated&per_page=20", TimeSpan.FromMinutes(5));
        return results.Where(x => !x.IsPullRequest).ToList();
    }

    public Task<List<ContributorSummary>> GetContributorsAsync(string owner, string repo) =>
        GetAsync<List<ContributorSummary>>($"repos/{Escape(owner)}/{Escape(repo)}/contributors?per_page=10", TimeSpan.FromMinutes(10));

    public Task<RateLimitResponse> GetRateLimitAsync() =>
        GetAsync<RateLimitResponse>("rate_limit", TimeSpan.FromSeconds(15));

    private async Task<T> GetAsync<T>(string endpoint, TimeSpan cacheDuration)
    {
        if (_cache.TryGet(endpoint, out string cached))
            return Deserialize<T>(cached);

        try
        {
            using HttpResponseMessage response = await _client.GetAsync(endpoint);
            string body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw BuildApiException(response, body);
            _cache.Set(endpoint, body, cacheDuration);
            return Deserialize<T>(body);
        }
        catch (TaskCanceledException ex)
        {
            throw new GitHubApiException("The GitHub request timed out. Check your internet connection and try again.", null, ex);
        }
        catch (HttpRequestException ex)
        {
            throw new GitHubApiException("GitHub could not be reached. Check your internet connection and try again.", null, ex);
        }
    }

    private T Deserialize<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? throw new JsonException("The response was empty."); }
        catch (JsonException ex) { throw new GitHubApiException("GitHub returned data the application could not read.", null, ex); }
    }

    private static GitHubApiException BuildApiException(HttpResponseMessage response, string body)
    {
        string message = response.StatusCode switch
        {
            HttpStatusCode.NotFound => "The requested GitHub user or repository was not found.",
            HttpStatusCode.Forbidden => "GitHub refused the request. You may have reached an API rate limit.",
            HttpStatusCode.UnprocessableEntity => "GitHub could not process that search. Try a simpler search term.",
            (HttpStatusCode)429 => "GitHub's API rate limit has been reached. Try again after the reset time.",
            _ => $"GitHub returned {(int)response.StatusCode} ({response.ReasonPhrase})."
        };
        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out JsonElement apiMessage))
                message += $" Details: {apiMessage.GetString()}";
        }
        catch (JsonException) { }
        return new GitHubApiException(message, (int)response.StatusCode);
    }

    private static string Escape(string segment) => Uri.EscapeDataString(segment.Trim());
    public void Dispose() => _client.Dispose();
}
