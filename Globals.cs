using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator
{
    internal class Globals
    {
        public static string RootPath { get; } = @"C:\Users\cyl18\Documents\GitHub\packages-index\manifests";
        //public static string RootPath { get; } = @"packages-index\manifests";
        public static string Token { get; } = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;
    }
}