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
foreach (var packageIndexSingleData in packageIndexSingleDatas)
{
    if (packageIndexSingleData.Url.URL.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/dist/")
        || packageIndexSingleData.Url.URL.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/3rdparty/")
        || packageIndexSingleData.Url.URL.StartsWith("https://mirror.iscas.ac.cn/openeuler-sig-riscv/openEuler-RISC-V/preview/openEuler-23.09-V1-riscv64/"))
    {
        var check = new RuyiDistMirrorChecker().Check(packageIndexSingleData);
        //Console.WriteLine($"{Path.GetFileName(packageIndexSingleData.Url.URL)}: {check.Result.CheckStatus}");
    }
    else
    {
        Console.WriteLine(packageIndexSingleData.Url.URL);
    }
}
// await RuyiDistMirrorChecker.GetAllFiles();
// var validateResult = await WebLinkValidator.Validate(packageIndexSingleDatas);
// WebLinkValidator.Print(validateResult);

Debugger.Break();