using System.Text.RegularExpressions;
using GammaLibrary.Extensions;
using Semver;

namespace RuyiPackageIndexValidator;

public class SoftwareVersion
{
    public static SoftwareVersion Parse(string version)
    {
        throw new Exception();
    }
}

public class ManifestVersion : IComparable<ManifestVersion>, IComparable
{

    public ManifestVersion(SemVersion version)
    {
        this.version = version;
    }

    public int CompareTo(ManifestVersion? other)
    {
        if (other == null)
        {
            return 1; // 当前对象大于 null
        }

        // 比较 Major 版本
        if (this.version.Major != other.version.Major)
        {
            return this.version.Major.CompareTo(other.version.Major);
        }

        // 比较 Minor 版本
        if (this.version.Minor != other.version.Minor)
        {
            return this.version.Minor.CompareTo(other.version.Minor);
        }

        // 比较 Patch 版本
        if (this.version.Patch != other.version.Patch)
        {
            return this.version.Patch.CompareTo(other.version.Patch);
        }

        if (this.version.Prerelease != other.version.Prerelease)
        {
            var regex = new Regex("\\d+");
            var thisVersion = regex.Match(this.version.Prerelease);
            var otherVersion = regex.Match(other.version.Prerelease);
            return thisVersion.Groups[0].Value.ToInt().CompareTo(otherVersion.Groups[0].Value.ToInt());
        }

        // 如果 Major、Minor 和 Patch 都相等，则两个版本相等
        throw new Exception("版本号相同");
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is ManifestVersion other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ManifestVersion)}");
    }

    public static bool operator <(ManifestVersion? left, ManifestVersion? right)
    {
        return Comparer<ManifestVersion>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(ManifestVersion? left, ManifestVersion? right)
    {
        return Comparer<ManifestVersion>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(ManifestVersion? left, ManifestVersion? right)
    {
        return Comparer<ManifestVersion>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(ManifestVersion? left, ManifestVersion? right)
    {
        return Comparer<ManifestVersion>.Default.Compare(left, right) >= 0;
    }

    private SemVersion version;
}