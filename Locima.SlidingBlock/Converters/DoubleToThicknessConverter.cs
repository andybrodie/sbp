using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locima.SlidingBlock.Converters
{

    /// <summary>
    /// Converts the double value to a <see cref="Thickness"/> with the same value for all sides of the frame.
    /// </summary>
    public class DoubleToThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Attempts to convert <paramref name="value"/> to a <see cref="double"/> then returns a new <see cref="Thickness"/> object using <paramref name="value"/>
        /// for the thickness of each side
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = Double.Parse(value.ToString());
            return new Thickness(d, d, d, d);
        }

        /// <summary>
        /// Casts <paramref name="value"/> to a <see cref="Thickness"/> instance and returns the value of <see cref="Thickness.Top"/>
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness t = (Thickness) value;
            return t.Top;
        }
    }
}