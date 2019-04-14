using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Sammelkarten {

    /// <summary>Provides a base implementation of the <see cref="ICommand"/> interface. </summary>
    public abstract class CommandBase : ObservableObject, ICommand {

        #region Events

        /// <summary>Occurs when changes occur that affect whether or not the command should execute. </summary>
        public event EventHandler CanExecuteChanged;

        #endregion Events

        #region Properties

        /// <summary>Gets a value indicating whether the command can execute in its current state. </summary>
        public abstract bool CanExecute { get; }

        #endregion Properties

        #region Methods

        /// <summary>Triggers the CanExecuteChanged event and a property changed event on the CanExecute property. </summary>
        public virtual void RaiseCanExecuteChanged() {
            RaisePropertyChanged(nameof(CanExecute));

            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Tries to execute the command by checking the <see cref="CanExecute"/> property
        /// and executes the command only when it can be executed. </summary>
        /// <returns>True if command has been executed; false otherwise. </returns>
        public bool TryExecute() {
            if (!CanExecute) {
                return false;
            }

            Execute();
            return true;
        }

        bool ICommand.CanExecute(object parameter) {
            return CanExecute;
        }

        void ICommand.Execute(object parameter) {
            Execute();
        }

        /// <summary>Defines the method to be called when the command is invoked. </summary>
        protected abstract void Execute();

        #endregion Methods
    }

    /// <summary>Provides an implementation of the <see cref="ICommand"/> interface. </summary>
    /// <typeparam name="T">The type of the command parameter. </typeparam>
    public abstract class CommandBase<T> : ICommand {

        #region Events

        /// <summary>Occurs when changes occur that affect whether or not the command should execute. </summary>
        public event EventHandler CanExecuteChanged;

        #endregion Events

        #region Methods

        /// <summary>Gets a value indicating whether the command can execute in its current state. </summary>
        /// <param name="parameter">todo: describe parameter parameter on CanExecute</param>
        [DebuggerStepThrough]
        public abstract bool CanExecute(T parameter);

        /// <summary>Triggers the CanExecuteChanged event. </summary>
        public virtual void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Tries to execute the command by calling the <see cref="CanExecute"/> method
        /// and executes the command only when it can be executed. </summary>
        /// <param name="parameter">todo: describe parameter parameter on TryExecute</param>
        /// <returns>True if command has been executed; false otherwise. </returns>
        public bool TryExecute(T parameter) {
            if (!CanExecute(parameter)) {
                return false;
            }

            Execute(parameter);
            return true;
        }

        [DebuggerStepThrough]
        bool ICommand.CanExecute(object parameter) {
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter) {
            Execute((T)parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">todo: describe parameter parameter on Execute</param>
        protected abstract void Execute(T parameter);

        #endregion Methods
    }
}