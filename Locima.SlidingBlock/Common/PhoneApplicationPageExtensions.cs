using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using NLog;

namespace Locima.SlidingBlock.Common
{

    /// <summary>
    /// Extension methods on the <see cref="PhoneApplicationPage"/> class
    /// </summary>
    public static class PhoneApplicationPageExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Retrieves a query parameter from the Uri used to launch this page
        /// </summary>
        /// <remarks>
        /// This handles error conditions and the call to <see cref="HttpUtility.UrlDecode"/></remarks>
        /// <param name="page">The page instance that we'll use the <see cref="Page.NavigationContext"/> from.</param>
        /// <param name="queryParamName">The query parameter name to retrieve</param>
        /// <returns>The value of the query parameter</returns>
        public static string GetQueryParameter(this PhoneApplicationPage page, string queryParamName)
        {
            return GetQueryParameter(page, queryParamName, s => s);
        }


        /// <summary>
        /// Retrieves a query parameter from the Uri used to launch this page and converts to an object/value type of type <typeparamref name="T"/>
        /// </summary>
        /// <remarks>
        /// This handles error conditions and the call to <see cref="HttpUtility.UrlDecode"/></remarks>
        /// <typeparam name="T">The type that the query parameter <paramref name="queryParamName"/> should be converted to by <paramref name="parser"/></typeparam>
        /// <param name="page">The page instance that we'll use the <see cref="Page.NavigationContext"/> from.</param>
        /// <param name="queryParamName">The query parameter name to retrieve</param>
        /// <param name="parser">A function that can convert the string value returned to the desired type <typeparamref name="T"/></param>
        /// <returns>The value of the query parameter</returns>
        public static T GetQueryParameter<T>(this PhoneApplicationPage page, string queryParamName,
                                             Func<string, T> parser)
        {
            string queryParamValue;
            T result;
            if (page.NavigationContext.QueryString.TryGetValue(queryParamName, out queryParamValue))
            {
                queryParamValue = HttpUtility.UrlDecode(queryParamValue);
                result = parser(queryParamValue);
            }
            else
            {
                Logger.Info("On {0}, found no value specified for query parameter {1}", page, queryParamName);
                return default(T);
            }
            return result;
        }


        /// <summary>
        /// Retrieves a query parameter from the Uri used to launch this page and converts to an <see cref="Enum"/> of type <typeparamref name="T"/>
        /// </summary>
        /// <remarks>
        /// This handles error conditions (returns null on all error cases and logs the error) and the call to <see cref="HttpUtility.UrlDecode"/></remarks>
        /// <typeparam name="T">The enum that the query parameter <paramref name="queryParamName"/> should be converted to</typeparam>
        /// <param name="page">The page instance that we'll use the <see cref="Page.NavigationContext"/> from.</param>
        /// <param name="queryParamName">The query parameter name to retrieve</param>
        /// <returns>The nullable enum value of the parameter</returns>
        public static T? GetQueryParameterAsEnum<T>(this PhoneApplicationPage page, string queryParamName)
            where T : struct
        {
            T? enumValue = GetQueryParameter(page, queryParamName, s =>
                {
                    T? parsedEnumValue;
                    if (string.IsNullOrEmpty(s))
                    {
                        parsedEnumValue = null;
                    }
                    else
                    {
                        try
                        {
                            parsedEnumValue = (T) Enum.Parse(typeof (T), s, true);
                        }
                        catch (ArgumentException ae)
                        {
                            Logger.ErrorException(
                                string.Format(
                                    "Unable to convert {0} to an enum of type {1} as an argument exception was thrown",
                                    s, typeof (T).FullName), ae);
                            parsedEnumValue = null;
                        }
                        catch (OverflowException oe)
                        {
                            Logger.ErrorException(
                                string.Format(
                                    "Unable to convert {0} to an enum of type {1} as an overflow exception was thrown",
                                    s, typeof (T).FullName), oe);
                            parsedEnumValue = null;
                        }
                    }
                    return parsedEnumValue;
                });
            return enumValue;
        }


