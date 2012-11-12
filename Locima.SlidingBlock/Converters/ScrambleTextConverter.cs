using System;
using System.Globalization;
using System.Windows.Data;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.Converters
{

    /// <summary>
    /// Converts a <see cref="Scrambler.ScrambleType"/> value to a readable string
    /// </summary>
    public class ScrambleTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Scrambler.ScrambleType type = (Scrambler.ScrambleType) value;
            return LocalizationHelper.GetString(string.Format("ScramblerType_{0}", type.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}