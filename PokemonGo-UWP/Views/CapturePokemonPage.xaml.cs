using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Common;
using PokemonGo.RocketAPI;
using Windows.System.Threading;
using System.Numerics;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CapturePokemonPage : Page
    {
        public CapturePokemonPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Of course binding doesn't work so we need to manually setup height for animations
                ShowInventoryDoubleAnimation.From =
                    HideInventoryDoubleAnimation.To = InventoryMenuTranslateTransform.Y = ActualHeight*3/2;
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }

        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            ViewModel.CatchSuccess += GameManagerViewModelOnCatchSuccess;
            ViewModel.CatchEscape += GameManagerViewModelOnCatchEscape;
            ViewModel.CatchMissed += GameManagerViewModelOnCatchMissed;
            // Add also handlers to enable the button once the animation is done            
            // TODO: fix names for actions in capture score and choose a proper font
            CatchSuccess.Completed += (s, e) => ShowCaptureStatsStoryboard.Begin();
            CatchEscape.Completed += (s, e) => ShowCaptureStatsStoryboard.Begin();
            CatchMissed.Completed += (s, e) => LaunchPokeballButton.IsEnabled = true;
        }

        private void UnsubscribeToCaptureEvents()
        {
            ViewModel.CatchSuccess -= GameManagerViewModelOnCatchSuccess;
            ViewModel.CatchEscape -= GameManagerViewModelOnCatchEscape;
            ViewModel.CatchMissed -= GameManagerViewModelOnCatchMissed;
        }

        private void GameManagerViewModelOnCatchMissed(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchMissed.Begin();
        }

        private void GameManagerViewModelOnCatchEscape(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchEscape.Begin();
        }

        private void GameManagerViewModelOnCatchSuccess(object sender, EventArgs eventArgs)
        {
            LaunchPokeballButton.IsEnabled = false;
            CatchSuccess.Begin();
        }

        #endregion

        private float StartingX;
        private float StartingY;
        private DateTime StartingTime;

        private float PokemonPosition;
        private float PokemonRadius;

        private Vector3 ThrowItemVelocity;
        private Vector3 ThrowItemPosition;
        private volatile Mutex UpdateLoopMutex = new Mutex();

        private DateTime prevTime;
        private async void PokeballUpdateLoop(ThreadPoolTimer timer)
        {
            if (UpdateLoopMutex.WaitOne(0))
            {
                DateTime curTime = DateTime.Now;

                float timeDelta = (curTime - prevTime).Milliseconds / 1000f;
                Vector3 gravity = new Vector3(0, 300f, 0);

                ThrowItemPosition += (ThrowItemVelocity * timeDelta) + (.5f * gravity * timeDelta * timeDelta);
                ThrowItemVelocity += (gravity * timeDelta);

                Logger.Write("Position" + ThrowItemPosition.X + ", " + ThrowItemPosition.Y + ", " + ThrowItemPosition.Z);
                Logger.Write("Velocity" + ThrowItemVelocity.X + ", " + ThrowItemVelocity.Y + ", " + ThrowItemVelocity.Z);

                prevTime = curTime;

                var translateX = ThrowItemPosition.X;
                var translateY = ThrowItemPosition.Y + (ThrowItemPosition.Z * 50.0f);
                var scaleX = Math.Max(1.0f - ThrowItemPosition.Z, 0.2f);
                var scaleY = scaleX;
                var pokeballStopped = false;

                if (ThrowItemPosition.Y > -50)
                {
                    timer.Cancel();
                    pokeballStopped = true;
                }


                UpdateLoopMutex.ReleaseMutex();

                await PokeballTransform.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    PokeballTransform.TranslateX = translateX;
                    PokeballTransform.TranslateY = translateY;
                    PokeballTransform.ScaleX = scaleX;
                    PokeballTransform.ScaleY = scaleY;
                    if(pokeballStopped) ViewModel.UseSelectedCaptureItem.Execute();
                });
            }
        }
                
        private void LaunchPokeballButton_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            Logger.Write("Manipulation Started...");
            PokeballTransform.ScaleX *= 1.05f;
            PokeballTransform.ScaleY *= 1.05f;
            StartingX = (float)PokeballTransform.TranslateX;
            StartingY = (float)PokeballTransform.TranslateY;
            StartingTime = DateTime.Now;
        }

        private void LaunchPokeballButton_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            Logger.Write("Manipulation Delta...");
            PokeballTransform.TranslateX += e.Delta.Translation.X;
            PokeballTransform.TranslateY += e.Delta.Translation.Y;
        }

        private void LaunchPokeballButton_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            Logger.Write("Manipulation Completed...");

            var EndingX = (float)PokeballTransform.TranslateX;
            var EndingY = (float)PokeballTransform.TranslateY;

            var displacement = new Vector2(StartingX - EndingX, StartingY - EndingY);
            var distance = -1 * displacement.Length();
            var throwDirection = Vector3.Normalize(new Vector3(displacement.X, displacement.Y, 1.0f));
            var speed = 3200000.0f / (StartingTime - DateTime.Now).Milliseconds;
            prevTime = DateTime.Now;

            ThrowItemPosition = new Vector3(EndingX, EndingY, 0);
            ThrowItemVelocity = (speed / distance) * throwDirection;
            //ThrowItemVelocity.Z *= 2;

            Logger.Write("Init Position" + ThrowItemPosition.X + ", " + ThrowItemPosition.X + ", " + ThrowItemPosition.Z);
            Logger.Write("Init Velocity" + ThrowItemVelocity.X + ", " + ThrowItemVelocity.X + ", " + ThrowItemVelocity.Z);

            ThreadPoolTimer.CreatePeriodicTimer(PokeballUpdateLoop, TimeSpan.FromMilliseconds(20));
        }
    }
}