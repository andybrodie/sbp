using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Locima.SlidingBlock.ViewModel
{
    public class MenuPageViewModel : ViewModelBase
    {
        private ObservableCollection<MenuItemViewModel> _menuItems;
        private string _pageTitle;

        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnNotifyPropertyChanged("PageTitle");
            }
        }

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

        private void MenuItemsOnCollectionChanged(object sender,
                                                  NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnNotifyPropertyChanged("MenuItems");
        }
    }
}