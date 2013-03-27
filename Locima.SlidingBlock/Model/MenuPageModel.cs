using System.Collections.ObjectModel;

namespace Locima.SlidingBlock.Model
{
    /// <summary>
    /// The view model for a single menu page, consisting of just a title and a collection of view models (<see cref="MenuItemModel"/>) for each menu item
    /// </summary>
    public class MenuPageModel
    {
        /// <summary>
        /// The title of the menu page
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// The list of view models for menu items to be shown on the menu page
        /// </summary>
        public Collection<MenuItemModel> MenuItems { get; set; }

    }
}