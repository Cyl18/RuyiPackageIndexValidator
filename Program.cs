// See https://aka.ms/new-console-template for more information

using GammaLibrary.Extensions;
using RuyiPackageIndexValidator;
using RuyiPackageIndexValidator.URLCheckers;
using Semver;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Octokit;

if (!Directory.Exists(BoardImagePath))
{
    Console.WriteLine($"路径 {BoardImagePath} 不存在");
    return;
}
var versions = ManifestFilter.Run();



var packageIndexSingleDatas = versions.Select(x => PackageIndexTomlParser.ParseSingle(x.FilePath)).SelectMany(x => x).ToArray();
// foreach (var (path, packageUrl) in packageIndexSingleDatas)
// {
//     Console.WriteLine(packageUrl.URL);
// }

var sb = new StringBuilder();
var results = await WebLinkValidator.Validate(packageIndexSingleDatas);
var checkAll = (await URLCheckerBase.CheckAll(packageIndexSingleDatas, results)).ToArray();
//SupportMatrixValidator.Run(await AIMapper.Run(), checkAll);
sb.AppendLine("## packages-index 到上游的检查：");
sb.AppendLine();
sb.AppendLine("packages-index 的最后 commit: " + RepoDataGatherer.Run(PackagesIndexPath));
sb.AppendLine();
sb.AppendLine("| 状态 | 最新文件 | 源文件 | 文件路径 |");
sb.AppendLine("| :--------: | :-: | :-: | :-: |");

foreach (var urlCheckResultse in checkAll.GroupBy(x => x.CheckStatus).OrderBy(x => x.Key))
{
    foreach (var (checkStatus, newestVersionFileName, (path, packageUrl)) in urlCheckResultse)
    {
        var relativePath = Path.GetRelativePath(BoardImagePath, path);
        sb.Append("| ");
        sb.Append(checkStatus switch
        {
            CheckStatus.AlreadyNewest => "√ 已经最新",
            CheckStatus.Failed => "⚠ 检查失败",
            CheckStatus.ManualCheckRequired => "🤚 手动检查",
            CheckStatus.UpdateRequired => "⬆️ 需要更新",
            CheckStatus.CannotFindRelease404 => "× 包不存在 404",
            CheckStatus.CannotFindRelease403 => "× 包不存在 403",
            CheckStatus.ImplementationNotNeeded => "❔ 无需实现",
            CheckStatus.InDev => "🚧 正在实现",
            _ => throw new ArgumentOutOfRangeException()
        });
        sb.Append($" | {newestVersionFileName} |");
        //sb.Append($" <{packageUrl.UnparsedURL}> / {packageUrl.URL} |");
        sb.Append($" {packageUrl.URL} |");
        sb.Append($" {relativePath} |");
        sb.AppendLine();
    }
}

sb.AppendLine();

{
    var amount = checkAll.Count(x => x.CheckStatus == CheckStatus.ManualCheckRequired);
    if (amount > 0)
    {
        sb.AppendLine($"有 {amount} 个包需要手动检查更新；  \n");
    }
}
{
    var amount = checkAll.Count(x => x.CheckStatus == CheckStatus.InDev);
    if (amount > 0)
    {
        sb.AppendLine($"有 {amount} 个包没有实现检查更新，需要人工开发；  \n");
    }
}
{
    var amount = checkAll.Count(x => x.CheckStatus == CheckStatus.Failed);
    if (amount > 0)
    {
        sb.AppendLine($"有 {amount} 个包检查更新失败；  \n");
    }
}
sb.AppendLine($"有 {checkAll.Count(x => x.CheckStatus == CheckStatus.UpdateRequired)} 个包需要更新；  \n" +
              $"有 {checkAll.Count(x => x.CheckStatus == CheckStatus.CannotFindRelease404)} 个包出现404错误；  \n" +
              $"有 {checkAll.Count(x => x.CheckStatus == CheckStatus.CannotFindRelease403)} 个包出现403错误；  \n" +
              $"有 {checkAll.Count(x => x.CheckStatus == CheckStatus.AlreadyNewest)} 个包已经最新；  \n" +
              $"有 {checkAll.Count(x => x.CheckStatus == CheckStatus.ImplementationNotNeeded)} 个包无需自动检查更新。  \n" );

var result2 = SupportMatrixValidator.Run(await AIMapper.Run(), checkAll);
sb.AppendLine();
sb.AppendLine("---");
sb.AppendLine();
sb.AppendLine("## packages-index 到 support-matrix 的检查");
sb.AppendLine();
sb.AppendLine("support-matrix 的最后 commit: " + RepoDataGatherer.Run(SupportMatrixRootPath));
sb.AppendLine();
sb.AppendLine("| 状态 | 名称 | 路径 | 包名 | 版本号 (index/matrix) |");
sb.AppendLine("| :--------: | :-: | :-: | :-: | :-: |");
foreach (var obj in result2.GroupBy(x => x.Result).OrderBy(x => x.Key))
{
    foreach (var (validateResult, package, ((displayName, packages), dirName), (manifestVersion, supportMatrixVersion)) in obj)
    {
        sb.Append("| ");
        sb.Append(validateResult switch
        {
            SupportMatrixValidateResults.DirNotFound => "❔ 找不到对应文件夹",
            SupportMatrixValidateResults.VersionNotExist => "⚠ 没有填写版本号",
            SupportMatrixValidateResults.VersionMismatch => "× 版本不匹配",
            SupportMatrixValidateResults.VersionTheSame => "√ 版本相同",
            _ => throw new ArgumentOutOfRangeException()
        });
        sb.Append(" | ");
        sb.Append(displayName + " | ");
        sb.Append(dirName + " | ");
        sb.Append(package + " | ");
        sb.Append($"{manifestVersion} / {supportMatrixVersion} |");
        sb.AppendLine();
    }
}

sb.AppendLine();
sb.AppendLine($"有 {result2.Count(x => x.Result == SupportMatrixValidateResults.VersionMismatch)} 个包版本不匹配；  \n" +
              $"有 {result2.Count(x => x.Result == SupportMatrixValidateResults.DirNotFound)} 个包没有找到对应的文件夹；  \n  " +
              $"有 {result2.Count(x => x.Result == SupportMatrixValidateResults.VersionNotExist)} 个包没有填写版本号；  \n  "+
              $"有 {result2.Count(x => x.Result == SupportMatrixValidateResults.VersionTheSame)} 个包版本相同。  \n  ");





sb.ToString().SaveToFile("result.md");


var dateTime = DateTime.Now.ToString("s");
 var gist = await GitHubReleaseChecker.githubClient.Gist.Create(new NewGist()
 {
     Description = $"Ruyi Package Index Test Report-{dateTime}",
     Files = { new KeyValuePair<string, string>($"ruyi-package-index-test-report-{dateTime}.md", sb.ToString()) },
     Public = true
 });
 Console.WriteLine();
 Console.WriteLine(gist.HtmlUrl);

// await RuyiDistMirrorChecker.GetAllFiles();
// var validateResult = await WebLinkValidator.Validate(packageIndexSingleDatas);
// WebLinkValidator.Print(validateResult);

