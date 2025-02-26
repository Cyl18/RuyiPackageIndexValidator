using System.Diagnostics;
using Octokit;
using Octokit.Internal;

namespace RuyiPackageIndexValidator.URLCheckers;

internal class GithubReleaseChecker : URLCheckerBase
{
    private static GitHubClient githubClient = new GitHubClient(new Connection(new ProductHeaderValue("Cyl18"),
        new InMemoryCredentialStore(new Credentials(Token))));

    public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
    {
        try
        {
            var latest = await githubClient.Repository.Release.GetLatest(data.Url.Segments[1], data.Url.Segments[2]);
            if (latest is null)
            {
                return new URLCheckResult(CheckStatus.CannotFindRelease, "");
            }

            return data.Url.Segments[5] == latest.TagName
                ? new URLCheckResult(CheckStatus.AlreadyNewest, latest.HtmlUrl)
                : new URLCheckResult(CheckStatus.UpdateRequired, latest.HtmlUrl);
        }
        catch (Exception e)
        {
            Trace.Assert(e is ApiException, "zhule");
            return new URLCheckResult(CheckStatus.Failed, "");
        }
    }
}