using Crystal.Core;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanasuWP8.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            RegisterForMessages("UpdateIndeterminateStatus");
            RegisterForMessages("SwitchTab");

            if (BackgroundAudioPlayerExtensions.SafeGetPlayerState(BackgroundAudioPlayer.Instance) == PlayState.Shutdown || BackgroundAudioPlayer.Instance.Track == null)
                    SelectedTab = 1; //stations
        }

        public override void OnNavigatedTo(KeyValuePair<string, string>[] argument = null)
        {
            base.OnNavigatedTo(argument);
        }

        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            switch (message.MessageString)
            {
                case "UpdateIndeterminateStatus":
                    {
                        try
                        {
                            Tuple<bool, string> tup = (Tuple<bool, string>)message.Data;

                            Microsoft.Phone.Shell.SystemTray.ProgressIndicator.Text = tup.Item2;
                            Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsIndeterminate = true;
                            Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsVisible = tup.Item1;
                        }
                        catch (Exception) { }
                        return true;
                    }
                case "SwitchTab":
                    {
                        SelectedTab = (int)message.Data;
                        return true;
                    }
                default:
                    return base.ReceiveMessage(source, message);
            }

        }

        public int SelectedTab
        {
            get { return GetPropertyOrDefaultType<int>(x => this.SelectedTab); }
            set { SetProperty(x => this.SelectedTab, value); }
        }
    }
}
