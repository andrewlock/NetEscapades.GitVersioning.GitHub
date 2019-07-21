using System;
using System.IO;
using System.Runtime.Serialization;

namespace NetEscapades.GitVersioning.GitHub.Helpers
{
    public static class PathHelpers
    {
        public static string NormalizeDirectoryPath(string path)
        {
            return
                (string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
        }

        public static string GetRelativePath(string absolutePath, string rootPath)
        {
            var rootUri = new Uri(rootPath, UriKind.Absolute);
            var absoluteUri = new Uri(absolutePath, UriKind.Absolute);
                
            return rootUri.MakeRelativeUri(absoluteUri).ToString();
        }
    }
}