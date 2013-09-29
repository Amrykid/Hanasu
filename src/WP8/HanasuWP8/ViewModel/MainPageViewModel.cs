using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanasuWP8.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            switch (message.MessageString)
            {
                case "UpdateIndeterminateStatus":
                    {
                        Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsIndeterminate = true;
                        Microsoft.Phone.Shell.SystemTray.ProgressIndicator.IsVisible = (bool)message.Data;
                        return true;
                    }
                default:
                    return base.ReceiveMessage(source, message);
            }

        }
    }
}
