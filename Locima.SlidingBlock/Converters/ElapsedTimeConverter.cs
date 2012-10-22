using System;
using System.Globalization;
using System.Windows.Data;

namespace Locima.SlidingBlock.Converters
{
    /// <summary>
    /// Converts a TimeSpan object to a string and back again
    /// </summary>
    
    public class ElapsedTimeConverter : IValueConverter
    {

        /// <summary>
        /// Converts a TimeSpan object to a string
        /// </summary>
        /// <remarks>
        /// Uses <see cref="TimeSpan.ToString(string)"/> to convert, using the <paramref name="parameter"/> as the formatting string.  This method is dumb and assumes all values are valid.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan duration = (TimeSpan) value;
            string format = (parameter == null) ? "c" : ((string) parameter);
            return duration.ToString(format, culture);
        }


        /// <summary>
        /// Converts back from a <see cref="string"/> to a <see cref="TimeSpan"/> object.
        /// </summary>
        /// <remarks>
        /// This method is dumb and assumes all values are valid.</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.Parse((string)value, culture);
        }
    }
}