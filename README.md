## NetEscapades.GitVersioning.GitHub

A stripped down version of [Nerdbank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning) that uses the [GitHub API](https://developer.github.com/v3/) instead of a local Git repository.

Implemented as .NET Core Global Tool `gitversioning-github`.

```bash
Usage: gitversioning-github [options] <owner> <repo> <commit>

Arguments:
  owner             The owner of the repository. Required.
  repo              The name of the repository. Required.
  commit            The SHA of the current commit for the working directory. Required.

Options:
  -?|-h|--help      Show help information
  -p|--project      The path to the project directory. The default is the current directory.
  -l|--login        The GitHub login for the user. Required.
  -a|--accesstoken  The GitHub password or access token for the user. Required.

Uses the GitHub API to calculate a build number for a commit, using similar rules to 
NerdBank.GitVersioning. Currently has a limited API - only a subset of features are supported.

```