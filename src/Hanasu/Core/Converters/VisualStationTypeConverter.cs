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

            object res = null;

            try
            {
                switch (s)
                {
                    case StationType.Radio: res = Application.Current.MainWindow.FindResource("appbar_music");
                        break;
                    case StationType.TV: res = Application.Current.MainWindow.FindResource("appbar_tv");
                        break;
                }
            }
            catch (Exception ex)
            {
            }

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
