using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Veeplay.media;
using Windows.System.Display;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Veeplay.utillities;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VeesoDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static bool navigatedfrom = false;
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // pause playback when navigating from page
            navigatedfrom = true;
            MediaPlayer.getInstance().pause();
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            // Resume playback when navigated to this page
            if (navigatedfrom)
                MediaPlayer.getInstance().resumePlay();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // init media player
            MediaPlayer.debugMode = true;
            MediaPlayer.getInstance().init(this, true);
            // handle MediaPlayer events
            MediaPlayer.getInstance().PlayerEvent += MainPage_PlayerEvent;
            // add player to UI layout
            displayGrid.Children.Add(MediaPlayer.getInstance().getMainView());
            //urlBox.Text = "http://sergiu.freshweb.ro/veeplay/tests/json/15VastDisplayAd.json";
        }

        void MainPage_PlayerEvent(object sender, Veeplay.media.events.MediaPlayerEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Type: " + e.Type + " Event: " + e.Message + ((e.Urls != null) ? (" Urls: " + e.Urls.Count) : ""));

            if (e.Type == MediaTrackingEvents.MediaEventType.START_PROCESSING_NEW_UNIT)
            {
                if (e.Message == "START_PROCESSING_NEW_UNIT")
                {
                    finishedText.Visibility = Visibility.Collapsed;
                    foreach (var btn in ButtonPanel.Children)
                        if (btn is Button) ((Button)btn).IsEnabled = true;
                }
            }

            if (e.Type == MediaTrackingEvents.MediaEventType.PLAYLIST_FINISHED)
            {
                if (e.Message == "playlist_finished")
                {
                    finishedText.Visibility = Visibility.Visible;
                    foreach (var btn in ButtonPanel.Children)
                        if (btn is Button) ((Button)btn).IsEnabled = false;
                }
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.getInstance().next();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.getInstance().stop();
        }

        private void urlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (urlBox.Text.Equals(""))
                playlistBtn.IsEnabled = false;
            else
                playlistBtn.IsEnabled = true;
        }

        private async void Play_Playlist_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.getInstance().stop();

            playlistBtn.IsEnabled = false;
            urlBox.IsEnabled = false;
            urlLoading.IsIndeterminate = true;
            urlLoading.Visibility = Visibility.Visible;

            try
            {
                // configure builder from json url
                MediaBuilder builder = new MediaBuilder();
                var url = urlBox.Text;
                await builder.configureFromURLAsync(url);

                // play media units
                MediaPlayer.getInstance().playMediaUnits(await builder.mediaUnitsAsync());
            }
            catch
            {
                var dialog = new MessageDialog("Invalid url");
                var task = dialog.ShowAsync();
            }

            urlLoading.IsIndeterminate = false;
            urlLoading.Visibility = Visibility.Collapsed;
            urlBox.IsEnabled = true;
            if (!urlBox.Text.Equals(""))
                playlistBtn.IsEnabled = true;
        }
    }
}
