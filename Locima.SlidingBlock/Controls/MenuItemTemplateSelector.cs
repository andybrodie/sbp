using System;
using System.Windows;
using Locima.SlidingBlock.ViewModel;

namespace Locima.SlidingBlock.Controls
{
    /// <summary>
    /// A dynamic template selector that allows a different display and behaviour for <see cref="MenuItemViewModel"/> items, depending on the return value of <see cref="MenuItemViewModel.IsEnabled"/>
    /// </summary>
    /// <remarks>For usage, see <see cref="MainPage"/> XAML.</remarks>
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// The template to use for enabled menu items
        /// </summary>
        public DataTemplate Enabled { get; set; }


        /// <summary>
        /// The template to use for disables menu items
        /// </summary>
        public DataTemplate Disabled { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            MenuItemViewModel model = item as MenuItemViewModel;
            
            // Set to whether we want the "Enabled" template (true), "Disabled" template (false) or don't care (null)
            Boolean? isEnabled;

            // Sample data in the designer uses a faked MenuItemViewModel
            if (model == null)
            {

                if (item != null)
                {
                    string isEnabledString = RetrieveDesignerProperty(item, "IsEnabled");
                    bool isEnabledPrimitive;
                    if (Boolean.TryParse(isEnabledString, out isEnabledPrimitive))
                    {
                        isEnabled = isEnabledPrimitive;
                    } else
                    {
                        isEnabled = null;
                    }
                } else
                {
                    isEnabled = null;
                }
            }
            else
            // The item passed to the template selector was an instance of MenuItemViewModel, so in that case just use the value in the IsEnabled property
            {
                isEnabled = model.IsEnabled;
            }

            return isEnabled.HasValue ? (isEnabled.Value ? Enabled : Disabled) : base.SelectTemplate(item, container);
        }

    }
}