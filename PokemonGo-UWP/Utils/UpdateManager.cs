using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Octokit;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    /// Manager that checks if there's an updated version on GitHub
    /// </summary>
    public static class UpdateManager
    {
        /// <summary>
        /// Client to access GitHub
        /// </summary>
        private static readonly GitHubClient GitHubClient = new GitHubClient(new ProductHeaderValue("PoGo-UWP"));

        /// <summary>
        /// Checks if we have an updated version and returns the URI
        /// </summary>
        /// <returns></returns>
        public static async Task<string> IsUpdateAvailable()
        {
            try
            {
                var releases = await GitHubClient.Repository.Release.GetAll("ST-Apps", "PoGo-UWP");
                var latestRelease = releases[0];
                // We skip prereleases, only stable ones
                if (latestRelease.Prerelease) return null;
                // Check if version number matches
                var currentVersion = Package.Current.Id.Version;
                var version = $"{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}";
                var tagVersion = latestRelease.TagName.Replace("v", "");
                return !version.Equals(tagVersion) ? latestRelease.HtmlUrl : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
