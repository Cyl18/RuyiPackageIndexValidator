using Octokit;
using Octokit.Internal;

namespace RuyiPackageIndexValidator.URLCheckers;

internal class GithubReleaseChecker : URLCheckerBase
{
    private static GitHubClient githubClient;

    public GithubReleaseChecker()
    {
        githubClient = new GitHubClient(new Connection(new ProductHeaderValue("Cyl18"),
            new InMemoryCredentialStore(new Credentials("TOKEN"))));
    }

    public override Task<URLCheckResult> Check(PackageIndexSingleData data)
    {

        throw new Exception();
    }

    private async Task<string> GetLatest(PackageUrl url)
    {
        var releases = await githubClient.Repository.Release.GetAll("octokit", "octokit.net");
        var latest = releases[0];
        Console.WriteLine(
            "The latest release is tagged at {0} and is named {1}",
            latest.TagName,
            latest.Name);
        throw new Exception();
    }
}