using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    ///   Represents a single item of a <see cref="MenuPageViewModel" />.
    /// </summary>
    /// <remarks>
    ///   <para> This class does not perform any explicit localization, the caller is responsible for this. </para>
    ///   <para> To understand the relationship between <see cref="SelectedAction" /> , <see cref="TargetPage" /> and <see
    ///    cref="TargetUri" /> see <see cref="Invoke" /> </para>
    /// </remarks>
    public class MenuItemViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private WriteableBitmap _icon;
        private bool _isEnabled = true;
        private string _targetPage;
        private Uri _targetUri;
        private string _text;

        private string _title;

        /// <summary>
        ///   The title of the menu item
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnNotifyPropertyChanged("ImageTitle");
            }
        }

        /// <summary>
        ///   The text typically beneath a menu item explaining in a few words what the menu item does.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnNotifyPropertyChanged("ImageText");
            }
        }


        /// <summary>
        ///   The <see cref="Uri" /> to navigate to when the menu item is selected
        /// </summary>
        public Uri TargetUri
        {
            get { return _targetUri; }
            set
            {
                if (_targetUri != value)
                {
                    if (value != null) TargetPage = null;
                    _targetUri = value;
                    OnNotifyPropertyChanged("TargetUri");
                }
            }
        }

        /// <summary>
        ///   The name of another menu page to navigate to when ths menu item is selected.
        /// </summary>
        public string TargetPage
        {
            get { return _targetPage; }
            set
            {
                if (_targetPage != value)
                {
                    if (value != null) TargetUri = null;
                    _targetPage = value;
                    OnNotifyPropertyChanged("TargetPage");
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        ///   This doesn't need to notify it has no impact on the GUI
        /// </remarks>
        public Func<Uri> SelectedAction { get; set; }

        /// <summary>
        ///   Returns true if the menu item should be enabled, false otherwise
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnNotifyPropertyChanged("IsEnabled");
            }
        }

        public WriteableBitmap Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnNotifyPropertyChanged("Icon");
            }
        }

        public static readonly Func<bool> AlwaysDisabled = () => false;
        public static readonly Func<bool> AlwaysEnabled = () => true;

        /// <summary>
        ///   Invokes the correct action for this menu item and returns a <see cref="Uri" /> to navigate to or <c>null</c> if no navigation should occur.
        /// </summary>
        /// <remarks>
        ///   <para> There are three different ways in which the result of clicking on a menu item can be actioned. </para>
        ///   <para> If <see cref="SelectedAction" /> is set then this is always invoked. If this method returns a <see cref="Uri" /> object then this is navigated to. If <c>null</c> is returned then if <see
        ///    cref="TargetUri" /> is specified then this is navigated to. If <see cref="TargetUri" /> is <c>null</c> then <see
        ///    cref="TargetPage" /> (a named menu page) is navigated to. </para>
        ///   <para> If <see cref="SelectedAction" /> returns <c>null</c> and both <see cref="TargetUri" /> and <see
        ///    cref="TargetPage" /> are both <c>null</c> then an <see cref="InvalidStateException" /> is thrown. </para>
        ///   <para> This logic is encapsulated in <see cref="Invoke" /> . </para>
        ///   <para> This class does not perform any explicit localization, the caller is responsible for this. </para>
        /// </remarks>
        /// <returns> A <see cref="Uri" /> that can be navigated to or <c>null</c> . </returns>
        public Uri Invoke()
        {
            Uri navUri;

            if (SelectedAction != null)
            {
                Logger.Debug("Invoking SelectedAction on menu item");
                navUri = SelectedAction();
                Logger.Debug("Invoked SelectedAction on menu item which returned Uri {0}", navUri);
            }
            else
            {
                navUri = null;
            }

            // If navUri has been set by SelectedAction, then follow that, if not look in TargetUri and TargetPage

            if (navUri == null)
            {
                if (TargetUri != null)
                {
                    Logger.Debug("Returning TargetUri {0}", TargetUri);
                    navUri = TargetUri;
                }
                else if (!string.IsNullOrEmpty(TargetPage))
                {
                    string currentUrl = ((App) Application.Current).RootFrame.CurrentSource.ToString();
                    if (currentUrl.Contains("?"))
                    {
                        currentUrl = currentUrl.Substring(currentUrl.IndexOf("?", StringComparison.Ordinal));
                    }
                    navUri = new Uri(string.Format("{0}?menuPage={1}", currentUrl, TargetPage),
                                     UriKind.Relative);
                    Logger.Debug("Returning TargetPage \"{0}\"", TargetPage);
                }
            }
            Logger.Debug("Menu item {0}(ImageTitle=\"{1}\",ImageText=\"{2}\") returning Uri {3}", this, Title, Text,
                         navUri == null ? "null" : navUri.ToString());
            return navUri;
        }
    }
}