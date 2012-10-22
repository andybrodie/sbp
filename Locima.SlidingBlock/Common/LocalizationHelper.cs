using System;
using System.Globalization;
using System.Resources;
using Locima.SlidingBlock.Resources;
using NLog;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Helper class for localising different parts of the application.
    /// </summary>
    /// <remarks>
    ///   Consists of <see cref="Strings" /> for use within XAML and <see cref="GetString" /> for use by code
    /// </remarks>
    public class LocalizationHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly AppResources _strings = new AppResources();

        private static readonly String[] OrdinalSuffixes = new[]
            {"th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th"};

        /// <summary>
        ///   Reference to <see cref="AppResources" /> used by XAML code for localisation
        /// </summary>
        public static AppResources Strings
        {
            get { return _strings; }
        }

        /// <summary>
        ///   Returns the translation of the <paramref name="label" /> passed using the <see cref="CultureInfo.CurrentUICulture" />, applying any parameters passed to the localized
        ///   string using <see cref="string.Format(string,object)" /> syntax.
        /// </summary>
        /// <remarks>
        ///   Over and above the default behaviour of <see cref="ResourceManager.GetString(string)" /> this method provides parameter substitution and default replacement text if
        ///   the string was not found (along with a warning message on the log)
        /// </remarks>
        /// <param name="label"> The label to search for in the <see cref="AppResources.ResourceManager" /> . Must not be null. </param>
        /// <param name="args"> The arguments to replace in the string, may be null. </param>
        /// <returns> A translated, parameter-filled string. </returns>
        /// <exception cref="ArgumentNullException">if
        ///   <paramref name="label" />
        ///   is null.</exception>
        public static string GetString(string label, params object[] args)
        {
            if (label == null) throw new ArgumentNullException("label");
            string localizedName = AppResources.ResourceManager.GetString(label);
            if (localizedName == null)
            {
                Logger.Warn(
                    "Could not resolve \"{0}\" to a string in the resource culture for {1}.  Returning \"{0}\".",
                    label, CultureInfo.CurrentUICulture);
                localizedName = label;
            }
            else if (args != null && args.Length > 0)
            {
                localizedName = string.Format(localizedName, args);
            }
            return localizedName;
        }


        /// <summary>
        ///   Converts the <paramref name="timeSpan" /> to a nice localized string
        /// </summary>
        /// <remarks>
        /// <para>We need to express time as a localized string.  E.g. "1 hour, 20 minutes and 30 seconds"</para>
        /// <para>Problems are:</para>
        /// <list type="number">
        /// <item><description>The string format might be totally different</description></item>
        /// <item><description>The English (At least) uses plurals for values greater than 1 on any single digit.</description></item>
        /// </list>
        /// <para>So the word "hour", "minute" and "second" has to be replacable with their plural counterparts
        /// But, we can't be too specific, or languages which don't have the same structure for the sentence won't be localizable easily.</para>
        /// Decided to do a <see cref="CultureInfo.CurrentUICulture"/> check and just leave it as an excercise to the programmer, defaulting to HH:MM:SS.FF, localized, of course.
        /// </remarks>
        /// <param name="timeSpan">What will be converted to a nice string</param>
        /// <returns>A nice string (well, if you're in an "en" locale, anyway...</returns>
        public static string GetTimeSpanString(TimeSpan timeSpan)
        {
            string timeString;
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en")
            {
                // First, create some short variables for the different components of the time
                int h = (int) Math.Floor(timeSpan.TotalHours);
                int m = timeSpan.Minutes;
                int s = timeSpan.Seconds;
                int ms = timeSpan.Milliseconds;

                // Now calculate the components of the strings, one for hours, one for minutes and one combined for seconds and milliseconds
                string hoursComponent = h == 0 ? String.Empty : (h == 1 ? "1 hour" : h + " hours");
                string minutesComponent = m == 0 ? String.Empty : (m == 1 ? "1 minute" : m + " minutes");
                string secondsComponent;
                if (s == 0 && ms == 0)
                {
                    secondsComponent = String.Empty;
                }
                else if (s == 1 && ms == 0)
                {
                    secondsComponent = "1 second";
                }
                else
                {
                    secondsComponent = ms == 0
                                           ? String.Format("{0} seconds", s)
                                           : String.Format("{0}.{1} seconds", s, ms);
                }


                // Now create a binary flags representation of which of the three components have text
                int i = (String.IsNullOrEmpty(hoursComponent) ? 0 : 4)
                        | (String.IsNullOrEmpty(minutesComponent) ? 0 : 2)
                        | (String.IsNullOrEmpty(secondsComponent) ? 0 : 1);

                // Finally, create a string for each combination (there's 8 in total)
                switch (i)
                {
                    case 0: // 000
                        timeString = "no time at all!";
                        break;
                    case 1: // 001 
                        timeString = secondsComponent;
                        break;
                    case 2: // 010
                        timeString = minutesComponent;
                        break;
                    case 3: // 011
                        timeString = String.Format("{0} and {1}", minutesComponent, secondsComponent);
                        break;
                    case 4: // 100:
                        timeString = hoursComponent;
                        break;
                    case 5: // 101
                        timeString = String.Format("{0} and {1}", hoursComponent, secondsComponent);
                        break;
                    case 6: // 110:
                        timeString = String.Format("{0} and {1}", hoursComponent, minutesComponent);
                        break;
                    case 7: // 111
                        timeString = String.Format("{0}, {1} and {2}", hoursComponent, minutesComponent,
                                                   secondsComponent);
                        break;
                    default:
                        timeString = "BUG";
                        break;
                }
            }
            else
            {
                // Sorry, all other languages just get a default localized serialisation
                timeString = timeSpan.ToString("c");
            }
            return timeString;
        }


        /// <summary>
        ///   Returns the ordinal representation of a number.  Ordinals are where (in English) you have st
        /// </summary>
        /// <remarks>
        ///   Credit: http://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c"
        /// </remarks>
        /// <param name="number">The number to create an ordinal version of</param>
        /// <returns>an ordinal version of a number (e.g. 1 -> 1st, 2 -> 2nd, 3544 -> 3544th)</returns>
        public static string GetOrdinal(int number)
        {
            string ordinal;
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en")
            {
                int n = Math.Abs(number);
                int lastTwoDigits = n % 100;
                int lastDigit = n % 10;
                int index = (lastTwoDigits >= 11 && lastTwoDigits <= 13) ? 0 : lastDigit;
                ordinal = number + OrdinalSuffixes[index];
            }
            else
            {
                ordinal = number.ToString(CultureInfo.CurrentUICulture);
            }
            return ordinal;
        }
    }
}