using System;
using System.Collections.Generic;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock.Common
{

    /// <summary>
    /// Helper methods for managing <see cref="PhoneApplicationService.State"/>
    /// </summary>
    public class StateManagementHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a key to use with <see cref="PhoneApplicationService.State"/> that is specific to this page (prefixes with the type name of the page and an underscore)
        /// </summary>
        /// <param name="owner">The owner of the property</param>
        /// <param name="key">The name of the property to generate a key for</param>
        /// <returns>A string key to use wih <see cref="PhoneApplicationService.State"/></returns>
        public static string CreateKey(string owner, string key)
        {
            return string.Format("{0}_{1}", owner, key);
        }


        /// <summary>
        /// Sets the property in <see cref="PhoneApplicationService.State"/> with the name
        /// </summary>
        /// <param name="owner">The owner of the property</param>
        /// <param name="propertyName">The name of the property to retrieve (note this isn't the property name as stored in <see cref="PhoneApplicationService.State"/>, see <see cref="CreateKey"/>)</param>
        /// <param name="propertyValue">The value to set.</param>
        public static void SetState(string owner, string propertyName, object propertyValue)
        {
            string key = CreateKey(owner, propertyName);
            PhoneApplicationService.Current.State[key] = propertyValue;
            Logger.Debug("Saved property \"{0}\"=\"{1}\"", key, propertyValue);
        }


        /// <summary>
        /// Removes a <see cref="KeyValuePair{TKey,TValue}"/> from the <see cref="PhoneApplicationService.State"/> dictionary.
        /// </summary>
        /// <param name="owner">The owner of the property</param>
        /// <param name="propertyName">The name of the property to remove</param>
        /// <returns><c>true</c> if an object was found to delete, <c>false</c> otherwise</returns>
        public static bool ClearState(string owner, string propertyName)
        {
            bool result;
            string key = CreateKey(owner, propertyName);
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
        /// <param name="owner">The owner of the property</param>
        public static void ClearState(string owner)
        {
            string ownedKeysPrefix = owner + "_";
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
        /// <param name="owner">The owner of the property</param>
        /// <param name="propertyName">The name of the property to retrieve (note this isn't the property name as stored in <see cref="PhoneApplicationService.State"/>, see <see cref="CreateKey"/>)</param>
        /// <param name="outputValue">The value to populate with the property.  If the property doesn't exist or is of the wrong type then <c>default(<typeparamref name="T"/>) will be set</c>.</param>
        /// <returns><c>true</c> if a property value was successfully retrieved and <paramref name="outputValue"/> has been set successfully, <c>false</c> otherwise</returns>
        public static bool TryGetState<T>(string owner, string propertyName, out T outputValue)
        {
            object stateObject;
            string key = CreateKey(owner, propertyName);
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
                            key, typeof (T), stateObject.GetType().Name);
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