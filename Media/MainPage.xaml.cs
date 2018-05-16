using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Media
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        MediaTimelineController mediaTimelineController = new MediaTimelineController();
        TimeSpan duration;
        DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();

            var mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///"));
            mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
            
            mediaPlayer.Source = mediaSource;
            mediaPlayer.CommandManager.IsEnabled = false;
            mediaPlayer.TimelineController = mediaTimelineController;

            mediaPlayerElement.SetMediaPlayer(mediaPlayer);
            nowTime.Text = "";
        }

        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            duration = sender.Duration.GetValueOrDefault();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                timeLine.Minimum = 0;
                timeLine.Maximum = duration.TotalSeconds;
                timeLine.StepFrequency = 1;
                nowTime.Text = "00:00";
                totalTime.Text = TimeFormat(duration);
            });
        }
        
        private async void Timer_Tick(object sender, object e)
        {
            timeLine.Value = mediaTimelineController.Position.TotalSeconds;
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                nowTime.Text = TimeFormat((mediaTimelineController.Position));
            }));
            if (timeLine.Value == timeLine.Maximum)
            {
                PlayOver();
            }
        }

        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            if (Volumn.Value != 0)
            {
                Volumn.Value = 0;
            }
            else
            {
                Volumn.Value = 80;
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            storyboard.Pause();
            mediaTimelineController.Pause();
            Show_PlayButton();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (duration.TotalSeconds == 0) return;
            if (storyboard.GetCurrentState() == Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
            {
                timer.Start();
                storyboard.Begin();
            }
            else
            {
                timer.Start();
                storyboard.Resume();
            }
            if (timeLine.Value == 0)
            {
                mediaTimelineController.Start();
            }
            else
            {
                mediaTimelineController.Resume();
            }
            Show_PauseButton();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            PlayOver();
        }
        
        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            timeLine.Value -= 5;
            if (timeLine.Value < 0)
            {
                timeLine.Value = 0;
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            timeLine.Value += 5;
            if (timeLine.Value > duration.TotalSeconds)
            {
                timeLine.Value = duration.TotalSeconds;
            }
        }
        /*
        private void List_Click(object sender, RoutedEventArgs e)
        {
            
        }
        */
        private void Display_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            storyboard.Pause();
            mediaTimelineController.Pause();

            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".wma");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                mediaPlayer.Source = mediaSource;
                if (file.FileType == ".mp3" || file.FileType == ".wma")
                {
                    Picture.Visibility = Visibility.Visible;
                }
                else
                {
                    Picture.Visibility = Visibility.Collapsed;
                }
                Flush(file.DisplayName);
            }
            Show_PlayButton();
        }
        
        private void Flush(string str)
        {
            Title.Text = str;
            timeLine.Value = 0;
            PlayOver();
            doubleAnimation.From = 0;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            /*
            timer.Tick += new EventHandler<object>(async (sender, e) =>
            {
                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                {
                    nowTime.Text = TimeFormat((mediaTimelineController.Position));
                }));
            });
            */
        }

        private string TimeFormat(TimeSpan time)
        {
            string str = "";
            if (time.TotalSeconds == 0) return "00:00";

            if (time.Hours > 0) str += time.Hours.ToString() + ":";

            if (time.Minutes < 10) str += "0";
            str += time.Minutes.ToString() + ":";

            if (time.Seconds < 10) str += "0";
            str += time.Seconds.ToString();

            return str;
        }

        private void Volumn_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            mediaPlayer.Volume = Volumn.Value * 0.01;
        }
        
        private void Show_PlayButton()
        {
            pause.Visibility = Visibility.Collapsed;
            start.Visibility = Visibility.Visible;
        }

        private void Show_PauseButton()
        {
            pause.Visibility = Visibility.Visible;
            start.Visibility = Visibility.Collapsed;
        }

        private void PlayOver()
        {
            mediaTimelineController.Position = TimeSpan.FromSeconds(0);
            mediaTimelineController.Pause();
            storyboard.Stop();
            timeLine.Value = 0;
            Show_PlayButton();
            nowTime.Text = TimeFormat((mediaTimelineController.Position));
        }
    }
}