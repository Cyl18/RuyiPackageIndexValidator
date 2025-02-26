using GammaLibrary.Enhancements;
using RuyiPackageIndexValidator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;

namespace RuyiPackageIndexValidator
{
    internal class WebLinkValidator
    {
        private static HttpClient hc = new();

        public static async Task<ValidateResult[]> Validate(PackageIndexSingleData[] datas)
        {
            var tasks = datas.Select(async data => (data, task: await Exists(data.Url))).ToList();

            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result)
                .Select(x => new ValidateResult(x.data, x.task.IsSuccessStatusCode, x.task.HttpCode)).ToArray();
        }

        public static void Print(ValidateResult[] datas)
        {
            var sb = new StringBuilder();
            foreach (var result in datas)
            {
                if (!result.IsSuccessStatusCode)
                {
                    sb.AppendLine($"| × | {result.HttpCode} | {result.PackageIndexSingleData.Path} | {result.PackageIndexSingleData.Url.URL} |");
                }
            }

            foreach (var result in datas)
            {
                if (result.IsSuccessStatusCode)
                {
                    sb.AppendLine($"| √ | {result.HttpCode} | {result.PackageIndexSingleData.Path} | {result.PackageIndexSingleData.Url.URL} |");
                }
            }

            sb.ToString().Print();
        }
        static async Task<(bool IsSuccessStatusCode, string HttpCode)> Exists(PackageUrl url)
        {
            try
            {
                var msg = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Head, url.URL));

                return (msg.IsSuccessStatusCode, ((int)msg.StatusCode).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return (false, "0");
            }
        }
    }
}

public record ValidateResult(PackageIndexSingleData PackageIndexSingleData, bool IsSuccessStatusCode, string HttpCode);