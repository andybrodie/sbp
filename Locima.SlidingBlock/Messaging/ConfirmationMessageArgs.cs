using System.Windows.Input;

namespace Locima.SlidingBlock.Messaging
{
    public class ConfirmationMessageArgs : MessageArgs
    {
        public string Message { get; set; }
        public string Title { get; set; }

        public ICommand OnOkCommand { get; set; }
        public ICommand OnCancelCommand { get; set; }
    }
}