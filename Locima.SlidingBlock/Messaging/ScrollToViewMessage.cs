namespace Locima.SlidingBlock.Messaging
{
    public class ScrollToViewMessage : MessageArgs
    {
        public ScrollToViewMessage(string scrollable, object itemToScrollTo)
        {
            Scrollable = scrollable;
            ItemToScrollTo = itemToScrollTo;
        }

        public object ItemToScrollTo { get; set; }

        public string Scrollable { get; set; }
    }
}