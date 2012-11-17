using System;
using System.Globalization;
using System.Resources;
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

        /// <summary>
        /// Converts a <see cref="Scrambler.ScrambleType"/> to a localised text equivalent, for use in <see cref="LevelEditor"/>
        /// </summary>
        /// <param name="value">The <see cref="Scrambler.ScrambleType"/></param>
        /// <param name="targetType"><see cref="string"/></param>
        /// <param name="parameter">unused</param>
        /// <param name="culture">unused (<see cref="LocalizationHelper.GetString"/> uses default culture (delegates to <see cref="ResourceManager.GetString(string)"/>)</param>
        /// <returns>Text scribing the scramble type</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Scrambler.ScrambleType type = (Scrambler.ScrambleType) value;
            return LocalizationHelper.GetString(string.Format("ScramblerType_{0}", type.ToString()));
        }


        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">unused</param>
        /// <param name="targetType">unused</param>
        /// <param name="parameter">unused</param>
        /// <param name="culture">unused</param>
        /// <returns>nothing</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}