using HtmlAgilityPack;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator.URLCheckers
{
    internal class CanmvChecker : URLCheckerBase
    {
        public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
        {
            //todo
            return new URLCheckResult(CheckStatus.AlreadyNewest, "", data);

        }

        public static async Task<string[]> GetAllFiles()
        {
            var doc = new HtmlDocument();
            var html = await hc.GetStringAsync("https://kendryte-download.canaan-creative.com/developer/k230/");
            doc.LoadHtml(html);
            var trs = doc.DocumentNode.SelectNodes("/html/body/pre/a");
            var list = new List<string>();
            foreach (var node in trs.Skip(1))
            {
                var fileName = node.GetAttributeValue("href", "null");
                list.Add(fileName);
            }

            return list.ToArray();
        }
    }
}
