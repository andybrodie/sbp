using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Locima.SlidingBlock.Controls
{
    /// <summary>
    /// Abstract class for creating data template selectors where the display of an item within, for example, a <see cref="ListBox"/> is altered depending on a value within the
    /// item being displayed.
    /// </summary>
    /// <remarks>Code and pattern courtesy of http://www.windowsphonegeek.com/articles/Implementing-Windows-Phone-7-DataTemplateSelector-and-CustomDataTemplateSelector</remarks>
    public abstract class DataTemplateSelector : ContentControl
    {

        /// <summary>
        /// SHould be overridded by subclasses, this method examines the <paramref name="item"/> and then returns the <see cref="DataTemplate"/> to use with that item.
        /// </summary>
        /// <param name="item">The itme being rendered (usually a view model object)</param>
        /// <param name="container">The object that will contain the item (usually this)</param>
        /// <returns>Default implementation returns <c>null</c></returns>
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
         
        /// <summary>
        /// Invoked if an item changes, this then allows the selector to set, or change its mind, which <see cref="DataTemplate"/> to use.
        /// </summary>
        /// <see cref="ContentControl.OnContentChanged"/>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectTemplate(newContent, this);
        }


        /// <summary>
        /// Uses reflection to retrieve the property named by <paramref name="propertyName"/> on the <paramref name="item"/> object
        /// </summary>
        /// <param name="item">The (typically view model) object to retrieve the property from</param>
        /// <param name="propertyName">The name of the property on the <paramref name="item"/> to retrieve</param>
        /// <returns>The value of the property as a string</returns>
        protected string RetrieveDesignerProperty(object item, string propertyName)
        {
            if (item == null) return null;
            Type itemType = item.GetType();
            PropertyInfo info = itemType.GetProperty(propertyName);
            object obj = info.GetValue(item, null);
            return obj == null ? null : obj.ToString();
        }

    }
}