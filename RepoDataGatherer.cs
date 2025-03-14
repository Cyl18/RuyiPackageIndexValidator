using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace RuyiPackageIndexValidator
{
    internal class RepoDataGatherer
    {
        public static string Run(string path)
        {
            var repo = new Repository(path);
            var commit = repo.Head.Commits.First();

            return $"{commit.Id.Sha[..6]}: {commit.Author.When.ToString("s")}，{(DateTimeOffset.Now - commit.Author.When).TotalDays:F1} 天前";
        }
    }
}
