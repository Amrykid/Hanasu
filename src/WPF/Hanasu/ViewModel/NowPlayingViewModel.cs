using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Hanasu.Model;

namespace Hanasu.ViewModel
{
    public class NowPlayingViewModel : BaseViewModel
    {
        public NowPlayingViewModel()
        {

            IsPlaying = true;
            CurrentStation = new Station() { Title = "XAMFM", ImageUrl = "https://si0.twimg.com/profile_images/1104224483/logo_transbkgr.png" };
            CurrentTrack = "Uprising";
            CurrentArtist = "Pieman";

        }

        public bool IsPlaying
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsPlaying); }
            set { SetProperty(x => this.IsPlaying, value); }
        }

        public Station CurrentStation
        {
            get { return GetPropertyOrDefaultType<Station>(x => this.CurrentStation); }
            set { SetProperty(x => this.CurrentStation, value); }
        }

        public string CurrentTrack
        {
            get { return GetPropertyOrDefaultType<string>(x => this.CurrentTrack); }
            set { SetProperty(x => this.CurrentTrack, value); }
        }

        public string CurrentArtist
        {
            get { return GetPropertyOrDefaultType<string>(x => this.CurrentArtist); }
            set { SetProperty(x => this.CurrentArtist, value); }
        }
    }
}
