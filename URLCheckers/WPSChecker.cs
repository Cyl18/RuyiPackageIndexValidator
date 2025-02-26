using System.Diagnostics;
using HtmlAgilityPack;

namespace RuyiPackageIndexValidator.URLCheckers;

internal class WPSChecker : URLCheckerBase
{
    protected static HttpClient hc = new HttpClient();

    public override async Task<URLCheckResult> Check(PackageIndexSingleData data)
    {
        try
        {
            var sel = "/html/body/div[1]/div[1]/div[2]/p[2]";
            var html = await hc.GetStringAsync("https://linux.wps.cn/");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var version = doc.DocumentNode.SelectSingleNode(sel).InnerText!;
            return data.Url.Segments[0] == version
                ? new URLCheckResult(CheckStatus.AlreadyNewest, $"wps://{version}", data)
                : new URLCheckResult(CheckStatus.UpdateRequired, $"wps://{version}", data);
        }
        catch (Exception e)
        {
            return new URLCheckResult(CheckStatus.Failed, "", data);
        }
    }
}