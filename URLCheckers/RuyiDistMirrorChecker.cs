using GammaLibrary.Extensions;
using HtmlAgilityPack;
using Semver;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RuyiPackageIndexValidator.URLCheckers;

internal class RuyiDistMirrorChecker : URLCheckerBase
{
    public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
    {
        var url = data.Url.URL;
        string fileName;
        if (url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/3rdparty/"))
        {
            fileName = url["https://mirror.iscas.ac.cn/ruyisdk/3rdparty/".Length..];

        }
        else if (url.StartsWith("https://mirror.iscas.ac.cn/openeuler-sig-riscv/openEuler-RISC-V/preview/openEuler-23.09-V1-riscv64/"))
        {
            fileName = url["https://mirror.iscas.ac.cn/openeuler-sig-riscv/openEuler-RISC-V/preview/openEuler-23.09-V1-riscv64/".Length..];
        }
        else
        {
            fileName = url["https://mirror.iscas.ac.cn/ruyisdk/dist/".Length..];

        }
        var softwareCheckerRegex = new Regex("^(.*?)-");
        var software = softwareCheckerRegex.Match(fileName).Groups[1].Value;
        var allFiles = await GetAllFiles();
        if (software == "qemu")
        {
            softwareCheckerRegex = new Regex("^(.*?-.*?)-");
            software = softwareCheckerRegex.Match(fileName).Groups[1].Value;
            if (fileName.Contains("xthead"))
            {
                software = "qemu-user-riscv-xthead";
            }
        }

        if (software == "llvm")
        {
            if (fileName.StartsWith("llvm-project"))
            {
                var versionResults = allFiles.Where(x =>
                    x.FullFileName.StartsWith("llvm-project")).ToArray();
                return CompareVersion(versionResults);
            } 
            else if (fileName.StartsWith("llvm-plct"))
            {
                var versionResults = allFiles.Where(x =>
                    x.FullFileName.StartsWith("llvm-plct")).ToArray();
                return CompareVersion(versionResults);
            }
            else
            {
                var versionResults = allFiles.Where(x =>
                    !x.FullFileName.Contains("-project") && !x.FullFileName.Contains("-plct") &&
                    x.FullFileName.StartsWith("llvm")).ToArray();
                return CompareVersion(versionResults);
            }
        }

        if (software.StartsWith("RuyiSDK"))
        {
            var regex = new Regex(@"RuyiSDK-\d{8}-((.*?)-(.*?)-(.*?)-(.*?)-(.*?)[^-]*)");
            var match = regex.Match(fileName);
            allFiles = allFiles.Where(x => x.FullFileName.Contains(match.Groups[1].Value)).ToArray();
            if (fileName.Contains("HOST-aarch64")) allFiles = allFiles.Where(x => x.FullFileName.Contains("HOST-aarch64")).ToArray();
            else allFiles = allFiles.Where(x => !x.FullFileName.Contains("HOST-aarch64")).ToArray();
            
            if (fileName.Contains("HOST-riscv64")) allFiles = allFiles.Where(x => x.FullFileName.Contains("HOST-riscv64")).ToArray();
            else allFiles = allFiles.Where(x => !x.FullFileName.Contains("HOST-riscv64")).ToArray();

        }


        return CompareVersion(allFiles.Where(x => x.FullFileName.StartsWith(software)).ToArray());

        URLCheckResult CompareVersion(VersionResult[] versions)
        {
            RuyiDistVersion currentNewestVersion = null;
            VersionResult currentNewestVersionResult = null;

            foreach (var ver in versions)
            {
                var (fullFileName, version, isDate) = ver;
                RuyiDistVersion distVersion;
                if (isDate)
                {
                    distVersion = new RuyiDistVersion(null, version);
                }
                else
                {
                    distVersion = new RuyiDistVersion(version, null);
                }

                if (version == null)
                {
                    var dateMatch = new Regex("(\\d{4}-?\\d{4})").Match(fullFileName);
                    if (dateMatch.Success)
                    {
                        distVersion = new RuyiDistVersion(null, dateMatch.Groups[0].Value.Replace("-", ""));
                    }
                    else
                    {
                        return new URLCheckResult(CheckStatus.NotImplemented, null);

                    }
                }
                if (currentNewestVersion == null)
                {
                    currentNewestVersion = distVersion;
                    currentNewestVersionResult = ver;
                }
                else
                {
                    if (distVersion > currentNewestVersion)
                    {
                        currentNewestVersion = distVersion;
                        currentNewestVersionResult = ver;
                    }
                }
            }

            if (currentNewestVersion == null)
            {
                return new URLCheckResult(CheckStatus.CannotFindRelease, null);
            }

            if (fileName == currentNewestVersionResult!.FullFileName)
            {
                return new URLCheckResult(CheckStatus.AlreadyNewest, null);
            }

            if (GetVersionResult(fileName) == currentNewestVersionResult)
            {
                return new URLCheckResult(CheckStatus.AlreadyNewest, null);
            }
            return new URLCheckResult(CheckStatus.UpdateRequired, currentNewestVersionResult.FullFileName);
        }

        throw new Exception();
    }

    private static string html;

    public RuyiDistMirrorChecker()
    {
        html = hc.GetStringAsync("https://mirror.iscas.ac.cn/ruyisdk/dist/").Result;
    }

    public static async Task<VersionResult[]> GetAllFiles()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var trs = doc.DocumentNode.SelectNodes("/html/body/table/tbody/tr");
        var list = new List<VersionResult>();
        foreach (var node in trs.Skip(2))
        {
            var fileName = node.SelectSingleNode("td[1]/a").GetAttributeValue("title", "null");
            
            var versionResult = GetVersionResult(fileName);
            list.Add(versionResult);
        }

        return list.ToArray();
    }

