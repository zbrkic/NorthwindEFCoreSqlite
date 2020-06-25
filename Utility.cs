using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyModel;

namespace NorthwindEFCoreSqlite
{
    public static class Utility
    {
        // SQLitePCLRaw is attempting to load the "libe_sqlite3.so" library using System.Runtime.InteropServices.NativeLibrary.
        // This doesn't account for the runtimes/$rid/native/*.dll layout.
        // This method will copy libe_sqlite3.so to the same path where SQLitePCLRaw is.
        public static void CopyNativeLib()
        {
            const string libSQLitePCLRaw = "SQLitePCLRaw.core.dll";
            const string nativeLib = "libe_sqlite3.so";

            var nugetPath = GetNugetPath();
            var fallbacks = GetFallbacks();
            var nativeLibFiles = Directory.GetFiles(nugetPath, nativeLib, SearchOption.AllDirectories);
            var nativeLibFile = nativeLibFiles.First(l => fallbacks.Any(f => l.EndsWith($"/runtimes/{f}/native/{nativeLib}")));
            var libSQLitePCLRawPath = Directory.GetFiles(nugetPath, libSQLitePCLRaw, SearchOption.AllDirectories).Single();
            var destFolder = Path.GetDirectoryName(libSQLitePCLRawPath);
            var destFile = Path.Combine(destFolder, nativeLib);

            File.Copy(nativeLibFile, destFile, true);
        }

        private static string GetNugetPath()
        {
            var userProfileVar = Environment.GetEnvironmentVariable("HOME");
            var nugetPath = Path.Combine(userProfileVar, ".nuget", "packages");

            return nugetPath;
        }

        private static IReadOnlyList<string> GetFallbacks()
        {
            var rti = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            var runtimeFallbacks = DependencyContext.Default.RuntimeGraph.Single(x => x.Runtime == rti);

            return runtimeFallbacks.Fallbacks;
        }
    }
}
