using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace NetEscapades.GitVersioning.GitHub
{
    [Command(
        Name = "gitversioning-github",
        Description = "Generates a NerdBank.GitVersioning compatible number using the GitHub API",
        ExtendedHelpText = Constants.ExtendedHelpText)]
    [HelpOption]
    public class VersionOracle
    {
        [Required(ErrorMessage = "The repository owner is required", AllowEmptyStrings = false)]
        [Argument(0, "owner", "The owner of the repository. Required.")]
        public string Owner { get; }

        [Required(ErrorMessage = "The repository name is required", AllowEmptyStrings = false)]
        [Argument(1, "repo", "The name of the repository. Required.")]
        public string RepositoryName { get; }

        [Required(ErrorMessage = "The commit SHA is required", AllowEmptyStrings = false)]
        [Argument(2, "commit", "The SHA of the current commit for the working directory. Required.")]
        public string CommitSha { get; }

        [Option("-p|--project", CommandOptionType.SingleValue,
            Description = "The path to the project or project directory. The default is the current directory.")]
        [FileOrDirectoryExists]
        public string ProjectPath { get; } = ".";
        
        [Required(ErrorMessage = "The GitHub login is required", AllowEmptyStrings = false)]
        [Option("-l|--login", CommandOptionType.SingleValue, Description = "The GitHub login for the user. Required.")]
        public string GitHubLogin { get; }

        [Required(ErrorMessage = "The GitHub password is required", AllowEmptyStrings = false)]
        [Option("-a|--accesstoken", CommandOptionType.SingleValue, Description = "The GitHub password or access token for the user. Required.")]
        public string GitHubPassword { get; }


        public async Task<int> OnExecute(CommandLineApplication app, IConsole console)
        {
            var newVersion = await GetVersion();

            console.WriteLine($"Version: {newVersion}");

            return Program.OK;
        }

        async Task<string> GetVersion()
        {
            var github = new GitHubClient(new Octokit.ProductHeaderValue(Constants.AppName))
            {
                Credentials = new Credentials(GitHubLogin, GitHubPassword),
            };

            var workingVersion = VersionFile.GetVersion(ProjectPath, out var actualDirectory);
            var absoluteJsonFilePath = Path.Combine(actualDirectory, VersionFile.JsonFileName);
            var relativeJsonFilePath = new Uri(ProjectPath, UriKind.Absolute)
                .MakeRelativeUri(new Uri(absoluteJsonFilePath, UriKind.Absolute))
                .ToString();

            var commitsForVersionFile = await github.Repository.Commit.GetAll(Owner, RepositoryName, new CommitRequest
            {
                Sha = CommitSha,
                Path = relativeJsonFilePath
            });

            var (rootCommitSha, isNewVersionFile) =
                await GetRootCommitForHeightCalculation(commitsForVersionFile, github, Owner, RepositoryName, relativeJsonFilePath, workingVersion);


            if (isNewVersionFile)
            {
                // we're done, we know what the version must be
                return GetVersion(workingVersion.Version.Version);
            }

            // we need to calculate the git height
            var compare = await github.Repository.Commit.Compare(Owner, RepositoryName, rootCommitSha, CommitSha);
            var gitHeight = compare.AheadBy + 1;
            return GetVersion(workingVersion.Version.Version, gitHeight);
        }

        static string GetVersion(Version version, int gitHeight = 1)
        {
            // we're done, we know what the version must be
            return $"{version.Major}.{version.Minor}.{gitHeight}";
        }


        static async Task<(string rootCommitSha, bool isNewVersionFile)> GetRootCommitForHeightCalculation(
            IReadOnlyList<GitHubCommit> commitsForVersionFile,
            GitHubClient github,
            string owner,
            string repo,
            string relativeJsonFilePath,
            VersionOptions workingVersion)
        {
            string previousCommit = null;
            // keep looping through the commits until we find one that is different from workingVersion
            foreach (var commit in commitsForVersionFile)
            {
                var fileContentsArray = await github.Repository.Content.GetAllContentsByRef(
                    owner, repo, relativeJsonFilePath, commit.Sha);

                // should only have 1 file here, and should be text
                var thisCommitVersion = VersionFile.GetVersionFromContent(fileContentsArray[0].Content);

                if (thisCommitVersion.Version.Version.Major != workingVersion.Version.Version.Major
                    || thisCommitVersion.Version.Version.Minor != workingVersion.Version.Version.Minor)
                {
                    // this commit had a different version of the file - if it's the first commit, it's a new file
                    var isNewFile = previousCommit == null;
                    return (previousCommit, isNewFile);
                }

                previousCommit = commit.Sha;
            }

            // we didn't get a match, so must either be same version as original, or we have no commits yet! 
            return (rootCommitSha: previousCommit, isNewVersionFile: previousCommit == null);
        }
    }
}