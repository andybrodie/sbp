using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locima.SlidingBlock.Converters
{

    /// <summary>
    /// Converts a zero or non-zero <see cref="int"/> to either <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/>, depending on the convert parameter
    /// </summary>
    public class IntegerToVisibilityConverter : IValueConverter
    {

        /// <summary>
        /// Converts a zero or non-zero integer to either <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/>, depending on the convert parameter
        /// </summary>
        /// <param name="value">The value that will be interpreted as a integer</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Acts as an inverter.  If <c>true</c> is passed then 0 maps to <see cref="Visibility.Visible"/> and non-zero maps to <see cref="Visibility.Collapsed"/>.  Any other value has no effect.  Not case-sensitive</param>
        /// <param name="culture">Ignored</param>
        /// <returns>Either <see cref="Visibility.Visible"/> or <see cref="Visibility.Collapsed"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverter = string.Equals("true", parameter as string, StringComparison.InvariantCultureIgnoreCase);
            bool valueIsZero = (value == null) || (((int) value) == 0);
            return valueIsZero ^ inverter ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
