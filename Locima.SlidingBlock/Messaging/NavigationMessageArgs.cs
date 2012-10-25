using System;
using System.Windows.Navigation;

namespace Locima.SlidingBlock.Messaging
{

    /// <summary>
    /// Represents arguments to a message to the view requesting that a new page is navigated to
    /// </summary>
    public class NavigationMessageArgs : MessageArgs
    {
        /// <summary>
        /// No-op
        /// </summary>
        public NavigationMessageArgs()
        {
        }

        /// <summary>
        /// Sets <see cref="Uri"/>
        /// </summary>
        /// <param name="uri"></param>
        public NavigationMessageArgs(Uri uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// Sets <see cref="Uri"/> and sets the <see cref="UriKind"/> to <see cref="UriKind.Relative"/>
        /// </summary>
        /// <param name="relativeUri"></param>
        public NavigationMessageArgs(string relativeUri)
        {
            Uri = new Uri(relativeUri, UriKind.Relative);
        }

        /// <summary>
        /// The navigation mode.  Only the <c>New</c> or <c>Back</c> are supported typically
        /// </summary>
        public NavigationMode NavigationMode { get; set; }

        /// <summary>
        /// The page to navigate to
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Convenience instance that requests a navigation back
        /// </summary>
        public static NavigationMessageArgs Back
        {
            get { return new NavigationMessageArgs {NavigationMode = NavigationMode.Back}; }
        }


        /// <summary>
        /// Just adds the Uri to the default implementation
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}({1})", base.ToString(), Uri);
        }
    }
}