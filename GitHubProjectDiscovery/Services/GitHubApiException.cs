namespace GitHubProjectDiscovery.Services;

public class GitHubApiException : Exception
{
    public int? StatusCode { get; }
    public GitHubApiException(string message, int? statusCode = null, Exception? inner = null) : base(message, inner)
    {
        StatusCode = statusCode;
    }
}
