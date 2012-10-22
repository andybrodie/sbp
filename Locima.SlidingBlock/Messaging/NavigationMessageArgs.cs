using System;
using System.Windows.Navigation;

namespace Locima.SlidingBlock.Messaging
{
    public class NavigationMessageArgs : MessageArgs
    {

        public NavigationMessageArgs()
        {
        }

        public NavigationMessageArgs(Uri uri)
        {
            Uri = uri;
        }

        public NavigationMessageArgs(string relativeUri)
        {
            Uri = new Uri(relativeUri, UriKind.Relative);
        }

        public NavigationMode NavigationMode { get; set; }
        public Uri Uri { get; set; }

        public static NavigationMessageArgs Back
        {
            get { return new NavigationMessageArgs {NavigationMode = NavigationMode.Back}; }
        }


        public override string ToString()
        {
            return string.Format("{0}({1})", base.ToString(), Uri);
        }
    }
}