    private static VersionResult GetVersionResult(string fileName)
    {
        var match = GetFileVersion(fileName);
        var isTrueDate = match.Groups[6].Value.StartsWith("202");
        var isDate = match.Groups[6].Success;

        var versionResult = new VersionResult(fileName, !isTrueDate? null : isDate ? match.Groups[6].Value : match.Groups[4].Value, isDate && isTrueDate);
        return versionResult;
    }

    private static Match GetFileVersion(string fileName)
    {
        var regex = new Regex(@"([^-_]*[-_])?(((\d+\.\d+(\.\d+)?.*?)\.?[^.]*)|(\d{4}-?\d{4}))");
        var match = regex.Match(fileName);
        return match;
    }

    public class RuyiDistVersion : IComparable<RuyiDistVersion>, IComparable
    {

        public RuyiDistVersion(string? version, string? date)
        {
            this.version = version == null ? null : Version.Parse(version);
            this.date = date == null ? null : new Regex("(\\d{4}-?\\d{4})").Match(date).Groups[0].Value.Replace("-", "").ToInt();
        }

        public int CompareTo(RuyiDistVersion? other)
        {
            if (other == null)
            {
                return 1; // 当前对象大于 null
            }

            if (this.date != null)
            {
                return this.date!.Value.CompareTo(other.date!.Value);
            }

            // 比较 Major 版本

            if (this.version.Major != other.version.Major)
            {
                return this.version.Major.CompareTo(other.version.Major);
            }

            // 比较 Minor 版本

            if (this.version.Minor != other.version.Minor)
            {
                return this.version.Minor.CompareTo(other.version.Minor);
            }

            // 比较 Patch 版本

            if (this.version.Build != other.version.Build)
            {
                return this.version.Build.CompareTo(other.version.Build);
            }

            // 如果 Major、Minor 和 Patch 都相等，则两个版本相等
            throw new Exception("版本号相同");
        }

        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is RuyiDistVersion other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(RuyiDistVersion)}");
        }

        public static bool operator <(RuyiDistVersion? left, RuyiDistVersion? right)
        {
            return Comparer<RuyiDistVersion>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(RuyiDistVersion? left, RuyiDistVersion? right)
        {
            return Comparer<RuyiDistVersion>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(RuyiDistVersion? left, RuyiDistVersion? right)
        {
            return Comparer<RuyiDistVersion>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(RuyiDistVersion? left, RuyiDistVersion? right)
        {
            return Comparer<RuyiDistVersion>.Default.Compare(left, right) >= 0;
        }

        private Version? version;
        private int? date;
    }
}
public record VersionResult(string FullFileName, string? Version, bool IsDate);