// See https://aka.ms/new-console-template for more information

using GammaLibrary.Extensions;
using RuyiPackageIndexValidator;
using RuyiPackageIndexValidator.URLCheckers;
using Semver;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

var versions = ManifestFilter.Run();



var packageIndexSingleDatas = versions.Select(x => PackageIndexTomlParser.ParseSingle(x.FilePath)).SelectMany(x => x).ToArray();
// foreach (var (path, packageUrl) in packageIndexSingleDatas)
// {
//     Console.WriteLine(packageUrl.URL);
// }
var sb = new StringBuilder();
var checkAll = await URLCheckerBase.CheckAll(packageIndexSingleDatas);
var results = await WebLinkValidator.Validate(packageIndexSingleDatas);

foreach (var (checkStatus, newestVersionFileName, packageIndexSingleData) in checkAll)
{
    if (checkStatus == CheckStatus.InDev && results.First(x => x.PackageIndexSingleData == packageIndexSingleData).IsSuccessStatusCode)
    {
        Console.WriteLine(packageIndexSingleData.Url.URL);
    }
}

// await RuyiDistMirrorChecker.GetAllFiles();
// var validateResult = await WebLinkValidator.Validate(packageIndexSingleDatas);
// WebLinkValidator.Print(validateResult);

Debugger.Break();