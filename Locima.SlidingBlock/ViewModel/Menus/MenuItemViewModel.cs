using System;
using System.Windows.Media.Imaging;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Model;
using NLog;

namespace Locima.SlidingBlock.ViewModel.Menus
{
    /// <summary>
    ///   View model to represent a single item of a <see cref="MenuPageViewModel" />.
    /// </summary>
    /// <remarks>
    ///   <para> This class does not perform any explicit localization, the caller is responsible for this. </para>
    ///   <para> To understand the relationship between <see cref="SelectedAction" /> , <see cref="TargetPage" /> and <see
    ///    cref="TargetUri" /> see <see cref="Invoke" /> </para>
    /// </remarks>
    public class MenuItemViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="Icon"/>
        /// </summary>
        private WriteableBitmap _icon;

        /// <summary>
        /// Backing field for <see cref="IsEnabled"/>
        /// </summary>
        private bool _isEnabled = true;

        /// <summary>
        /// Backing field for <see cref="TargetPage"/>
        /// </summary>
        private string _targetPage;

        /// <summary>
        /// Backing field for <see cref="TargetUri"/>
        /// </summary>
        private Uri _targetUri;

        /// <summary>
        /// Backing field for <see cref="Text"/>
        /// </summary>
        private string _text;

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
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
                OnNotifyPropertyChanged("Title");
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
                OnNotifyPropertyChanged("Text");
            }
        }


        /// <summary>
        ///   The <see cref="Uri" /> to navigate to when the menu item is selected
        /// </summary>
        /// <remarks>
        /// <para>See <see cref="Invoke"/> to understand the precedence of <see cref="SelectedAction"/>, 
        /// <see cref="TargetPage"/> and <see cref="TargetUri"/></para>
        /// </remarks>
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
        /// <remarks>
        /// <para>See <see cref="Invoke"/> to understand the precedence of <see cref="SelectedAction"/>, 
        /// <see cref="TargetPage"/> and <see cref="TargetUri"/></para>
        /// </remarks>
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
        /// Retrieve the <see cref="Uri"/> to navigate to when selected
        /// </summary>
        /// <remarks>
        /// Useful when the generation of the <see cref="Uri"/> is computationally expensive, or has side effects that you only want to occur when the user selects the action (e.g. creating a new game)
        /// <para>See <see cref="Invoke"/> to understand the precedence of <see cref="SelectedAction"/>, 
        /// <see cref="TargetPage"/> and <see cref="TargetUri"/></para>
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

        /// <summary>
        /// The icon to display next to this menu item
        /// </summary>
        public WriteableBitmap Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnNotifyPropertyChanged("Icon");
            }
        }

        /// <summary>
        /// Re-usable defined function for <see cref="IsEnabled"/> that always returns <c>false</c>
        /// </summary>
        public static readonly Func<bool> AlwaysDisabled = () => false;

        /// <summary>
        /// Re-usable defined function for <see cref="IsEnabled"/> that always returns <c>true</c>
        /// </summary>
        public static readonly Func<bool> AlwaysEnabled = () => true;


        /// <summary>
        /// Initialises this view model with the model passed in <paramref name="menuItemModel"/>
        /// </summary>
        /// <param name="menuItemModel">The model to use for this viewmodel</param>
        public MenuItemViewModel(MenuItemModel menuItemModel)
        {
            Icon = menuItemModel.Icon;
            TargetPage = menuItemModel.TargetPage;
            TargetUri = menuItemModel.TargetUri;
            SelectedAction = menuItemModel.SelectedAction;
            Text = menuItemModel.Text;
            Title = menuItemModel.Title;
        }

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
                Logger.Debug("Invoking SelectedAction on menu item {0}", this);
                navUri = SelectedAction();
                Logger.Debug("Invoked SelectedAction on menu item which returned Uri {0}", navUri);
            }
            // No SelectedAction has been set, so look at TargetUri
            else if (TargetUri != null)
            {
                Logger.Debug("Returning TargetUri {0}", TargetUri);
                navUri = TargetUri;
            }
            // No SelectedAction or TargetUri so must be TargetPage
            else if (!string.IsNullOrEmpty(TargetPage))
            {
                Logger.Debug("Returning TargetPage \"{0}\"", TargetPage);
                navUri = MainPage.CreateNavigationUri(TargetPage);
            }
            // No SelectedAction, TargetUri or TargetPage, so this is a bug
            else
            {
                throw new InvalidStateException("MenuPage bug, {0} has no SelectedAction, TargetUri or TargetPage set");
            }

            // If navUri has been set by SelectedAction, then follow that, if not look in TargetUri and TargetPage

            Logger.Debug("Menu item {0}(Title=\"{1}\",Text=\"{2}\") returning Uri {3}", this, Title, Text,
                         navUri == null ? "null" : navUri.ToString());
            return navUri;
        }
    }
}