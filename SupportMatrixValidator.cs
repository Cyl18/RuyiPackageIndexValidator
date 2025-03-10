using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GammaLibrary.Extensions;

namespace RuyiPackageIndexValidator
{
    public record SupportMatrixValidateResult(
        SupportMatrixValidateResults Result,
        string Package,
        SupportMatrixImageResultData supportMatrixImageResultData,
        (string ManifestVersion, string SupportMatrixVersion) Version);
    public enum SupportMatrixValidateResults
    {
        VersionMismatch,
        DirNotFound,
        VersionNotExist,
        VersionTheSame
    }
    internal class SupportMatrixValidator
    {
        static string StripNonDigit(string s)
        {
            var list = new List<char>(s);
            list.RemoveAll(x => !char.IsDigit(x));
            return new string(list.ToArray());
        }


        public static List<SupportMatrixValidateResult> Run(SupportMatrixImageResultData[] supportMatrixImageResultDatas, URLCheckResult[] urlCheckResults)
        {
            var result = new List<SupportMatrixValidateResult>();
            foreach (var obj in supportMatrixImageResultDatas)
            {
                var (supportMatrixImageSingleData, dirName) = obj;
                foreach (var package in supportMatrixImageSingleData.Packages)
                {
                    if (dirName != null)
                    {
                        var filePath = Path.Combine(SupportMatrixRootPath, dirName, "README.md");
                        var fileLines = File.ReadAllLines(filePath);

                        var manifestPath = urlCheckResults
                            .FirstOrDefault(x => x.PackageIndexSingleData.Path.Contains(package.Replace("board-image/", "")));
                        var manifestFileName = Path.GetFileNameWithoutExtension(manifestPath.PackageIndexSingleData.Path);

                        if (manifestFileName.Contains("-"))
                        {
                            manifestFileName = manifestFileName.Split('-')[0];
                        }

                        var regex = new Regex("^sys_ver: (.*)$");
                        string? version = null;
                        foreach (var line in fileLines)
                        {
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                version = match.Groups[1].Value;
                                if (version == "null") version = null;
                                break;
                            }
                        }

                        if (version == null)
                        {
                            result.Add(new SupportMatrixValidateResult(SupportMatrixValidateResults.VersionNotExist, package, obj, (manifestFileName, "null")));
                            continue;
                        }
                        version = version?.Trim('"')?.TrimStart('v').Split('-')[0].Split('+')[0];
                        var regex1 = new Regex(@"^0.(\d{8}).0$");
                        var match1 = regex1.Match(manifestFileName);
                        if (match1.Success)
                        {
                            manifestFileName = match1.Groups[1].Value;
                        }

                        var regex2 = new Regex(@"^0.(\d{4}).([012])$");
                        var match2 = regex2.Match(manifestFileName);
                        if (match2.Success)
                        {
                            var rs = "";
                            var g1 = match2.Groups[1].Value;
                            var g2 = match2.Groups[2].Value;
                            rs += g1[0];
                            rs += g1[1];
                            rs += ".";
                            rs += g1[2];
                            rs += g1[3];
                            if (g2.ToInt() != 0)
                            {
                                rs += ".";
                                rs += g2;
                            }

                            manifestFileName = rs;
                        }

                        bool matched = manifestFileName.Replace(".","").Contains(version.Replace(".", ""));

                        result.Add(new SupportMatrixValidateResult(matched ? SupportMatrixValidateResults.VersionTheSame : SupportMatrixValidateResults.VersionMismatch, package, obj, (manifestFileName, version)));
                        //Console.WriteLine(manifestFileName + " | \t" + version + $"\t | {matched}");
                    }
                    else
                    {
                        result.Add(new SupportMatrixValidateResult(SupportMatrixValidateResults.DirNotFound, "", obj, ("null", "null")));
                    }
                }
                

            }

            return result;
        }
    }
}
