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

        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
         
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectTemplate(newContent, this);
        }


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