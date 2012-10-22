using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;

namespace Locima.SlidingBlock.Messaging
{
    /// <summary>
    /// Adds a convenience extension method to all page code-behind files to register default message handlers
    /// </summary>
    public static class DefaultHandlerExtensionMethod
    {
        public static void RegisterDefaultMessageHandlers(this PhoneApplicationPage page, ViewModelBase viewModel)
        {
            DefaultMessageHandlers.Register(page, viewModel);
        }
    }
}