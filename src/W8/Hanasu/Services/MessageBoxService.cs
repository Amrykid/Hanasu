using Crystal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Hanasu.Services
{
    public class MessageBoxService: IMessageBoxService
    {
        public async void ShowMessage(string title = "Title", string message = "Message")
        {
            try
            {
                MessageDialog md = new MessageDialog(message, title);
                await md.ShowAsync();
            }
            catch (Exception)
            {
            }
        }

        public bool? ShowOkayCancelMessage(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
