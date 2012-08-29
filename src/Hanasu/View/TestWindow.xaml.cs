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

namespace Hanasu.View
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : MetroWindow, INotifyPropertyChanged
    {
        public TestWindow()
        {
            InitializeComponent();
        }

        private bool _c = false;
        public bool CurrentArtistInfoPaneIsOpen
        {
            get
            {
                return _c;
            }
            set
            {
                if (_c != value)
                {
                    _c = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this,
                            new PropertyChangedEventArgs("CurrentArtistInfoPaneIsOpen"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CurrentArtistInfoPaneIsOpen = !CurrentArtistInfoPaneIsOpen;
        }
    }
}
