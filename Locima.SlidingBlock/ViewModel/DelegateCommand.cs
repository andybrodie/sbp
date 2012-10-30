using System;
using System.Windows.Input;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// Provides a skeleton implementation of <see cref="ICommand"/> which allows an <see cref="Func{TParam, TResult}"/> and <see cref="Action{T1}"/> to be used to provide the
    /// implementations of <see cref="ICommand.CanExecute"/> and <see cref="ICommand.Execute"/> within a view model.
    /// </summary>
    /// <remarks>
    ///   Code courtesy of Windows Phone Geek: http://www.windowsphonegeek.com/articles/Building-a-Reusable-ICommand-implementation-for-Windows-Phone-Mango-MVVM-apps
    /// </remarks>
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _executeAction;

        /// <summary>
        /// Delegates to <see cref="DelegateCommand(Action{object}, Func{object,bool})"/> passing <paramref name="executeAction"/>
        /// </summary>
        public DelegateCommand(Action<object> executeAction)
            : this(executeAction, null)
        {
        }

        /// <summary>
        /// Initialises this delegate command with the <paramref name="executeAction"/> function and <paramref name="canExecute"/> function
        /// </summary>
        /// <param name="executeAction">Must not be null</param>
        /// <param name="canExecute"></param>
        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            if (executeAction == null)
            {
                throw new ArgumentNullException("executeAction");
            }
            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        #region ICommand Members

        /// <summary>
        /// Delegates to <see cref="CanExecute"/>, if it's not null, passing <paramref name="parameter"/>
        /// </summary>
        /// <See cref="ICommand.CanExecute"/>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            bool result = true;
            Func<object, bool> canExecuteHandler = _canExecute;
            if (canExecuteHandler != null)
            {
                result = canExecuteHandler(parameter);
            }

            return result;
        }

        /// <summary>
        /// Raised by the response from <see cref="ICommand.CanExecute"/> would change
        /// </summary>
        /// <see cref="ICommand.CanExecuteChanged"/>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Delegates call to the execute action passed on the constructor <see cref="DelegateCommand"/>
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        #endregion

        /// <summary>
        /// If set, invoke <see cref="CanExecuteChanged"/>
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}