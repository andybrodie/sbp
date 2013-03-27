using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Navigation;
using Locima.SlidingBlock.IO;
using Locima.SlidingBlock.Model;

namespace Locima.SlidingBlock.ViewModel.Menus
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
        /// Backing field for <see cref="ActivePlayerName"/>
        /// </summary>
        private string _activePlayerName;

        /// <summary>
        /// Default constructor used in page creation
        /// </summary>
        public MenuPageViewModel()
        {
            
        }

       
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
        /// A note on the UI on who the active player is
        /// </summary>
        public string ActivePlayerName
        {
            get { return _activePlayerName; }
            set
            {
                _activePlayerName = value;
                OnNotifyPropertyChanged("ActivePlayerName");
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
        /// Initialise with the named menu page model
        /// </summary>
        /// <param name="menuPageName">The name of the menu page (see <see cref="Menus"/>)</param>
        /// <param name="backStack">The current backstack, as obtined from <see cref="NavigationService.BackStack"/></param>
        public void Initialise(string menuPageName, IEnumerable<JournalEntry> backStack)
        {
            MenuPageModel menuPageModel = MenuPageBroker.RetrieveMenuPage(menuPageName, backStack);
            Initialise(menuPageModel);
        }


        /// <summary>
        /// Initialise with the menu page model passed by <paramref name="menuPageModel"/>
        /// </summary>
        /// <param name="menuPageModel">The menu page</param>
        public void Initialise(MenuPageModel menuPageModel)
        {
            PageTitle = menuPageModel.PageTitle;
            ActivePlayerName = PlayerStorageManager.Instance.CurrentPlayer.Name;
            MenuItems = new ObservableCollection<MenuItemViewModel>();
            foreach (MenuItemModel item in menuPageModel.MenuItems)
            {
                MenuItems.Add(new MenuItemViewModel(item));
            }
        }

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