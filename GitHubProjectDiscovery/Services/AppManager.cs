using GitHubProjectDiscovery.Models;
using GitHubProjectDiscovery.Persistence;
using GitHubProjectDiscovery.Utilities;

namespace GitHubProjectDiscovery.Services;

public class AppManager
{
    private readonly GitHubApiService _api = new();
    private readonly FavoritesRepository _favoritesRepository = new();
    private List<FavoriteRepository> _favorites = [];

    public async Task RunAsync()
    {
        _favorites = await _favoritesRepository.LoadAsync();
        bool running = true;
        while (running)
        {
            ShowMainMenu();
            int choice = InputHelper.ReadMenuChoice(0, 9);
            try
            {
                switch (choice)
                {
                    case 1: await SearchAsync(); break;
                    case 2: await BrowsePopularAsync(); break;
                    case 3: await SearchByLanguageAsync(); break;
                    case 4: await ViewUserAsync(); break;
                    case 5: await InspectByNameAsync(); break;
                    case 6: await CompareAsync(); break;
                    case 7: await AnalyzeLanguagesAsync(); break;
                    case 8: await ManageFavoritesAsync(); break;
                    case 9: await ShowRateLimitsAsync(); break;
                    case 0: running = false; break;
                }
            }
            catch (GitHubApiException ex)
            {
                Console.WriteLine($"\nAPI error: {ex.Message}");
                InputHelper.Pause();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnexpected error: {ex.Message}");
                InputHelper.Pause();
            }
        }
        _api.Dispose();
        Console.WriteLine("Thank you for using GitHub Project Discovery.");
    }

    private static void ShowMainMenu()
    {
        ConsoleDisplay.Header("GitHub Project Discovery & Repository Inspector");
        Console.WriteLine("Discover projects without needing to know a GitHub username.\n");
        Console.WriteLine("1. Search repositories by keyword");
        Console.WriteLine("2. Browse popular recent repositories");
        Console.WriteLine("3. Search by programming language");
        Console.WriteLine("4. View a GitHub user profile");
        Console.WriteLine("5. Inspect a repository by owner/name");
        Console.WriteLine("6. Compare two repositories");
        Console.WriteLine("7. Analyze a repository's language usage");
        Console.WriteLine("8. Manage favorite repositories");
        Console.WriteLine("9. View GitHub API usage limits");
        Console.WriteLine("0. Exit\n");
    }

    private async Task SearchAsync()
    {
        ConsoleDisplay.Header("Search Repositories");
        string query = InputHelper.ReadRequired("Search words (example: C# baseball): ");
        string? language = InputHelper.ReadOptional("Optional language filter (press Enter to skip): ");
        Console.WriteLine("Sort by: 1. Best match  2. Most stars  3. Recently updated");
        int sortChoice = InputHelper.ReadMenuChoice(1, 3);
        string sort = sortChoice switch { 2 => "stars", 3 => "updated", _ => string.Empty };
        SearchResponse result = await _api.SearchRepositoriesAsync(query, language, sort);
        await ChooseAndInspectAsync(result.Items, $"Found {result.TotalCount:N0} matching repositories");
    }

    private async Task BrowsePopularAsync()
    {
        ConsoleDisplay.Header("Popular Recent Repositories");
        string? language = InputHelper.ReadOptional("Optional language filter (press Enter for all): ");
        SearchResponse result = await _api.DiscoverPopularAsync(language);
        await ChooseAndInspectAsync(result.Items, "Popular repositories created within the last year");
    }

    private async Task SearchByLanguageAsync()
    {
        ConsoleDisplay.Header("Search by Programming Language");
        string language = InputHelper.ReadRequired("Language (example: C#, Python, Java): ");
        string? keywords = InputHelper.ReadOptional("Optional project keywords (press Enter to browse): ");
        SearchResponse result = await _api.SearchRepositoriesAsync(keywords ?? "stars:>=10", language, "stars");
        await ChooseAndInspectAsync(result.Items, $"Top {language} repositories");
    }

    private async Task ChooseAndInspectAsync(List<RepositorySummary> repositories, string heading)
    {
        ConsoleDisplay.Header(heading);
        ConsoleDisplay.RepositoryList(repositories);
        if (repositories.Count == 0) { InputHelper.Pause(); return; }
        int selection = InputHelper.ReadSelection(repositories.Count);
        if (selection == 0) return;
        await RepositoryMenuAsync(repositories[selection - 1].Owner.Login, repositories[selection - 1].Name);
    }

