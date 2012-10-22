using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locima.SlidingBlock.Converters
{
    /// <summary>
    /// Maps between <see cref="Boolean"/> values and <see cref="Visibility"/> values.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {

        /// <summary>
        /// Maps <c>true</c> to <see cref="Visibility.Visible"/> and anything else to <see cref="Visibility.Collapsed"/>
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Maps <see cref="Visibility.Visible"/> to <c>true</c>and anything else to <c>false</c>
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility) value) == Visibility.Visible;
        }
    }
}