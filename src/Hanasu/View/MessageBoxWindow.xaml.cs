using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using MahApps.Metro.Controls;
using Hanasu.ViewModel;

namespace Hanasu.View
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : MetroWindow
    {
        public MessageBoxWindow(string title, string message, MessageBoxButton ops = MessageBoxButton.OK)
        {
            InitializeComponent();

            ((MessageBoxWindowViewModel)this.DataContext).Title = title;
            ((MessageBoxWindowViewModel)this.DataContext).Message = message;
            ((MessageBoxWindowViewModel)this.DataContext).Options = ops;
        }
    }
}
