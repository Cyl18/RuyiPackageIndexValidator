using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuyiPackageIndexValidator;

namespace RuyiPackageIndexValidator.URLCheckers
{
    public abstract class URLCheckerBase
    {
        protected static HttpClient hc = new HttpClient();
        public abstract Task<URLCheckResult> Check(PackageIndexSingleData data);

        public static async Task<List<URLCheckResult>> CheckAll(PackageIndexSingleData[] datas)
        {
            var tasks = new List<(PackageIndexSingleData data, Task<URLCheckResult> task)>();
            var result = new List<URLCheckResult>();
            foreach (var data in datas)
            {
                var url = data.Url.URL;
                if (url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/dist/") || url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/3rdparty/milkv/repacks/"))
                {
                    result.Add(await new RuyiDistMirrorChecker().Check(data));
                } 
                else if (url.StartsWith("https://mirror.iscas.ac.cn/openeuler-sig-riscv/openEuler-RISC-V/"))
                {
                    result.Add(new URLCheckResult(CheckStatus.NotImplemented, null, data));
                } 
                else if (url.StartsWith("https://mirror.iscas.ac.cn/"))
                {
                    result.Add(await new RuyiMirrorGenericChecker().Check(data));

                }
                else if (url.StartsWith("https://github.com"))
                {
                    result.Add(await new GitHubReleaseChecker().Check(data));
                }
                else
                {
                    result.Add(new URLCheckResult(CheckStatus.InDev, null, data));
                }
            }
            return result;
        }
    }

    
}

public record URLCheckResult(CheckStatus CheckStatus, string? NewestVersionFileName, PackageIndexSingleData PackageIndexSingleData);

public enum CheckStatus
{
    AlreadyNewest,
    Failed,
    UpdateRequired,
    CannotFindRelease,
    NotImplemented,
    InDev
    // UnableToCheck,
}
