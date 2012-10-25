using System.Windows;
using System.Windows.Input;

namespace Locima.SlidingBlock.Messaging
{

    /// <summary>
    /// Allows the view to be requested to show a confirmation dialogue box and invoke methods depending on which button is pressed by the user
    /// </summary>
    public class ConfirmationMessageArgs : MessageArgs
    {

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
        /// May be null</remarks>
        public ICommand OnCancelCommand { get; set; }
    }
}