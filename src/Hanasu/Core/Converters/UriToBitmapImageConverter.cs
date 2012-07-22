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
    using System.Collections;
    using System.Threading;
    using System.Windows;
    using Hanasu.Services.Friends;

    public class UriToBitmapImageConverter : IValueConverter // from http://blog.davidsandor.com/post/2010/05/12/How-To-WPF-Databind-a-URL-URI-to-an-Image-control.aspx
    {
        private static Hashtable ImageCache = new Hashtable();
        public object Convert(object value, Type targetType, object parameter,
             CultureInfo culture)
        {
            BitmapImage image = new BitmapImage();
            if (value != null)
            {
                try
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.Default;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                    Uri url = null;

                    if (value is Uri)
                        url = (Uri)value;
                    else
                        url = new Uri((string)value, UriKind.Absolute);

                    if (ImageCache.ContainsKey(url))
                        return (BitmapImage)ImageCache[url];
                    else
                        image.UriSource = url;

                    if (parameter != null)
                    {
                        if (parameter is int[])
                        {
                            int[] info = (int[])parameter;

                            image.DecodePixelHeight = info[0];
                            image.DecodePixelWidth = info[1];
                        }
                        else if (parameter is int)
                        {
                            image.DecodePixelWidth = (int)parameter;
                        }
                        else if (parameter is string)
                        {
                            int outint = 0;
                            
                            if (int.TryParse((string)parameter, out outint))
                                image.DecodePixelWidth = (int)outint;
                        }
                    }

                    image.EndInit();

                    if (!ImageCache.ContainsKey(url))
                        ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                            {
                                if (!ImageCache.ContainsKey(url))
                                    ImageCache.Add(url, null);
                                else
                                    return;

                                bool running = true;
                                while (running)
                                {
                                    if (Application.Current == null) return;

                                    Application.Current.Dispatcher.Invoke(new EmptyDelegate(() =>
                                        {
                                            running = image.IsDownloading;

                                        }));
                                    Thread.Sleep(50);
                                }

                                if (Application.Current == null) return;

                                Application.Current.Dispatcher.Invoke(new EmptyDelegate(() =>
                                {
                                    ImageCache[url] = image.GetAsFrozen();
                                }));
                            }));
                }
                catch
                {
                    image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri("pack://application:,,,/Hanasu;component/Resources/cancel.png", UriKind.Absolute);
                    image.EndInit();
                }
            }
            else
            {
                image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://application:,,,/Hanasu;component/Resources/cancel.png", UriKind.Absolute);
                image.EndInit();
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

