using System;
using System.Windows;
using Locima.SlidingBlock.ViewModel;
using Locima.SlidingBlock.ViewModel.Menus;

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

        /// <summary>
        ///   Selects either the <see cref="Enabled" /> template or <see cref="Disabled" /> template, depending on the value of <see
        ///    cref="MenuItemViewModel.IsEnabled" />
        /// </summary>
        /// <param name="item"> The <see cref="MenuItemViewModel" /> that is to be rendered </param>
        /// <param name="container"> <c>this</c> </param>
        /// <returns>
        ///   <para> Either <see cref="Enabled" /> or <see cref="Disabled" /> . If no value for <see
        ///    cref="MenuItemViewModel.IsEnabled" /> could be found then this defers to <see
        ///    cref="DataTemplateSelector.SelectTemplate" /> (this returns <c>null</c> which means that the item won't be rendered. </para>
        ///   <para> Serves you right for including an object that wasn't an instance of <see cref="MenuItemViewModel" />! </para>
        /// </returns>
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