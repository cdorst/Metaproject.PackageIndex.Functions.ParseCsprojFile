using DevOps.Primitives.NuGet;
using Metaproject.PackageIndex.Structures.PackageProject;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.String;

namespace Metaproject.PackageIndex.Functions.ParseCsprojFile
{
    public static class CsprojFileParser
    {
        public static PackageCsproj ParseCsproj(string path)
        {
            var lines = File.ReadAllLines(path)
                .Where(line => !IsNullOrWhiteSpace(line));
            return new PackageCsproj(
                GetName(path),
                GetTagValue("Description", lines),
                GetTagValue("Version", lines),
                GetReferences(lines).ToList());
        }

        private static string GetName(string path)
        {
            var name = new FileInfo(path).Name;
            return name.Substring(0, name.Length - ".csproj".Length);
        }

        private static IEnumerable<NuGetReference> GetReferences(IEnumerable<string> lines)
        {
            foreach (var line in lines.Where(it
                => it.Contains("<PackageReference")
                || it.Contains("<DotNetCliToolsReference")))
            {
                var split = line.Split('"');
                yield return new NuGetReference(
                    include: split.ElementAt(2),
                    version: split.ElementAt(4),
                    line.Contains("<PackageReference")
                        ? ReferenceType.PackageReference
                        : ReferenceType.DotNetCliToolReference);
            }
        }

        private static string GetTagValue(string tag, string line)
            => IsNullOrWhiteSpace(line) ? null
            :  new string(
                line.Split(tag.ToArray())
                .ElementAt(2)
                .Substring(1)
                .Reverse()
                .Skip(2)
                .Reverse()
                .ToArray());

        private static string GetTagValue(string tag, IEnumerable<string> lines)
            => GetTagValue(tag, lines
                .Where(line => line.Contains(tag))
                .FirstOrDefault());
    }
}
