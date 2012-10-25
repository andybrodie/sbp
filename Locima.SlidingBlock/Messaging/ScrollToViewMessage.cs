using System.Windows.Controls;

namespace Locima.SlidingBlock.Messaging
{
    /// <summary>
    /// Represents a request to the view to scroll the scrollable UI element (usually a <see cref="ListBox"/>) to show the item passed
    /// </summary>
    public class ScrollToViewMessage : MessageArgs
    {

        /// <summary>
        /// Initialises both <see cref="ItemToScrollTo"/> and <see cref="Scrollable"/>.  Typically implemented via <see cref="ListBox.ScrollIntoView"/>.
        /// </summary>
        /// <param name="scrollable">The ID of the object, typically a <see cref="ListBox"/> that you want to set the scroll position to</param>
        /// <param name="itemToScrollTo">The item that should be shown</param>
        public ScrollToViewMessage(string scrollable, object itemToScrollTo)
        {
            Scrollable = scrollable;
            ItemToScrollTo = itemToScrollTo;
        }

        /// <summary>
        /// The item that should be shown
        /// </summary>
        public object ItemToScrollTo { get; set; }

        /// <summary>
        /// The ID of the object, typically a <see cref="ListBox"/> that you want to set the scroll position to
        /// </summary>
        public string Scrollable { get; set; }
    }
}