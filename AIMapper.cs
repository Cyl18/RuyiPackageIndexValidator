using GammaLibrary.Extensions;
using OpenAI;
using OpenAI.Chat;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GammaLibrary;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RuyiPackageIndexValidator
{
    internal class AIMapper
    {
        public static async Task<SupportMatrixImageResultData[]> Run()
        {
            var folderNames = GetSupportMatrixFolderNames();
            var supportMatrixImages = GetSupportMatrixImages();
            var sha = (folderNames.Connect("") +
                      supportMatrixImages.Select(x => x.DisplayName + x.Packages.Connect("")).Connect()).SHA256().ToHexString();
            if (SupportMatrixImageCache.Instance.Sha == sha)
            {
                return SupportMatrixImageCache.Instance.Datas;
            }

            var openAiClient = new OpenAIClient(clientSettings: new OpenAIClientSettings("https://ark.cn-beijing.volces.com/api", apiVersion: "v3"),
                openAIAuthentication: Environment.GetEnvironmentVariable("HUOSHAN_API_KEY"), client: new HttpClient() { Timeout = TimeSpan.FromMinutes(1000) });
            var progressBar = new ProgressBar(supportMatrixImages.Length - 1, "Checking Release Updates...",
                new ProgressBarOptions() { ForegroundColor = ConsoleColor.Cyan });
            var resultList = new ConcurrentBag<SupportMatrixImageResultData>();
            await Parallel.ForEachAsync(supportMatrixImages, async (data, token) =>
            {
                var response = await openAiClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(new[]
                {
                    new Message(Role.User, "文件夹名：\n" + folderNames.Connect("\n") 
                                                     + $"\n根据以上文件夹名，你认为 {data.DisplayName}/[{data.Packages.Connect()}] 应该分类于哪个文件夹，如果找不到请输出null，请不要输出多余内容，请注意保留文件夹的大小写")
                }, "deepseek-v3-241226"));
                var result = response.FirstChoice.Message.ToString();
                resultList.Add(new (data, result.Contains("null")?null:result));
                if (!result.Contains("null") && !folderNames.Contains(result))
                {
                    throw new Exception($"找不到文件夹：{result}");
                }
                progressBar.Tick(data.DisplayName);
            });
            progressBar.Dispose();

            SupportMatrixImageCache.Instance.Sha = sha;
            var resultDatas = resultList.ToArray();
            SupportMatrixImageCache.Instance.Datas = resultDatas;
            SupportMatrixImageCache.Save();
            return resultDatas;
        }

        public static string[] GetSupportMatrixFolderNames()
        {
            var dirs = Directory.GetDirectories(SupportMatrixRootPath, "*", SearchOption.AllDirectories)
                    .Select(x => Path.GetRelativePath(SupportMatrixRootPath, x))
                    .Where(x => 
                    !x.StartsWith(".git") 
                    && !x.StartsWith("assets")).Where(x=> x.Count(y => y == Path.DirectorySeparatorChar) == 1)
                ;

            return dirs.ToArray();
        }

        public static SupportMatrixImageSingleData[] GetSupportMatrixImages()
        {
            var file = File.ReadAllText(Path.Combine(RootPath, "..", "..", "provisioner", "config.yml"));
            var obj = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build()
                .Deserialize<RuyiProvisionerConfig>(file);
            
            return obj.ImageCombos.Where(x => !x.DisplayName.Contains("(documentation-only)")).Select(x=>new SupportMatrixImageSingleData(x.DisplayName, x.Packages)).ToArray();
        }
    }

    public record SupportMatrixImageSingleData(string DisplayName, List<string> Packages);
    public record SupportMatrixImageResultData(SupportMatrixImageSingleData Data, string? DirName);

    [ConfigurationPath("support-matrix-image-cache.json")]
    public class SupportMatrixImageCache : Configuration<SupportMatrixImageCache>
    {
        public string Sha { get; set; } = null;
        public SupportMatrixImageResultData[] Datas { get; set; } = null;
    }


    public class RuyiProvisionerConfig
    {
        public string ruyi_provisioner_config { get; set; }
        public Dictionary<string, string> PostinstMessages { get; set; }
        public List<ImageCombo> ImageCombos { get; set; }
        public List<Device> Devices { get; set; }
    }

    public class ImageCombo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<string> Packages { get; set; }
        public string PostinstMsgid { get; set; }
    }

    public class Device
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<DeviceVariant> Variants { get; set; }
    }

    public class DeviceVariant
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<string> SupportedCombos { get; set; }
    }
}
