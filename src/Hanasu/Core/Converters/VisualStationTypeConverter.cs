using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Hanasu.Services.Stations;
using System.Windows;

namespace Hanasu.Core.Converters
{
    public class VisualStationTypeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            StationType s = (StationType)value;

            try
            {
                switch (s)
                {
                    case StationType.Radio: return (System.Windows.Media.Visual)Application.Current.MainWindow.FindResource("appbar_music");
                    case StationType.TV: return (System.Windows.Media.Visual)Application.Current.MainWindow.FindResource("appbar_tv");
                }
            }
            catch (Exception ex)
            {
            }

            return Application.Current.MainWindow.FindResource("appbar_music");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
