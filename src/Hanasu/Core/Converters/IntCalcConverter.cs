// -----------------------------------------------------------------------
// <copyright file="IntCalcConverter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Hanasu.Core.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class IntCalcConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var equa = parameter as string;
            var bits = equa.Split(';');

            bool isPercent = bits[1].StartsWith("%") && (double)value > 0.0;

            double result = 0.0;

            if ((double)value == 0.0 && isPercent == false && bits[1].StartsWith("%"))
                bits[1] = bits[1].Substring(1);

            double rightValue = (isPercent ? ((int.Parse(bits[1].Substring(1))) / 100.0) * (double)value : int.Parse(bits[1]));

            switch(bits[0])
            {
                case "*": result = (double)value * rightValue;
                    break;
                case "+": result = (double)value + rightValue;
                    break;
                case "-": result = (double)value - rightValue;
                    break;
                case "/": result = (double)value / rightValue;
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
