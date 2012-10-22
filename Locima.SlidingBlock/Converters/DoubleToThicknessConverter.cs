using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locima.SlidingBlock.Converters
{
    public class DoubleToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = Double.Parse(value.ToString());
            return new Thickness(d, d, d, d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}