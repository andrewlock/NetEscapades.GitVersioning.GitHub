// adapted from https://github.com/AArnott/Nerdbank.GitVersioning/blob/9880338a605b64e8750208883053b4362b7216be/src/NerdBank.GitVersioning/VersionFile.cs
// using System;

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Validation;

namespace NetEscapades.GitVersioning.GitHub
{
    /// <summary>
    /// Extension methods for interacting with the version.txt file.
    /// </summary>
    public static class VersionFile
    {
        /// <summary>
        /// The filename of the version.txt file.
        /// </summary>
        public const string TxtFileName = "version.txt";

        /// <summary>
        /// The filename of the version.json file.
        /// </summary>
        public const string JsonFileName = "version.json";

        /// <summary>
        /// A sequence of possible filenames for the version file in preferred order.
        /// </summary>
        public static readonly IReadOnlyList<string> PreferredFileNames = new[] { JsonFileName, TxtFileName };

        /// <summary>
        /// Reads the version.txt file and returns the <see cref="Version"/> and prerelease tag from it.
        /// </summary>
        /// <param name="projectDirectory">The path to the directory which may (or its ancestors may) define the version.txt file.</param>
        /// <returns>The version information read from the file, or <c>null</c> if the file wasn't found.</returns>
        public static VersionOptions GetVersion(string projectDirectory) => GetVersion(projectDirectory, out string _);

        public static VersionOptions GetVersionFromContent(string versionJsonContent)
        {
            return TryReadVersionJsonContent(versionJsonContent);
        }
        
        /// <summary>
        /// Reads the version.txt file and returns the <see cref="Version"/> and prerelease tag from it.
        /// </summary>
        /// <param name="projectDirectory">The path to the directory which may (or its ancestors may) define the version.txt file.</param>
        /// <param name="actualDirectory">Set to the actual directory that the version file was found in, which may be <paramref name="projectDirectory"/> or one of its ancestors.</param>
        /// <returns>The version information read from the file, or <c>null</c> if the file wasn't found.</returns>
        public static VersionOptions GetVersion(string projectDirectory, out string actualDirectory)
        {
            Requires.NotNullOrEmpty(projectDirectory, nameof(projectDirectory));

            string searchDirectory = projectDirectory;
            while (searchDirectory != null)
            {
                string parentDirectory = Path.GetDirectoryName(searchDirectory);
                string versionTxtPath = Path.Combine(searchDirectory, TxtFileName);
                if (File.Exists(versionTxtPath))
                {
                    using (var sr = new StreamReader(File.OpenRead(versionTxtPath)))
                    {
                        var result = TryReadVersionFile(sr, isJsonFile: false);
                        if (result != null)
                        {
                            actualDirectory = searchDirectory;
                            return result;
                        }
                    }
                }

                string versionJsonPath = Path.Combine(searchDirectory, JsonFileName);
                if (File.Exists(versionJsonPath))
                {
                    string versionJsonContent = File.ReadAllText(versionJsonPath);
                    VersionOptions result = TryReadVersionJsonContent(versionJsonContent);
                    if (result?.Inherit ?? false)
                    {
                        if (parentDirectory != null)
                        {
                            result = GetVersion(parentDirectory);
                            if (result != null)
                            {
                                JsonConvert.PopulateObject(versionJsonContent, result, VersionOptions.GetJsonSettings());
                                actualDirectory = searchDirectory;
                                return result;
                            }
                        }

                        throw new InvalidOperationException($"\"{versionJsonPath}\" inherits from a parent directory version.json file but none exists.");
                    }
                    else if (result != null)
                    {
                        actualDirectory = searchDirectory;
                        return result;
                    }
                }

                searchDirectory = parentDirectory;
            }

            actualDirectory = null;
            return null;
        }

        /// <summary>
        /// Checks whether the version.txt file is defined in the specified project directory
        /// or one of its ancestors.
        /// </summary>
        /// <param name="projectDirectory">The directory to start searching within.</param>
        /// <returns><c>true</c> if the version.txt file is found; otherwise <c>false</c>.</returns>
        public static bool IsVersionDefined(string projectDirectory)
        {
            Requires.NotNullOrEmpty(projectDirectory, nameof(projectDirectory));

            return GetVersion(projectDirectory) != null;
        }

        /// <summary>
        /// Reads the version.txt file and returns the <see cref="Version"/> and prerelease tag from it.
        /// </summary>
        /// <param name="versionTextContent">The content of the version.txt file to read.</param>
        /// <param name="isJsonFile"><c>true</c> if the file being read is a JSON file; <c>false</c> for the old-style text format.</param>
        /// <returns>The version information read from the file; or <c>null</c> if a deserialization error occurs.</returns>
        private static VersionOptions TryReadVersionFile(TextReader versionTextContent, bool isJsonFile)
        {
            if (isJsonFile)
            {
                string jsonContent = versionTextContent.ReadToEnd();
                return TryReadVersionJsonContent(jsonContent);
            }

            string versionLine = versionTextContent.ReadLine();
            string prereleaseVersion = versionTextContent.ReadLine();
            if (!string.IsNullOrEmpty(prereleaseVersion))
            {
                if (!prereleaseVersion.StartsWith("-"))
                {
                    // SemVer requires that prerelease suffixes begin with a hyphen, so add one if it's missing.
                    prereleaseVersion = "-" + prereleaseVersion;
                }
            }

            SemanticVersion semVer;
            Verify.Operation(SemanticVersion.TryParse(versionLine + prereleaseVersion, out semVer), "Unrecognized version format.");
            return new VersionOptions
            {
                Version = semVer,
            };
        }

        /// <summary>
        /// Tries to read a version.json file from the specified string, but favors returning null instead of throwing a <see cref="JsonSerializationException"/>.
        /// </summary>
        /// <param name="jsonContent">The content of the version.json file.</param>
        /// <returns>The deserialized <see cref="VersionOptions"/> object, if deserialization was successful.</returns>
        private static VersionOptions TryReadVersionJsonContent(string jsonContent)
        {
            try
            {
                return JsonConvert.DeserializeObject<VersionOptions>(jsonContent, VersionOptions.GetJsonSettings());
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }
    }
}