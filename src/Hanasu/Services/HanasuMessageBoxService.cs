using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Services;
using Hanasu.View;
using System.Windows;
using System.ComponentModel.Composition;

namespace Hanasu.Services
{
    [Export(typeof(IMessageBoxService))]
    public class HanasuMessageBoxService: IMessageBoxService
    {
        public void ShowMessage(string title = "Title", string message = "Message")
        {
            MessageBoxWindow mbw = new MessageBoxWindow(title, message, System.Windows.MessageBoxButton.OK);

            if (Application.Current.MainWindow.IsLoaded)
                mbw.Owner = Application.Current.MainWindow;
            else
            {
                mbw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mbw.ShowInTaskbar = true;
            }

            mbw.ShowDialog();
            mbw.Close();
        }

        public bool? ShowOkayCancelMessage(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
