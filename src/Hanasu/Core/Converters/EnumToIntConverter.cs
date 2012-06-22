using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Hanasu.Services.Stations;

namespace Hanasu.Core.Converters
{
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //I could of immiedately return instead of using a variable but its for making debugging easier.
            if (value == null)
                return 0;

            int res = 0;
            if (value is Station)
                res = (int)((Station)value).StationType;
            else
                res = (int)value;

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
