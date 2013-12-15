using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hanasu.ViewModel;
using MahApps.Metro.Controls;

namespace Hanasu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MainWindowViewModel))]
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public async void Moo()
        {

        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InfoPane.Visibility = tabControl.SelectedIndex == 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            NowPlayingPane.Visibility = tabControl.SelectedIndex == 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
    }
}
