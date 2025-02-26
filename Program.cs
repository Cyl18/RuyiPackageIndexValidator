﻿// See https://aka.ms/new-console-template for more information

using GammaLibrary.Extensions;
using RuyiPackageIndexValidator;
using RuyiPackageIndexValidator.URLCheckers;
using Semver;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

var versions = ManifestFilter.Run();



var packageIndexSingleDatas = versions.Select(x => PackageIndexTomlParser.ParseSingle(x.FilePath)).SelectMany(x => x).ToArray();
// foreach (var (path, packageUrl) in packageIndexSingleDatas)
// {
//     Console.WriteLine(packageUrl.URL);
// }
var sb = new StringBuilder();
var results = await WebLinkValidator.Validate(packageIndexSingleDatas);
var checkAll = await URLCheckerBase.CheckAll(packageIndexSingleDatas, results);

foreach (var (checkStatus, newestVersionFileName, packageIndexSingleData) in checkAll)
{

}

// await RuyiDistMirrorChecker.GetAllFiles();
// var validateResult = await WebLinkValidator.Validate(packageIndexSingleDatas);
// WebLinkValidator.Print(validateResult);

Debugger.Break();