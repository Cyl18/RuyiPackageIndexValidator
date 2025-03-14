using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GammaLibrary.Extensions;

namespace RuyiPackageIndexValidator.URLCheckers
{
    internal class RuyiOpenEulerLpi4aChecker : URLCheckerBase
    {
        public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
        {
            var fileName = data.Url.Segments.Last();
            if (fileName.StartsWith("u-boot"))
            {
                return new URLCheckResult(CheckStatus.ImplementationNotNeeded, "", data);
            }
            var regex = new Regex(@"\d{8}-\d{6}");
            var match = regex.Match(fileName);
            if (!match.Success) throw new Exception();
            var version = match.Value.Replace("-", "").ToLong();
            long[] versions;
            if (fileName.StartsWith("boot-"))
            {
                versions = await GetAllFiles(data.Url.URL, "boot-");
            } 
            else if (fileName.StartsWith("root-"))
            {
                versions = await GetAllFiles(data.Url.URL, "root-");
            }
            else
            {
                throw new Exception();
            }

            var newest = versions.Max();
            if (newest != version)
            {
                var date = newest.ToString().Substring(0, 8);
                var subdate = newest.ToString().Substring(8, 6);
                return new URLCheckResult(CheckStatus.UpdateRequired, date + "-" + subdate, data);
            }
            else
            {
                return new URLCheckResult(CheckStatus.AlreadyNewest, "", data);
            }

        }
        private static string html;

        public static async Task<long[]> GetAllFiles(string url, string filter)
        {
            var doc = new HtmlDocument();
            var requestUri = Path.GetDirectoryName(url).Replace("\\", "/").Replace("https:/", "https://") + "/";

            html = await hc.GetStringAsync(requestUri);
            doc.LoadHtml(html);
            var trs = doc.DocumentNode.SelectNodes("/html/body/table/tbody/tr");
            var list = new List<long>();
            foreach (var node in trs.Skip(2))
            {
                var fileName = node.SelectSingleNode("td[1]/a").GetAttributeValue("title", "null");
                if (!fileName.StartsWith(filter))
                {
                    continue;
                }
                var regex = new Regex(@"\d{8}-\d{6}");
                var match = regex.Match(fileName);
                if (match.Success)
                {
                    list.Add(match.Value.Replace("-", "").ToLong());

                }
            }

            return list.ToArray();
        }


    }
}
