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

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for PLSStreamChooseWindow.xaml
    /// </summary>
    public partial class PLSStreamChooseWindow : MetroWindow
    {
        public PLSStreamChooseWindow()
        {
            InitializeComponent();
        }

        private void listBox1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (listBox1.SelectedItem != null)
                this.DialogResult = true;
        }
    }
}
