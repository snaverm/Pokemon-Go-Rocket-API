using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using System;

namespace PokemonGo_UWP.Utils
{
    public static class AudioUtils
    {  
        private static bool isPlaying;
        public static bool IsPlaying { get; set; }
        public static MediaElement mysong1 = new MediaElement();
        public static MediaElement mysong = new MediaElement();
        public static async Task PlaySound(string asset)
        {
            if (SettingsService.Instance.IsMusicEnabled)
            {
                if((string)asset == "Gameplay.mp3")
                {
                    mysong1.IsLooping = true;
                }
                var folder =
                       await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
                var file = await folder.GetFileAsync(asset);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                mysong1.Volume = 1;
                mysong1.SetSource(stream, file.ContentType);
                IsPlaying = true;
                mysong1.Play();
                            
            }
        }

        public static async Task PlaySoundCapture(string asset)
        {
            if (SettingsService.Instance.IsMusicEnabled)
            {
                mysong1.Pause();

                var folder =
                    await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
                var file = await folder.GetFileAsync(asset);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                mysong.Volume = 1;
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
        }
    }
}
