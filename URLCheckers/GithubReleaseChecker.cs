using System.Diagnostics;
using Octokit;
using Octokit.Internal;

namespace RuyiPackageIndexValidator.URLCheckers;

internal class GitHubReleaseChecker : URLCheckerBase
{
    internal static GitHubClient githubClient = new GitHubClient(new Connection(new ProductHeaderValue("Cyl18"),
        new InMemoryCredentialStore(new Credentials(Token))));

    public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
    {
        try
        {
            var latest = (await githubClient.Repository.Release.GetAll(data.Url.Segments[1], data.Url.Segments[2], new ApiOptions(){PageCount = 1, PageSize = 1}))[0];
            if (latest is null)
            {
                return new URLCheckResult(CheckStatus.CannotFindRelease404, "", data);
            }

            if (data.Url.Segments[5] == latest.TagName)
            {
                return new URLCheckResult(CheckStatus.AlreadyNewest, latest.HtmlUrl, data);
            }
            else
            {
                // Console.WriteLine($"UpdateRequired {data.Url.URL}\n {latest.HtmlUrl}");
                // Console.WriteLine();
                return new URLCheckResult(CheckStatus.UpdateRequired, latest.HtmlUrl, data);
            }
        }
        catch (Exception e)
        {
            Trace.Assert(e is ApiException, "zhule");
            return new URLCheckResult(CheckStatus.Failed, "", data);
        }
    }
}