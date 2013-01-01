using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Hanasu.Converters
{
    public class NowPlayingPageShoutcastOperationVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((Hanasu.ViewModel.NowPlayingPageViewModel.SongHistoryOperationStatusType)value)
            {
                default:
                    return Visibility.Visible;
                case ViewModel.NowPlayingPageViewModel.SongHistoryOperationStatusType.DataReturned:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class NowPlayingPageShoutcastOperationVisibilityReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((Hanasu.ViewModel.NowPlayingPageViewModel.SongHistoryOperationStatusType)value)
            {
                default:
                    return Visibility.Collapsed;
                case ViewModel.NowPlayingPageViewModel.SongHistoryOperationStatusType.DataReturned:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
