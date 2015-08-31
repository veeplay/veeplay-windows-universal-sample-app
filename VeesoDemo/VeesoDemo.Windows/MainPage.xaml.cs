using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Veeplay.media;
using Veeplay.media.events;
using Veeplay.media.renderers;
using Veeplay.utillities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VeesoDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Loaded += MainPage_Loaded;
            SizeChanged += MainPage_SizeChanged;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // init media player
            MediaPlayer.debugMode = true;
            MediaPlayer.debugLevel = 3;
            MediaPlayer.getInstance().init(this, false);
            // handle MediaPlayer events
            MediaPlayer.getInstance().PlayerEvent += MainPage_PlayerEvent;
            // add player to UI layout
            displayGrid.Children.Add(MediaPlayer.getInstance().getMainView());
        }

        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 700)
            {
                urlContainer.ColumnDefinitions[1].Width = new GridLength(600);
                urlContainer.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                urlContainer.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                urlContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.getInstance().back();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.getInstance().skip();
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
            urlLoading.IsActive = true;

            try
            {
                // configure builder from json url
                MediaBuilder builder = new MediaBuilder();
                await builder.configureFromURLAsync(urlBox.Text);

                // play media units
                MediaPlayer.getInstance().playMediaUnits(await builder.mediaUnitsAsync());
            }
            catch
            {
                var dialog = new MessageDialog("Invalid url");
                var task = dialog.ShowAsync();
            }

            urlLoading.IsActive = false;
            urlBox.IsEnabled = true;
            if (!urlBox.Text.Equals(""))
                playlistBtn.IsEnabled = true;
        }
    }
}
