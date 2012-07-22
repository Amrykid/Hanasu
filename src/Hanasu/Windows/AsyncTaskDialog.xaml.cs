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
using MahApps.Metro.Controls;
using System.Dynamic;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for AsyncTaskDialog.xaml
    /// </summary>
    public partial class AsyncTaskDialog : MetroWindow
    {
        public AsyncTaskDialog()
        {
            InitializeComponent();
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
            this.Unloaded += AsyncTaskDialog_Unloaded;
        }

        void AsyncTaskDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= AsyncTaskDialog_Unloaded;
            BindingOperations.ClearAllBindings(this);
        }
        public static AsyncTaskDialog GetNewTaskDialog(Window owner, string title, string message)
        {
            dynamic d = new ExpandoObject();
            d.Title = title;
            d.Message = message;

            AsyncTaskDialog t = new AsyncTaskDialog();
            t.DataContext = d;
            t.Owner = owner;

            return t;
        }
    }
}
