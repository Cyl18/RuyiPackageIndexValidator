﻿using RuyiPackageIndexValidator;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator.URLCheckers
{
    public abstract class URLCheckerBase
    {
        protected static HttpClient hc = new HttpClient(new HttpClientHandler() {AllowAutoRedirect = true});
        public abstract Task<URLCheckResult> Check(PackageIndexSingleData data);
        private static ProgressBar progressBar;

        public static async Task<List<URLCheckResult>> CheckAll(PackageIndexSingleData[] datas,
            ValidateResult[] results)
        {
            progressBar = new ProgressBar(datas.Length - 1, "Checking Release Updates...",
                new ProgressBarOptions() { ForegroundColor = ConsoleColor.Cyan });
            var result = new ConcurrentBag<URLCheckResult>();
            await Parallel.ForEachAsync(datas, async (data, token) =>
            {
                var url = data.Url.URL;
                if (!results.First(x => x.PackageIndexSingleData == data).IsSuccessStatusCode)
                {
                    if (results.First(x => x.PackageIndexSingleData == data).HttpCode == "403")
                    {
                        result.Add(new URLCheckResult(CheckStatus.CannotFindRelease403, null, data));
                    }
                    else if (results.First(x => x.PackageIndexSingleData == data).HttpCode == "404")
                    {
                        result.Add(new URLCheckResult(CheckStatus.CannotFindRelease404, null, data));
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/dist/") || url.StartsWith("https://mirror.iscas.ac.cn/ruyisdk/3rdparty/milkv/repacks/"))
                {
                    result.Add(await new RuyiDistMirrorChecker().Check(data));
                }
                else if (url.StartsWith("https://mirror.iscas.ac.cn/openeuler-sig-riscv/openEuler-RISC-V/")
                         || url.StartsWith("https://releases.openkylin.top/1.0/"))
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
                else if (url.StartsWith("https://mirrors.tuna.tsinghua.edu.cn/openwrt/releases/"))
                {
                    result.Add(await new OpenWrtChecker().Check(data));
                }
                else if (url.StartsWith("wps://"))
                {
                    result.Add(await new WPSChecker().Check(data));
                }
                else
                {
                    result.Add(new URLCheckResult(CheckStatus.InDev, null, data));
                }
                progressBar.Tick(url);
            });
            progressBar.Dispose();
            return result.ToList();
        }
    }

    
}

public record URLCheckResult(CheckStatus CheckStatus, string? NewestVersionFileName, PackageIndexSingleData PackageIndexSingleData);

public enum CheckStatus
{
    UpdateRequired,
    Failed,
    CannotFindRelease404,
    CannotFindRelease403,
    InDev,
    NotImplemented,
    AlreadyNewest,
    // UnableToCheck,
}
