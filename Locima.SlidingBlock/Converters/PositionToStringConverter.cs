using System;
using System.Globalization;
using System.Windows.Data;
using Locima.SlidingBlock.Common;

namespace Locima.SlidingBlock.Converters
{
    public class PositionToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Position))
                throw new ArgumentException("Only instances of Position may be passed, you passed " + value);
            Position pos = (Position) value;
            return String.Format("({0},{1})", pos.X, pos.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}