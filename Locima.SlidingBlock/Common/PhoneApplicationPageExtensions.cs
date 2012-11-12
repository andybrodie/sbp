using System;
using System.Net;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using NLog;

namespace Locima.SlidingBlock.Common
{
    public static class PhoneApplicationPageExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static T GetQueryParameter<T>(this PhoneApplicationPage page, string parameterName,
                                             Func<string, T> parser)
        {
            string queryParamValue;
            T result;
            if (page.NavigationContext.QueryString.TryGetValue(parameterName, out queryParamValue))
            {
                queryParamValue = HttpUtility.UrlDecode(queryParamValue);
                result = parser(queryParamValue);
            }
            else
            {
                Logger.Error("{0} No value specified for {1}", page, parameterName);
                return default(T);
            }
            return result;
        }

        public static int GetQueryParameterAsInt(this PhoneApplicationPage page, string parameterName)
        {
            return GetQueryParameter(page, parameterName, delegate(string s)
                                                              {
                                                                  int i;
                                                                  if (!int.TryParse(s, out i))
                                                                  {
                                                                      i = default(int);
                                                                      Logger.Error("Unable to parse {0} to an integer",s);
                                                                  }
                                                                  return i;
                                                              });
        }

        /// <summary>
        ///   Launches a chooser task, suppressing the <see cref="InvalidOperationException" /> thrown if the chooser is launched twice (where the user double-taps the icon to choose)
        /// </summary>
        /// <param name="page">The page to launch the chooser from</param>
        /// <param name="chooser">Th chooser to launch</param>
        public static void LaunchChooserSafely<TTaskEventArgs>(this PhoneApplicationPage page, ChooserBase<TTaskEventArgs> chooser)
            where TTaskEventArgs : TaskEventArgs
        {
            try
            {
                chooser.Show();
            }
            catch (InvalidOperationException)
            {
                Logger.Debug(
                    "Supressed InvalidOperationException caused by user double-tapping the control that launched the {0} chooser", chooser);
            }
        }



    }
}