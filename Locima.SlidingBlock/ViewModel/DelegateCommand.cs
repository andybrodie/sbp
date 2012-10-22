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

        public DelegateCommand(Action<object> executeAction)
            : this(executeAction, null)
        {
        }

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

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        #endregion

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