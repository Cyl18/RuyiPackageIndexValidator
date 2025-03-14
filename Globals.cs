using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuyiPackageIndexValidator
{
    internal class Globals
    {
        public static string PackagesIndexPath { get; } = @"C:\Users\cyl18\Documents\GitHub\packages-index";
        public static string BoardImagePath { get; } = $@"{PackagesIndexPath}\manifests\board-image";
        public static string SupportMatrixRootPath { get; } = @"C:\Users\cyl18\Documents\GitHub\support-matrix";
        //public static string BoardImagePath { get; } = @"packages-index\manifests\board-image";
        public static string Token { get; } = Environment.GetEnvironmentVariable("GITHUB_TOKEN")!;
    }
}