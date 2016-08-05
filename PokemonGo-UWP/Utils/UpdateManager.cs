using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Octokit;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Web.Http;
using Windows.System;
using Universal_Authenticator_v2.Views;
using System.IO;
using Windows.Management.Deployment;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;

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
        /// Describes
        /// </summary>
        public class UpdateInfo
        {
            public UpdateInfo(string version, string url, Release release)
            {
                this.version = version;
                this.url = url;
                this.release = release;
            }
            public string version;
            public string url;
            public Release release;
        }

        /// <summary>
        /// Checks if we have an updated version and returns update info
        /// </summary>
        /// <returns>Update info</returns>
        public static async Task<UpdateInfo> IsUpdateAvailable()
        {
            try
            {
                var releases = await GitHubClient.Repository.Release.GetAll("ST-Apps", "PoGo-UWP");
                Release latestRelease = null;

                foreach (var release in releases)
                {
                    //Look only for releases for our architecture
                    var archAssets = FilterAssetsByArchitecture(release);

                    // We skip prereleases, only stable ones
                    if (!release.Prerelease && archAssets.Count > 0)
                    {
                        latestRelease = release;
                        break;
                    }
                }

                if (latestRelease == null)
                    return null;

                // Check if version is newer
                var currentVersion = Package.Current.Id.Version;
                var regex = new Regex(@"\D*(\d*)\.(\d*)\.(\d*).*");
                var match = regex.Match(latestRelease.TagName);

                if(match.Success && match.Groups.Count >= 4)
                {
                    PackageVersion repoVersion = new PackageVersion();
                    repoVersion.Major = ushort.Parse(match.Groups[1].Value);
                    repoVersion.Minor = ushort.Parse(match.Groups[2].Value);
                    repoVersion.Build = ushort.Parse(match.Groups[3].Value);

                    //compare major & minor & build (ignore revision)
                    if ((repoVersion.Major > currentVersion.Major)
                       || (repoVersion.Major == currentVersion.Major && repoVersion.Minor > currentVersion.Minor)
                       || (repoVersion.Major == currentVersion.Major && repoVersion.Minor == currentVersion.Minor && repoVersion.Build > currentVersion.Build)
                       )
                    {
                        return new UpdateInfo(repoVersion.Major+"."+repoVersion.Minor+"."+repoVersion.Build, latestRelease.HtmlUrl, latestRelease);
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static async Task InstallUpdate(Release release)
        {
            bool browserFallback = true;


            /* This part of code is experimental and currently does not work - probably because .apps are not signed
             
            ReleaseAsset mainAsset = null;

            //if we have only 1 asset lets try to download and install it (we cant install frameworks)
            if (release.Assets.Count > 0)
            {
                //TODO: we cant solve dependencies yet - for this we should at first download appx, extract AppxManifest, look into it and download them from somewhere
                //List<ReleaseAsset> dependencies = new List<ReleaseAsset>();

                var archAssets = FilterAssetsByArchitecture(release);
                if (archAssets.Count == 1)
                    mainAsset = archAssets[0];

                if (mainAsset != null)
                {

                    try
                    {
                        //this needs restricted capability "packageManagement" in appxmanifest
                        Windows.Management.Deployment.PackageManager packageManager = new Windows.Management.Deployment.PackageManager();

                        StorageFile destinationFile = null;

                        try
                        {
                            Uri uri = new Uri(mainAsset.BrowserDownloadUrl);

                            Busy.SetBusy(true, Resources.Translation.GetString("DownloadingUpdate"));


                            //Download dependencies
                            List<Uri> dependencies = new List<Uri>();
                            dependencies.Add(new Uri(@"https://github.com/ST-Apps/PoGo-UWP/blob/master/PokemonGo-UWP/AppPackages/PokemonGo-UWP_1.0.3.0_ARM_Debug_Test/Dependencies/" + Package.Current.Id.Architecture.ToString().ToLower() + @"/Microsoft.NET.CoreRuntime.1.0.appx?raw=true"));
                            dependencies.Add(new Uri(@"https://github.com/ST-Apps/PoGo-UWP/blob/master/PokemonGo-UWP/AppPackages/PokemonGo-UWP_1.0.3.0_ARM_Debug_Test/Dependencies/" + Package.Current.Id.Architecture.ToString().ToLower() + @"/Microsoft.VCLibs." + Package.Current.Id.Architecture.ToString().ToLower() + @".Debug.14.00.appx?raw=true"));

                            for (int i = 0; i < dependencies.Count; i++)
                            {
                                StorageFile file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                                            i + ".appx", CreationCollisionOption.GenerateUniqueName);
                                await DownloadFile(file, dependencies[i]);

                                dependencies[i] = new Uri(file.Path);
                            }


                            destinationFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                                        mainAsset.Name, CreationCollisionOption.GenerateUniqueName);
                            await DownloadFile(destinationFile, uri);
                            Busy.SetBusy(false);
                            Busy.SetBusy(true, Resources.Translation.GetString("InstallingUpdate"));

                            try
                            {
                                var result = await packageManager.UpdatePackageAsync(new Uri(destinationFile.Path),
                                            dependencies,
                                            DeploymentOptions.ForceApplicationShutdown);
                                browserFallback = false;
                            }
                            catch (COMException exc)
                            {
                                try
                                {
                                    var result = await packageManager.AddPackageAsync(new Uri(destinationFile.Path),
                                            dependencies,
                                            DeploymentOptions.ForceApplicationShutdown);
                                    browserFallback = false;
                                }
                                catch (COMException exc2)
                                {
                                    var result = await packageManager.RegisterPackageAsync(new Uri(destinationFile.Path),
                                            dependencies,
                                            DeploymentOptions.ForceApplicationShutdown);
                                    browserFallback = false;
                                }
                            }
                        }
                        finally
                        {
                            if (destinationFile != null)
                                await destinationFile.DeleteAsync();
                        }

                        
                    }
                    catch
                    {
                        //lets do fallback to browser
                    }
                    finally
                    {
                        Busy.SetBusy(false);
                    }
                }
            }*/

            if (browserFallback)
            {
                //we can laso open direct link to appx with mainAsset.BrowserDownloadUrl, but Edge waits on click small unseeable "Save" button
                await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl));
            }


        }

        private static async Task DownloadFile(StorageFile destinationFile, Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    using (IOutputStream outstream = (await destinationFile.OpenStreamForWriteAsync()).AsOutputStream())
                    {
                        await response.Content.WriteToStreamAsync(outstream);
                    }
                }
            }
        }

        private static List<ReleaseAsset> FilterAssetsByArchitecture(Release release)
        {
            return release.Assets.Where(asset => asset.Name.ToLower().Contains("_" + Package.Current.Id.Architecture.ToString().ToLower() + "_")).ToList();
        }
    }
}
