// -----------------------------------------------------------------------
// <copyright file="UriToBitmapImageConverter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Hanasu.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;
    using System.Globalization;

    public class UriToBitmapImageConverter : IValueConverter // from http://blog.davidsandor.com/post/2010/05/12/How-To-WPF-Databind-a-URL-URI-to-an-Image-control.aspx
    {
        public object Convert(object value, Type targetType, object parameter,
             CultureInfo culture)
        {
            BitmapImage image = new BitmapImage();
            if (value != null)
            {
                try
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    if (value is Uri)
                        image.UriSource = (Uri)value;
                    else
                        new Uri((string)value, UriKind.Absolute);
                    image.EndInit();
                }
                catch
                {
                    image = null;
                }
            }

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
               CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

