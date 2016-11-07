using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChallengePage : Page
    {
        public string ChallengeUri;
        private static Client _client;
        public ChallengePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ChallengeUri = e.Parameter.ToString();
            ChallengePanel.Navigate(new Uri(ChallengeUri));
        }

        private void ChallengePanel_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string url = e.Uri.ToString();
            if (url.StartsWith("unity:"))
            {
                int index = url.IndexOf("unity:");
                if (index != -1)
                {
                    string token = url.Remove(index);
                    Verify(token);
                }
                else
                {
                    //Error, no token!
                }
            }
        }
        private async void Verify(string token)
        {
            await VerifyChallenge(token);
            //BootStrapper.Current.NavigationService.Navigate(typeof(GameMapPage));
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }
        private static async
     Task
         <VerifyChallengeResponse> VerifyChallenge(string token)
        {
            return await _client.Misc.VerifyChallenge(token);
        }
    }
}
