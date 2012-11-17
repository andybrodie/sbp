using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Navigation;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The view model for a single menu page, consisting of just a title and a collection of view models (<see cref="MenuItemViewModel"/>) for each menu item
    /// </summary>
    public class MenuPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="MenuItems"/>
        /// </summary>
        private ObservableCollection<MenuItemViewModel> _menuItems;

        /// <summary>
        /// Backing field for <see cref="PageTitle"/>
        /// </summary>
        private string _pageTitle;

        /// <summary>
        /// The title of the menu page
        /// </summary>
        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnNotifyPropertyChanged("PageTitle");
            }
        }

        /// <summary>
        /// The list of view models for menu items to be shown on the menu page
        /// </summary>
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get { return _menuItems; }
            set
            {
                if (_menuItems != null)
                {
                    _menuItems.CollectionChanged -= MenuItemsOnCollectionChanged;
                }
                _menuItems = value;
                if (_menuItems != null)
                {
                    _menuItems.CollectionChanged += MenuItemsOnCollectionChanged;
                }
            }
        }

        /// <summary>
        /// The back stack, as obtained from <see cref="NavigationService.BackStack"/>
        /// </summary>
        public IEnumerable<JournalEntry> BackStack { get; set; }

        /// <summary>
        /// Raised when a new menu item is added or an existing one removed from <see cref="MenuItems"/>
        /// </summary>
        private void MenuItemsOnCollectionChanged(object sender,
                                                  NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnNotifyPropertyChanged("MenuItems");
        }
    }
}