    private async Task ViewUserAsync()
    {
        ConsoleDisplay.Header("GitHub User Profile");
        string username = InputHelper.ReadRequired("GitHub username: ");
        GitHubUser user = await _api.GetUserAsync(username);
        Console.WriteLine($"Name: {user.Name ?? "Not provided"}");
        Console.WriteLine($"Username: {user.Login}");
        Console.WriteLine($"Bio: {user.Bio ?? "Not provided"}");
        Console.WriteLine($"Company: {user.Company ?? "Not provided"}");
        Console.WriteLine($"Location: {user.Location ?? "Not provided"}");
        Console.WriteLine($"Public repositories: {user.PublicRepos:N0}");
        Console.WriteLine($"Followers: {user.Followers:N0} | Following: {user.Following:N0}");
        Console.WriteLine($"Member since: {user.CreatedAt:MMM d, yyyy}");
        Console.WriteLine($"Profile: {user.HtmlUrl}\n");
        List<RepositorySummary> repos = await _api.GetUserRepositoriesAsync(username);
        ConsoleDisplay.RepositoryList(repos.Take(15).ToList());
        if (repos.Count > 0)
        {
            int selection = InputHelper.ReadSelection(Math.Min(15, repos.Count));
            if (selection > 0) await RepositoryMenuAsync(repos[selection - 1].Owner.Login, repos[selection - 1].Name);
        }
        else InputHelper.Pause();
    }

    private async Task InspectByNameAsync()
    {
        ConsoleDisplay.Header("Inspect Repository");
        var name = InputHelper.ReadRepositoryName();
        if (name is not null) await RepositoryMenuAsync(name.Value.Owner, name.Value.Repo);
        else InputHelper.Pause();
    }

    private async Task RepositoryMenuAsync(string owner, string repo)
    {
        bool viewing = true;
        while (viewing)
        {
            RepositorySummary details = await _api.GetRepositoryAsync(owner, repo);
            ConsoleDisplay.Header(details.FullName);
            ConsoleDisplay.RepositoryDetails(details);
            Console.WriteLine("\n1. View recent commits");
            Console.WriteLine("2. View open issues");
            Console.WriteLine("3. View top contributors");
            Console.WriteLine("4. View language breakdown");
            Console.WriteLine("5. Add or remove favorite");
            Console.WriteLine("0. Return\n");
            switch (InputHelper.ReadMenuChoice(0, 5))
            {
                case 1: await ShowCommitsAsync(owner, repo); break;
                case 2: await ShowIssuesAsync(owner, repo); break;
                case 3: await ShowContributorsAsync(owner, repo); break;
                case 4: await ShowLanguagesAsync(owner, repo); break;
                case 5: await ToggleFavoriteAsync(details); break;
                case 0: viewing = false; break;
            }
        }
    }

    private async Task ShowCommitsAsync(string owner, string repo)
    {
        ConsoleDisplay.Header("Recent Commits");
        List<CommitSummary> commits = await _api.GetCommitsAsync(owner, repo);
        if (commits.Count == 0) Console.WriteLine("No commits were returned.");
        foreach (CommitSummary c in commits)
        {
            string firstLine = c.Commit.Message.Split('\n')[0];
            Console.WriteLine($"{c.Sha[..Math.Min(7, c.Sha.Length)]} | {c.Commit.Author.Date:MMM d, yyyy} | {c.Author?.Login ?? c.Commit.Author.Name}");
            Console.WriteLine($"  {ConsoleDisplay.Trim(firstLine, 90)}");
        }
        InputHelper.Pause();
    }

    private async Task ShowIssuesAsync(string owner, string repo)
    {
        ConsoleDisplay.Header("Open Issues");
        List<IssueSummary> issues = await _api.GetIssuesAsync(owner, repo);
        if (issues.Count == 0) Console.WriteLine("No open issues were returned. Pull requests are excluded.");
        foreach (IssueSummary issue in issues)
        {
            Console.WriteLine($"#{issue.Number} {ConsoleDisplay.Trim(issue.Title, 85)}");
            Console.WriteLine($"  Opened by {issue.User.Login} | Updated {issue.UpdatedAt:MMM d, yyyy} | Comments: {issue.Comments}");
        }
        InputHelper.Pause();
    }

    private async Task ShowContributorsAsync(string owner, string repo)
    {
        ConsoleDisplay.Header("Top Contributors");
        List<ContributorSummary> contributors = await _api.GetContributorsAsync(owner, repo);
        if (contributors.Count == 0) Console.WriteLine("No contributor information was returned.");
        for (int i = 0; i < contributors.Count; i++)
            Console.WriteLine($"{i + 1,2}. {contributors[i].Login,-25} {contributors[i].Contributions,8:N0} contributions");
        InputHelper.Pause();
    }

    private async Task ShowLanguagesAsync(string owner, string repo)
    {
        ConsoleDisplay.Header("Language Breakdown");
        Dictionary<string, long> languages = await _api.GetLanguagesAsync(owner, repo);
        long total = languages.Values.Sum();
        if (total == 0) Console.WriteLine("No language data was returned.");
        foreach (var item in languages.OrderByDescending(x => x.Value))
        {
            double percent = item.Value * 100.0 / total;
            Console.WriteLine($"{item.Key,-25} {percent,6:F1}%  ({item.Value:N0} bytes)");
        }
        InputHelper.Pause();
    }

