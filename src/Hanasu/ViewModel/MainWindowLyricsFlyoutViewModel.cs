using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Messaging;
using Hanasu.Core.Songs;
using Hanasu.Core;
using System.Timers;

namespace Hanasu.ViewModel
{
    public class MainWindowLyricsFlyoutViewModel : BaseViewModel
    {
        public MainWindowLyricsFlyoutViewModel()
        {
            SelectedItem = null;
            lyricsTimer = new Timer();
            lyricsTimer.Elapsed += new ElapsedEventHandler(lyricsTimer_Elapsed);
            lyricsTimer.Interval = 10;
        }

        void lyricsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            songTime.Add(TimeSpan.FromMilliseconds(lyricsTimer.Interval));
            Dictionary<TimeSpan, string> lyrics = (Dictionary<TimeSpan, string>)Lyrics;
            try
            {
                if (_CurrentSongWasCaughtAtBeginning)
                    SelectedItem = lyrics.First(t => t.Key.CompareTo(songTime) == 0);
            }
            catch (Exception)
            {
            }
        }

        private TimeSpan songTime;
        private bool synchronized = false;
        private Timer lyricsTimer = null;
        private volatile bool _CurrentSongWasCaughtAtBeginning = false;

        [MessageHandler("CurrentSongWasCaughtAtBeginning")]
        public void HandleCurrentSongWasCaughtAtBeginning(object data)
        {
            songTime = new TimeSpan();
            _CurrentSongWasCaughtAtBeginning = (bool)data;

            lyricsTimer.Stop();

            if (_CurrentSongWasCaughtAtBeginning == true)
                lyricsTimer.Start();
        }

        [MessageHandler("LyricsReceived")]
        public void HandleLyricsReceived(object data)
        {
            SongData sd = (SongData)data;

            if (sd.LyricsSynchronized)
            {
                synchronized = true;

                //start displaying lyrics

                if (_CurrentSongWasCaughtAtBeginning)
                {
                }
            }
            else
                synchronized = false;

            Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                {
                    Lyrics = sd.Lyrics;
                }));
        }

        public object SelectedItem
        {
            get { return this.GetProperty("SelectedItem"); }
            set { this.SetProperty("SelectedItem", value); }
        }

        public object Lyrics
        {
            get { return (object)this.GetProperty("Lyrics"); }
            set { this.SetProperty("Lyrics", value); }
        }
    }
}
