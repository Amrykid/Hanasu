using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Hanasu.Services.Stations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hanasu.Core.Converters
{
    public class VisualStationTypeConverter: IValueConverter
    {
        private Lazy<BitmapImage> RadioLazy = new Lazy<BitmapImage>(new Func<BitmapImage>(() =>
            {
                BitmapImage res = new BitmapImage();
                res.BeginInit();
                res.UriSource = new Uri("pack://application:,,,/Hanasu;component/Resources/play.png", UriKind.Absolute);
                res.EndInit();
                return (BitmapImage)res.GetAsFrozen();
            }));
        private Lazy<BitmapImage> TVLazy = new Lazy<BitmapImage>(new Func<BitmapImage>(() =>
        {
            BitmapImage res = new BitmapImage();
            res.BeginInit();
            res.UriSource = new Uri("pack://application:,,,/Hanasu;component/Resources/video.png", UriKind.Absolute);
            res.EndInit();
            return (BitmapImage)res.GetAsFrozen();
        }));
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            StationType s = (StationType)value; 
            try
            {
                switch (s)
                {
                    case StationType.Radio: return RadioLazy.Value;
                    case StationType.TV: return TVLazy.Value;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
