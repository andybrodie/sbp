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
        /// Creates a key to use with <see cref="PhoneApplicationService.State"/> that is specific to this page (prefixes with the type name of the page and an underscore)
        /// </summary>
        /// <param name="page">The page that this property key is associated with</param>
        /// <param name="key">The name of the property to generate a key for</param>
        /// <returns>A string key to use wih <see cref="PhoneApplicationService.State"/></returns>
        private static string CreateKey(PhoneApplicationPage page, string key)
        {
            return string.Format("{0}_{1}", page.GetType().Name, key);
        }



        /// <summary>
        /// Sets the property in <see cref="PhoneApplicationService.State"/> with the name
        /// </summary>
        /// <param name="page">The page that this property key is associated with</param>
        /// <param name="propertyName">The name of the property to retrieve (note this isn't the property name as stored in <see cref="PhoneApplicationService.State"/>, see <see cref="CreateKey"/>)</param>
        /// <param name="propertyValue">The value to set.</param>
        public static void SetState(this PhoneApplicationPage page, string propertyName, object propertyValue)
        {
            string key = CreateKey(page, propertyName);
            PhoneApplicationService.Current.State[key] = propertyValue;
            Logger.Debug("Saved property \"{0}\"=\"{1}\"", key, propertyValue);
        }


        /// <summary>
        /// Removes a <see cref="KeyValuePair{TKey,TValue}"/> from the <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <param name="page">The page that this property is associated with</param>
        /// <param name="propertyName">The name of the property to remove</param>
        /// <returns><c>true</c> if an object was found to delete, <c>false</c> otherwise</returns>
        public static bool ClearState(this PhoneApplicationPage page, string propertyName)
        {
            bool result;
            string key = CreateKey(page, propertyName);
            if (PhoneApplicationService.Current.State.ContainsKey(key))
            {
                Logger.Debug("Clearing property \"{0}\"", key);
                PhoneApplicationService.Current.State.Remove(key);
                result = true;
            }
            else
            {
                Logger.Warn("Could not find key \"{0}\" when asked to clear it", key);
                result = false;
            }
            return result;
        }


        /// <summary>
        /// Remove all keys from the <see cref="PhoneApplicationService.State"/> dictionary set by this page
        /// </summary>
        /// <param name="page">The page this property is associated with</param>
        public static void ClearState(this PhoneApplicationPage page)
        {
            string ownedKeysPrefix = page.GetType().Name + "_";
            foreach (KeyValuePair<string, object> keyPair in PhoneApplicationService.Current.State)
            {
                if (keyPair.Key.StartsWith(ownedKeysPrefix))
                {
                    Logger.Info("Removing \"{0}\"=\"{1}\"", keyPair.Key, keyPair.Value);
                    PhoneApplicationService.Current.State.Remove(keyPair);
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve a property from the <see cref="PhoneApplicationService.Current"/>'s <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the output parameter</typeparam>
        /// <param name="page">The phone application page to store the property for</param>
        /// <param name="propertyName">The name of the property to retrieve (note this isn't the property name as stored in <see cref="PhoneApplicationService.State"/>, see <see cref="CreateKey"/>)</param>
        /// <param name="outputValue">The value to populate with the property.  If the property doesn't exist or is of the wrong type then <c>default(<typeparamref name="T"/>) will be set</c>.</param>
        /// <returns><c>true</c> if a property value was successfully retrieved and <paramref name="outputValue"/> has been set successfully, <c>false</c> otherwise</returns>
        public static bool TryGetState<T>(this PhoneApplicationPage page, string propertyName, out T outputValue)
        {
            object stateObject;
            string key = CreateKey(page, propertyName);
            bool result = PhoneApplicationService.Current.State.TryGetValue(key, out stateObject);
            if (result)
            {
                // If null has been explicitly stored, then I won't be able to cast this to a value type (e.g. int, struct), therefore manually set this to the default value
                if (stateObject == null)
                {
                    outputValue = default(T);
                }
                else
                {
                    try
                    {
                        outputValue = (T) stateObject;
                    }
                    catch (InvalidCastException)
                    {
                        // If you can't cast because the object stored in the property is incompatible, then behave as if the the property didn't exist
                        result = false;
                        Logger.Error(
                            "Attempt to retrieve {0} from PhoneApplicationService.Current.State as {1} but object was {2}",
                            key, typeof(T), stateObject.GetType().Name);
                        outputValue = default(T);
                    }
                }
            }
            else
            {
                outputValue = default(T);
            }
            return result;
        }


    }
}