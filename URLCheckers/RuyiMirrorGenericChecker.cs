using GammaLibrary.Extensions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator.URLCheckers
{
    internal class RuyiMirrorGenericChecker : URLCheckerBase
    {
        public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
        {
            var versions = new List<(int Index, RuyiDistVersion Version)>();
            var i = 0;
            foreach (var urlSegment in data.Url.Segments)
            {
                var ver = new RuyiDistVersion(urlSegment, urlSegment);
                i++;
                if (ver.version == null && ver.date == null) continue;
                versions.Add((i, ver));
            }

            versions = versions.DistinctBy(x => x.Version).ToList();
            Trace.Assert(versions.Count == 1, $"无法检查版本号: {data.Url.URL}");
            var version = versions[0];
            if (version.Index == data.Url.Segments.Count)
            {
                return await new RuyiDistMirrorChecker().Check(data);
            }
            var reconstructedUrl = data.Url.Protocol + data.Url.Segments.Take(version.Index - 1).Connect("/") + "/";
            var allFiles = await GetAllFiles(reconstructedUrl);
            var netDist = allFiles.OrderByDescending(x => x).First();
            if (netDist > version.Version)
            {
                return new URLCheckResult(CheckStatus.UpdateRequired, reconstructedUrl + netDist.date, data);
            }
            else if (netDist == version.Version)
            {
                return new URLCheckResult(CheckStatus.AlreadyNewest, null, data);

            }
            else
            {
                Trace.Assert(false, "检测到的最新版本比当前版本还老");
                return null;
            }
        }

        public static async Task<RuyiDistVersion[]> GetAllFiles(string url)
        {
            var doc = new HtmlDocument();
            var html = await hc.GetStringAsync(url);
            doc.LoadHtml(html);
            var trs = doc.DocumentNode.SelectNodes("/html/body/table/tbody/tr");
            var list = new List<RuyiDistVersion>();
            foreach (var node in trs)
            {
                var anode = node.SelectSingleNode("td[1]/a");
                var fileName = anode.GetAttributeValue("title", "null");
                var ver = new RuyiDistVersion(fileName, fileName);
                if (ver.date != null || ver.version != null)
                {
                    list.Add(ver);
                }
            }

            return list.ToArray();
        }
    }


}
