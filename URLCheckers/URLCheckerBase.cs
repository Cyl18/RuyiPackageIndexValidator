using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator.URLCheckers
{
    public abstract class URLCheckerBase
    {
        protected static HttpClient hc = new HttpClient();
        public abstract Task<URLCheckResult> Check(PackageIndexSingleData data);

        public static void CheckAll(PackageIndexSingleData[] datas)
        {
            var tasks = new List<(PackageIndexSingleData data, Task<URLCheckResult> task)>();
            foreach (var data in datas)
            {
                var url = data.Url.URL;
                if (url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/dist/") || url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/3rdparty/milkv/repacks/")
                    || url.StartsWith("https://mirror.iscas.ac.cn/openeuler-sig-riscv/openEuler-RISC-V/preview/openEuler-23.09-V1-riscv64/"))
                {
                    tasks.Add((data, new RuyiDistMirrorChecker().Check(data)));
                } 
                else if (url.StartsWith(""))
                {
                    
                }
            }
        }
    }

    
}

public record URLCheckResult(CheckStatus CheckStatus, string? NewestVersionFileName);

public enum CheckStatus
{
    AlreadyNewest,
    Failed,
    UpdateRequired,
    CannotFindRelease,
    // UnableToCheck,
    NotImplemented
}
