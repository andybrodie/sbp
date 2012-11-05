using System;
using System.Globalization;
using System.Windows.Data;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;

namespace Locima.SlidingBlock.Converters
{
    /// <summary>
    /// Converts a <see cref="Position"/> to a <see cref="String"/>
    /// </summary>
    /// <remarks>
    /// Used for debugging
    /// </remarks>
    public class PositionToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="Position"/> then returns it as a string
        /// </summary>
        /// <remarks>
        /// This is used for debugging the <see cref="Puzzle"/>, it's helpful to know where each tile should be, so this allows us to put the position overlaid on the tile</remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Position))
                return String.Empty;
            Position pos = (Position) value;
            return String.Format("({0},{1})", pos.X, pos.Y);
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