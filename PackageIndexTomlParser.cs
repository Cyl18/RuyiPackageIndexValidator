using System.Diagnostics;
using GammaLibrary.Extensions;
using ShellProgressBar;
using Tommy;

namespace RuyiPackageIndexValidator;

public class PackageIndexTomlParser
{
    public static PackageIndexSingleData[] ParseSingle(string path)
    {
        var data = File.ReadAllText(path);
        var toml = TOML.Parse(new StringReader(data));
        var urlsResult = new List<string>();
        
        toml.TryGetNode("distfiles", out var node);
        foreach (TomlNode o in node)
        {
            if (o.TryGetNode("fetch_restriction", out var fetchRestriction))
            {
                var ver = fetchRestriction["params"]["version"].AsString.Value;
                var restrictedSoftwareName = toml["metadata"]["desc"].AsString.Value;
                Trace.Assert(restrictedSoftwareName == "Kingsoft WPS Office", $"new restricted software has been added, not WPS, name: {restrictedSoftwareName}");
                urlsResult.Add($"wps://{ver}");
                continue;
            }
            
            var name = o["name"];
            var hasUrls = o.TryGetNode("urls", out var urls);

            if (hasUrls)
            {
                var urls1 = new List<string>();
                foreach (TomlNode url in urls)
                {
                    urls1.Add(url.AsString.Value);
                }

                if (urls1.Count > 1)
                {
                    var originalLength = urls1.Count;
                    if (urls1.Any(x => x.Contains("openwrt")))
                    {
                        urls1.RemoveAll(x => x.Contains("openwrt.org"));
                    }
                    else
                    {
                        urls1.RemoveAll(x => x.Contains("mirror"));
                    }
                    Trace.Assert(originalLength - urls1.Count == 1);
                    urlsResult.Add(urls1.First());
                }
                else
                {
                    if (urls1.Any())
                    {
                        urlsResult.Add(urls1.First());
                    }
                }    
            }
            else
            {
                urlsResult.Add(name.AsString.Value);
            }
            
            
        }

        if (toml.TryGetNode("binary", out var binaries))
        {
            foreach (TomlNode binary in binaries)
            {
                foreach (TomlNode dist in binary["distfiles"])
                {
                    var distUrl = dist.AsString.Value;
                    urlsResult.Add(distUrl);
                }

            }
        }

        var result = new List<PackageIndexSingleData>();
        foreach (var packageUrl in urlsResult.Distinct().Select(x => PackageUrl.FromString(x)))
        {
            result.Add(new PackageIndexSingleData(path, packageUrl));
        }

        return result.ToArray();
    }
}


public record PackageIndexSingleData(string Path, PackageUrl Url);