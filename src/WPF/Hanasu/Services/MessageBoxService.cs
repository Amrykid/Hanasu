using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Crystal.Services;

namespace Hanasu.Services
{
    [Export(typeof(IMessageBoxService))]
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowMessage(string title = "Title", string message = "Message")
        {
            //MessageBox.Show(message, title);
            (App.Current.MainWindow as MahApps.Metro.Controls.MetroWindow).ShowMessageAsync(title, message);
        }

        public bool? ShowOkayCancelMessage(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
