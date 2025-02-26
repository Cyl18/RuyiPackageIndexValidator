using GammaLibrary.Extensions;
using HtmlAgilityPack;
using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator.URLCheckers
{
    internal class OpenWrtChecker : URLCheckerBase
    {
        public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
        {
            var version = SemVersion.Parse(data.Url.Segments[3], SemVersionStyles.AllowLeadingZeros);
            var allFiles = await GetAllFiles();
            var netDist = allFiles.Where(x => !x.IsPrerelease).OrderByDescending(x => x, new SemVerComparer()).First();
            if (new SemVerComparer().Compare(netDist, version) > 0)
            {
                return new URLCheckResult(CheckStatus.UpdateRequired, "https://mirrors.tuna.tsinghua.edu.cn/openwrt/releases/" + version, data);
            }
            else if (netDist.Equals(version))
            {
                return new URLCheckResult(CheckStatus.AlreadyNewest, null, data);

            }
            else
            {
                Trace.Assert(false, "检测到的最新版本比当前版本还老");
                return null;
            }

        }


        public static async Task<SemVersion[]> GetAllFiles()
        {
            var doc = new HtmlDocument();
            var html = await hc.GetStringAsync("https://mirrors.tuna.tsinghua.edu.cn/openwrt/releases/");
            doc.LoadHtml(html);
            var trs = doc.DocumentNode.SelectNodes("/html/body/div[2]/div[2]/div/div/table/tbody/tr");
            var list = new List<SemVersion>();
            foreach (var node in trs)
            {
                var anode = node.SelectSingleNode("td[1]/a");
                var fileName = anode.GetAttributeValue("title", "null");
                if (SemVersion.TryParse(fileName, out var ver))
                {
                    list.Add(ver);
                }
            }

            return list.ToArray();
        }

        public class SemVerComparer : IComparer<SemVersion>
        {
            public int Compare(SemVersion? x, SemVersion? y)
            {
                if (x.Major != y.Major)
                {
                    return x.Major.CompareTo(y.Major);
                }
                if (x.Minor != y.Minor)
                {
                    return x.Minor.CompareTo(y.Minor);
                }
                if (x.Patch != y.Patch)
                {
                    return x.Patch.CompareTo(y.Patch);
                }

                return 0;
            }
        }
    }
}
