/* Jeff O'Hara
 * 7/24/2026
 * 
 * GitHub Project Discovery
 * 
 * This program is designed to discover and analyze GitHub projects based on specific criteria.
 * It utilizes the Octokit library to interact with the GitHub API and retrieve project information.
 * The program can be configured to search for projects based on topics, stars, forks, and other parameters.
 * 
 * Usage:
 * 1. Configure the search parameters in the AppManager class.
 * 2. Run the program to discover GitHub projects that match the criteria.
 * 3. The results will be displayed in the console or saved to a file as needed.
 */

using GitHubProjectDiscovery.Services;

var app = new AppManager();
await app.RunAsync();
