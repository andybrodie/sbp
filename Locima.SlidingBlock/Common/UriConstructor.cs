using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NLog;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Helper class for constructing <see cref="Uri"/> instances.
    /// </summary>
    public class UriConstructor
    {
        private readonly Dictionary<string, string> _parameters = new Dictionary<string,string>();
        private readonly string _baseUri;
        private readonly UriKind _uriKind;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Create a new instance using the parameters specified.
        /// </summary>
        /// <param name="baseUri">The base Uri (no query string), must not be null or zero length</param>
        /// <param name="uriKind">The kind of Uri to construct</param>
        public UriConstructor(string baseUri, UriKind uriKind)
        {
            if (string.IsNullOrEmpty(baseUri)) throw new ArgumentNullException("baseUri");
            _baseUri = baseUri;
            _uriKind = uriKind;
        }


        /// <summary>
        /// Adds a new parameter to the query string of the <see cref="Uri"/>
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The value of the parameter</param>
        public void AddParameter(string parameterName, object parameterValue)
        {
            if (parameterValue != null)
            {
                _parameters.Add(parameterName, parameterValue.ToString());
            }
        }


        /// <summary>
        /// Converts to a <see cref="Uri"/> instance
        /// </summary>
        /// <returns>A <see cref="Uri"/> based on the values passed to the constructor and <see cref="AddParameter"/></returns>
        public Uri ToUri()
        {
            StringBuilder builder = new StringBuilder(_baseUri);
            if (_parameters.Count > 0)
            {
                bool firstParameter = true;
                foreach (KeyValuePair<string, string> parameter in _parameters)
                {
                    builder.Append(firstParameter ? '?' : '&');
                    firstParameter = false;
                    builder.AppendFormat("{0}={1}", HttpUtility.UrlEncode(parameter.Key),
                                         HttpUtility.UrlEncode(parameter.Value));

                }
            }
            string uriString = builder.ToString();
            Logger.Debug("Returning {0} URI {1}", _uriKind, uriString);
            return new Uri(uriString, _uriKind);
        }
    }
}