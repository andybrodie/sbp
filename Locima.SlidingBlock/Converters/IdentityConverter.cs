using System;
using System.Globalization;
using System.Windows.Data;
using NLog;

namespace Locima.SlidingBlock.Converters
{

    /// <summary>
    /// Identity converter that passes pack whatever value is passed back in
    /// </summary>
    /// <remarks>
    /// This is useful for debugging when a binding update is invoked, by setting a breakpoint on the <see cref="Convert"/> or <see cref="ConvertBack"/> methods</remarks>.  This also
    /// sends the parameters to the log
    public class IdentityConverter : IValueConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region IValueConverter Members

        /// <summary>
        /// Returns <paramref name="value"/>
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Logger.Debug("Binding from {0} passing value {1} ({2}) invoked", targetType, value, value==null ? "NULL" : value.GetType().FullName);
            return value;
        }

        /// <summary>
        /// Returns <paramref name="value"/>
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Logger.Debug("Binding from {0} passing value {1} ({2}) invoked", targetType, value, value == null ? "NULL" : value.GetType().FullName);
            return value;
        }

        #endregion
    }
}