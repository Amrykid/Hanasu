using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HanasuWP8.ViewModel;

namespace HanasuWP8.View
{
    [Crystal.Navigation.NavigationSetViewModel(typeof(SongHistoryViewModel))]
    public partial class SongHistoryPage : PhoneApplicationPage
    {
        public SongHistoryPage()
        {
            InitializeComponent();
        }
    }
}