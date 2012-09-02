using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Messaging;
using Hanasu.Core.Songs;
using Hanasu.Core;

namespace Hanasu.ViewModel
{
    public class MainWindowLyricsFlyoutViewModel: BaseViewModel
    {
        public MainWindowLyricsFlyoutViewModel()
        {
            SelectedItem = null;
        }

        private bool synchronized = false;
        private volatile bool _CurrentSongWasCaughtAtBeginning = false;

        [MessageHandler("CurrentSongWasCaughtAtBeginning")]
        public void HandleCurrentSongWasCaughtAtBeginning(object data)
        {
            _CurrentSongWasCaughtAtBeginning = (bool)data;
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

            Lyrics = sd.Lyrics;
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
