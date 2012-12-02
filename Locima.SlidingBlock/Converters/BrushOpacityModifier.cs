using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NLog;

namespace Locima.SlidingBlock.Converters
{
    /// <summary>
    /// Changes the opacity of the passed brush to the value passe din the converter parameter
    /// </summary>
    public class BrushOpacityModifier : IValueConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Converts the <see cref="Brush.Opacity"/> property of the <see cref="Brush"/> passed by <paramref name="value"/> to the value passed in <paramref name="parameter"/>
        /// </summary>
        /// <param name="value">The brush to change the <see cref="Brush.Opacity"/> of</param>
        /// <param name="targetType">Ignored</param>
        /// <param name="parameter">The opacity to set the <see cref="Brush.Opacity"/> to, must be parseable by <see cref="double.TryParse(string,out double)"/></param>
        /// <param name="culture">Ignored</param>
        /// <returns>A modified version of the brush passed by <paramref name="value"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = (Brush) value;
            double opacity;
            if (double.TryParse((string) parameter, out opacity))
            {
                brush.Opacity = opacity;
            }
            else
            {
                Logger.Error("Non-double opacity parameter passed {0}", parameter);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}