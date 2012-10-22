using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NLog;

namespace Locima.SlidingBlock.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean? castedValue = value as Boolean?;
            bool boolValue;
            if (value == null)
            {
                boolValue = false;
            }
            else if (value is bool)
            {
                boolValue = (bool) value;
            }
            else if (!Boolean.TryParse(value.ToString(), out boolValue))
            {
                boolValue = false;
            }

            string property = boolValue ? "PhoneAccentColor" : "PhoneForegroundColor";
            Logger.Debug("Converted {0} to {1}", value, property);
            Color color = (Color) Application.Current.Resources[property];
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}