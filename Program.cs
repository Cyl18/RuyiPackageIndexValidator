﻿// See https://aka.ms/new-console-template for more information

using GammaLibrary.Extensions;
using RuyiPackageIndexValidator;
using RuyiPackageIndexValidator.URLCheckers;
using Semver;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Octokit;

if (!Directory.Exists(RootPath))
{
    Console.WriteLine($"路径 {RootPath} 不存在");
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
sb.AppendLine("| 状态 | 最新文件 | 源文件 | 文件路径 |");
sb.AppendLine("| :--------: | :-: | :-: | :-: |");

foreach (var urlCheckResultse in checkAll.GroupBy(x => x.CheckStatus).OrderBy(x => x.Key))
{
    foreach (var (checkStatus, newestVersionFileName, (path, packageUrl)) in urlCheckResultse)
    {
        var relativePath = Path.GetRelativePath(RootPath, path);
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

var dateTime = DateTime.Now.ToString("s");
// var gist = await GitHubReleaseChecker.githubClient.Gist.Create(new NewGist()
// {
//     Description = $"Ruyi Package Index Test Report-{dateTime}",
//     Files = { new KeyValuePair<string, string>($"ruyi-package-index-test-report-{dateTime}.md", sb.ToString()) },
//     Public = true
// });
// Console.WriteLine();
// Console.WriteLine(gist.HtmlUrl);

var result2 = SupportMatrixValidator.Run(await AIMapper.Run(), checkAll);
sb.AppendLine();
sb.AppendLine("---");
sb.AppendLine();
sb.AppendLine("## packages-index 到 support-matrix 的检查");
sb.AppendLine("| 状态 | 名称 | 路径 | 包名 | 版本号 |");
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






sb.ToString().SaveToFile("result.md");




// await RuyiDistMirrorChecker.GetAllFiles();
// var validateResult = await WebLinkValidator.Validate(packageIndexSingleDatas);
// WebLinkValidator.Print(validateResult);