    private async Task AnalyzeLanguagesAsync()
    {
        ConsoleDisplay.Header("Analyze Language Usage");
        var name = InputHelper.ReadRepositoryName();
        if (name is null) { InputHelper.Pause(); return; }
        await ShowLanguagesAsync(name.Value.Owner, name.Value.Repo);
    }

    private async Task CompareAsync()
    {
        ConsoleDisplay.Header("Compare Repositories");
        Console.WriteLine("First repository");
        var first = InputHelper.ReadRepositoryName();
        if (first is null) { InputHelper.Pause(); return; }
        Console.WriteLine("\nSecond repository");
        var second = InputHelper.ReadRepositoryName();
        if (second is null) { InputHelper.Pause(); return; }
        RepositorySummary a = await _api.GetRepositoryAsync(first.Value.Owner, first.Value.Repo);
        RepositorySummary b = await _api.GetRepositoryAsync(second.Value.Owner, second.Value.Repo);
        ConsoleDisplay.Header("Repository Comparison");
        Console.WriteLine($"{"Metric",-24}{a.FullName,-23}{b.FullName,-23}");
        Console.WriteLine(new string('-', 70));
        Row("Language", a.Language ?? "Unknown", b.Language ?? "Unknown");
        Row("Stars", a.Stars.ToString("N0"), b.Stars.ToString("N0"));
        Row("Forks", a.Forks.ToString("N0"), b.Forks.ToString("N0"));
        Row("Open issues/PRs", a.OpenIssues.ToString("N0"), b.OpenIssues.ToString("N0"));
        Row("License", a.License?.SpdxId ?? "None", b.License?.SpdxId ?? "None");
        Row("Last updated", a.UpdatedAt.ToString("MMM d, yyyy"), b.UpdatedAt.ToString("MMM d, yyyy"));
        Row("Archived", a.Archived ? "Yes" : "No", b.Archived ? "Yes" : "No");
        InputHelper.Pause();
    }

    private static void Row(string label, string a, string b) =>
        Console.WriteLine($"{label,-24}{ConsoleDisplay.Trim(a, 20),-23}{ConsoleDisplay.Trim(b, 20),-23}");

    private async Task ToggleFavoriteAsync(RepositorySummary repository)
    {
        FavoriteRepository? existing = _favorites.FirstOrDefault(x => x.FullName.Equals(repository.FullName, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _favorites.Add(new FavoriteRepository { FullName = repository.FullName, HtmlUrl = repository.HtmlUrl, Description = repository.Description, Language = repository.Language });
            Console.WriteLine("Repository added to favorites.");
        }
        else
        {
            _favorites.Remove(existing);
            Console.WriteLine("Repository removed from favorites.");
        }
        await _favoritesRepository.SaveAsync(_favorites);
        InputHelper.Pause();
    }

    private async Task ManageFavoritesAsync()
    {
        ConsoleDisplay.Header("Favorite Repositories");
        if (_favorites.Count == 0) { Console.WriteLine("No favorites saved yet."); InputHelper.Pause(); return; }
        for (int i = 0; i < _favorites.Count; i++)
        {
            FavoriteRepository f = _favorites[i];
            Console.WriteLine($"{i + 1,2}. {f.FullName} | {f.Language ?? "Unknown"} | Saved {f.SavedAt:MMM d, yyyy}");
            Console.WriteLine($"    {ConsoleDisplay.Trim(f.Description, 86)}");
        }
        int selection = InputHelper.ReadSelection(_favorites.Count);
        if (selection == 0) return;
        string[] parts = _favorites[selection - 1].FullName.Split('/');
        await RepositoryMenuAsync(parts[0], parts[1]);
    }

    private async Task ShowRateLimitsAsync()
    {
        ConsoleDisplay.Header("GitHub API Usage Limits");
        RateLimitResponse limits = await _api.GetRateLimitAsync();
        Console.WriteLine($"Core requests:   {limits.Resources.Core.Remaining:N0} remaining of {limits.Resources.Core.Limit:N0}");
        Console.WriteLine($"Core resets:     {limits.Resources.Core.ResetAt:g}");
        Console.WriteLine($"Search requests: {limits.Resources.Search.Remaining:N0} remaining of {limits.Resources.Search.Limit:N0}");
        Console.WriteLine($"Search resets:   {limits.Resources.Search.ResetAt:g}");
        Console.WriteLine("\nOptional: set a GITHUB_TOKEN environment variable for authenticated requests.");
        InputHelper.Pause();
    }
}