        /// <summary>
        /// Uses <see cref="GetQueryParameter{T}"/> to retrieve the query parameter specific as an <see cref="int"/>
        /// </summary>
        /// <param name="page">The page instance that we'll use the <see cref="Page.NavigationContext"/> from.</param>
        /// <param name="queryParamName">The query parameter name to retrieve</param>
        /// <returns>The value parsed by <see cref="int.TryParse(string,out int)"/> or <c>default(int)</c> if not a valid integer</returns>/returns>
        public static int GetQueryParameterAsInt(this PhoneApplicationPage page, string queryParamName)
        {
            return GetQueryParameter(page, queryParamName, delegate(string s)
                                                              {
                                                                  int i;
                                                                  if (!int.TryParse(s, out i))
                                                                  {
                                                                      i = default(int);
                                                                      Logger.Error("Unable to parse {0} to an integer",
                                                                                   s);
                                                                  }
                                                                  return i;
                                                              });
        }

        /// <summary>
        ///   Launches a chooser task, suppressing the <see cref="InvalidOperationException" /> thrown if the chooser is launched twice (where the user double-taps the icon to choose)
        /// </summary>
        /// <param name="page">The page to launch the chooser from</param>
        /// <param name="chooser">Th chooser to launch</param>
        public static void LaunchChooserSafely<TTaskEventArgs>(this PhoneApplicationPage page,
                                                               ChooserBase<TTaskEventArgs> chooser)
            where TTaskEventArgs : TaskEventArgs
        {
            try
            {
                chooser.Show();
            }
            catch (InvalidOperationException)
            {
                Logger.Debug(
                    "Supressed InvalidOperationException caused by user double-tapping the control that launched the {0} chooser",
                    chooser);
            }
        }

        /// <summary>
        /// Navigates back <paramref name="skip"/> pages in to the backstack
        /// </summary>
        /// <param name="page">TThe page this extension method is attached to</param>
        /// <param name="skip">The number of backstack entries to skip (0 means none)</param>
        public static void GoBack(this PhoneApplicationPage page, int skip)
        {
            Logger.Info("Removing {0} entries from the backstack", skip);
            while (skip-- > 0)
            {
                JournalEntry removedEntry = page.NavigationService.RemoveBackEntry();
                Logger.Debug("Removed {0} from the backstack", removedEntry.Source);
            }
            Logger.Info("Navigating back");
            page.Dispatcher.BeginInvoke(() => page.NavigationService.GoBack());
        }


        /// <summary>
        /// Sets the property in <see cref="PhoneApplicationService.State"/> with the name
        /// </summary>
        /// <param name="page">The page that this property key is associated with</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <param name="propertyValue">The value to set.</param>
        public static void SetState(this PhoneApplicationPage page, string propertyName, object propertyValue)
        {
            StateManagementHelper.SetState(page.GetType().Name, propertyName, propertyValue);
        }


        /// <summary>
        /// Removes a <see cref="KeyValuePair{TKey,TValue}"/> from the <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <param name="page">The page that this property is associated with</param>
        /// <param name="propertyName">The name of the property to remove</param>
        /// <returns><c>true</c> if an object was found to delete, <c>false</c> otherwise</returns>
        public static bool ClearState(this PhoneApplicationPage page, string propertyName)
        {
            return StateManagementHelper.ClearState(page.GetType().Name, propertyName);
        }


        /// <summary>
        /// Remove all keys from the <see cref="PhoneApplicationService.State"/> dictionary set by this page
        /// </summary>
        /// <param name="page">The page this property is associated with</param>
        public static void ClearState(this PhoneApplicationPage page)
        {
            StateManagementHelper.ClearState(page.GetType().Name);
        }

        /// <summary>
        /// Attempts to retrieve a property from the <see cref="PhoneApplicationService.Current"/>'s <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the output parameter</typeparam>
        /// <param name="page">The phone application page to store the property for</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <param name="outputValue">The value to populate with the property.  If the property doesn't exist or is of the wrong type then <c>default(<typeparamref name="T"/>) will be set</c>.</param>
        /// <returns><c>true</c> if a property value was successfully retrieved and <paramref name="outputValue"/> has been set successfully, <c>false</c> otherwise</returns>
        public static bool TryGetState<T>(this PhoneApplicationPage page, string propertyName, out T outputValue)
        {
            return StateManagementHelper.TryGetState(page.GetType().Name, propertyName, out outputValue);
        }


    }
}