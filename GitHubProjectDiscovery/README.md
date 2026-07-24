# GitHub Project Discovery & Repository Inspector

A .NET 8 console application that uses the GitHub REST API to discover, inspect, compare, and save public software repositories. The discovery-first menu works without knowing a GitHub username.

## Features
- Keyword repository search
- Popular recent project discovery
- Programming-language search
- Optional user profile lookup
- Repository details, commits, issues, contributors, and language usage
- Side-by-side repository comparisons
- JSON favorites persistence
- In-memory API response caching
- Rate-limit display and friendly error handling

## Run
1. Open `GitHubProjectDiscovery.csproj` in Visual Studio 2022.
2. Confirm the .NET 8 SDK is installed.
3. Build and run with `Ctrl+F5`.

Public data works without authentication. GitHub allows fewer unauthenticated requests, so you may optionally set a fine-grained personal access token in the `GITHUB_TOKEN` environment variable. Never hard-code or commit a token.

Windows PowerShell example for the current session:
```powershell
$env:GITHUB_TOKEN="your_token_here"
dotnet run
```

## Architecture
- `Program.cs` — minimal entry point
- `Services/AppManager.cs` — menus and user workflow
- `Services/GitHubApiService.cs` — HTTP/API communication
- `Services/MemoryCacheService.cs` — short-lived response caching
- `Models/` — JSON response and persistence models
- `Persistence/FavoritesRepository.cs` — JSON file storage
- `Utilities/` — input validation and console presentation

## Important GitHub behavior
The repository issues endpoint also returns pull requests. This application filters those entries so the Open Issues screen displays issues only.
