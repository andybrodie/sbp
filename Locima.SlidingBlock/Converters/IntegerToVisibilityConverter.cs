using System;
using System.Globalization;
using System.Windows.Data;

namespace Locima.SlidingBlock.Converters
{
    public class IntegerToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverter = parameter != null && !((bool) parameter);
            bool valueIsZero = (value == null) || (((int) value) == 0);
            return valueIsZero ^ inverter ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
