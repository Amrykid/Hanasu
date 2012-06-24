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
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : MetroWindow
    {
        public ErrorWindow()
        {
            InitializeComponent();
            this.Unloaded += ErrorWindow_Unloaded;
        }

        void ErrorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.button1.Click -= new System.Windows.RoutedEventHandler(this.button1_Click);
            this.button2.Click -= new System.Windows.RoutedEventHandler(this.button2_Click);
            this.Unloaded -= ErrorWindow_Unloaded;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBlock1.Text, TextDataFormat.Text);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
