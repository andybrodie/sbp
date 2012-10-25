using Locima.SlidingBlock.ViewModel;
using Microsoft.Phone.Controls;

namespace Locima.SlidingBlock.Messaging
{
    /// <summary>
    /// Adds a convenience extension method to all page code-behind files to register default message handlers
    /// </summary>
    public static class DefaultHandlerExtensionMethod
    {

        /// <summary>
        /// Registers the default message handlers <see cref="DefaultMessageHandlers.Register"/>
        /// </summary>
        /// <param name="page">The object that this method is available on</param>
        /// <param name="viewModel">The view model object to register the handlers on</param>
        public static void RegisterDefaultMessageHandlers(this PhoneApplicationPage page, ViewModelBase viewModel)
        {
            DefaultMessageHandlers.Register(page, viewModel);
        }
    }
}