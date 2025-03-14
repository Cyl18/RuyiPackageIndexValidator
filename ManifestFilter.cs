using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semver;

namespace RuyiPackageIndexValidator
{
    internal class ManifestFilter
    {
        public static List<(string ManifestPath, string FilePath)> Run()
        {
            var versionsDictionary = new Dictionary<string, List<string>>();
            foreach (var file in Directory.GetFiles(BoardImagePath, "*.toml", SearchOption.AllDirectories))
            {
                var path = Path.GetRelativePath(BoardImagePath, file);
                var dir = Path.GetDirectoryName(path);
                if (versionsDictionary.TryGetValue(dir, out var list))
                {
                    list.Add(file);
                }
                else
                {
                    versionsDictionary[dir] = [file];
                }

            }


            var versions = versionsDictionary.Select(x =>
                {
                    var orderByDescending = x.Value
                        .OrderByDescending(v =>
                            new ManifestVersion(SemVersion.Parse(Path.GetFileNameWithoutExtension(v)))).ToArray();
                    return (x.Key, orderByDescending
                        .First()
                        .ToString());
                })
                    .ToList();
            return versions;

        }
    }
}
