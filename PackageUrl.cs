using GammaLibrary.Enhancements;
using GammaLibrary.Extensions;

namespace RuyiPackageIndexValidator;
public class PackageUrl
{
    public List<string> Segments { get; }
    public string Protocol { get; }
    public string URL { get; set; }

    private PackageUrl(string url)
    {
        URL = url;
        if (url.StartsWith("https://"))
        {
            Protocol = "https://";
            Segments = url["https://".Length..].Split('/').ToList();
            return;
        }

        if (url.StartsWith("wps://"))
        {
            Protocol = "wps://";
            Segments = [url["wps://".Length..]];
            return;
        }
        throw new Exception("Parse Error");
    }

    public static PackageUrl FromString(string url)
    {
        if (url.StartsWith("https://"))
        {
            return new PackageUrl(url);
        }
        else if (url.StartsWith("mirror://"))
        {
            url = url["mirror://".Length..];
            if (url.StartsWith("ruyi-3rdparty-canaan/"))
            {
                return new PackageUrl(url.Replace("ruyi-3rdparty-canaan/",
                    "https://mirror.iscas.ac.cn/ruyisdk/3rdparty/canaan/"));
            }
            else if (url.StartsWith("ruyi-3rdparty-milkv/"))
            {
                return new PackageUrl(url.Replace("ruyi-3rdparty-milkv/",
                    "https://mirror.iscas.ac.cn/ruyisdk/3rdparty/milkv/"));
            }
            else
            {
                throw new Exception("Parse Error");
            }
        }

        if (!url.Contains("/"))
        {
            url = "https://mirror.iscas.ac.cn/ruyisdk/dist/" + url;
        }
        return new PackageUrl(url);
    }
}
