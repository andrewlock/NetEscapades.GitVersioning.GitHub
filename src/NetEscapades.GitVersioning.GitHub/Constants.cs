using System;
using System.Collections.Generic;

namespace NetEscapades.GitVersioning.GitHub
{
    public static class Constants
    {
        public const string AppName = "NetEscapades.GitVersioning.GitHub";

        public const string ExtendedHelpText = @"
Uses the GitHub API to calculate a build number for a commit,
using similar rules to NerdBank.GitVersioning. 
Currently has a limited API - only a subset of features are 
supported.
";
    }
}