using Crystal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HanasuWP8.Services
{
    public class MessageBoxService: IMessageBoxService
    {
        public void ShowMessage(string title = "Title", string message = "Message")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK);
        }

        public bool? ShowOkayCancelMessage(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }
    }
}
