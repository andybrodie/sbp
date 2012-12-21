using System.Windows;
using System.Windows.Input;
using Locima.SlidingBlock.ViewModel;
using NLog;

namespace Locima.SlidingBlock.Messaging
{

    /// <summary>
    /// Allows the view to be requested to show a confirmation dialogue box and invoke methods depending on which button is pressed by the user
    /// </summary>
    public class ConfirmationMessageArgs : MessageArgs
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The message to display (you need to localise it first), typically applied to <see cref="MessageBox.Show(string,string,System.Windows.MessageBoxButton)"/>
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The title to apply to the message box <see cref="MessageBox.Show(string,string,System.Windows.MessageBoxButton)"/>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Will be invoked if the OK button is pressed
        /// </summary>
        /// <remarks>
        /// May be null.</remarks>
        public ICommand OnOkCommand { get; set; }

        /// <summary>
        /// Will be invoked if the Cancel button is pressed
        /// </summary>
        /// <remarks>
        /// If null, then no cancel option will be presented to the user</remarks>
        public ICommand OnCancelCommand { get; set; }

        /// <summary>
        /// A convenience action that does nothing except log a debug message
        /// </summary>
        public static ICommand NoOpCommand { get; private set; }

        /// <summary>
        /// Initialises <see cref="NoOpCommand"/>
        /// </summary>
        static ConfirmationMessageArgs()
        {
            NoOpCommand = new DelegateCommand(o => Logger.Debug("No-op action invoked for confirmation dialog {0}", o));
        }
    }
}