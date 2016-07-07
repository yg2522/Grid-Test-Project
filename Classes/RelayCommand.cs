using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WpfApplication1.Classes
{
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        #endregion Fields

        #region Properties

        // Added to support custom keybinding for commands.
        // Example XAML implementation of the command inside a view:
        //  <KeyBinding Command="{Binding ExitCommand}" Key="{Binding ExitCommand.GestureKey}" Modifiers="{Binding ExitCommand.GestureModifier}"/>
        public Key GestureKey { get; set; }
        public ModifierKeys GestureModifier { get; set; }
        public MouseAction MouseGesture { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute", @"Execute Method cannot be null");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return _canExecute == null || _canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            _execute(parameters);
        }

        #endregion ICommand Members
    }

    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T, object> _execute;
        readonly Predicate<object> _canExecute;
        private readonly T _associatedObject;

        //Added to support custom keybinding for commands.
        // Example implementation of the command inside a view
        //<KeyBinding Command="{Binding ExitCommand}" Key="{Binding ExitCommand.GestureKey}" Modifiers="{Binding ExitCommand.GestureModifier}"/>
        public Key GestureKey { get; set; }
        public ModifierKeys GestureModifier { get; set; }
        public MouseAction MouseGesture { get; set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<T, object> execute, T item)
            : this(execute, null, item)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T, object> execute, Predicate<object> canExecute, T item)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
            _associatedObject = item;
        }

        #endregion Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return _canExecute == null || _canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            _execute(_associatedObject, parameters);
        }

        #endregion ICommand Members
    }

    #region Async command handling

    /// <summary>
    /// This is the no-parameter version of the one defined below.
    /// </summary>
    public class RelayCommandAsync : RelayCommandAsync<object>
    {
        public RelayCommandAsync(Action execute)
            : base(o => execute())
        {
        }

        public RelayCommandAsync(Action execute, Predicate<object> canExecute)
            : base(o => execute(), o => canExecute(null))
        {
        }
    }

    /// <summary>
    /// A command that calls the specified delegate when the command is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommandAsync<T> : ICommand, IRaiseCanExecuteChanged
    {
        #region Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;
        private bool _isExecuting;

        #endregion Fields

        #region Properties

        // Added to support custom keybinding for commands.
        // Example XAML implementation of the command inside a view:
        //  <KeyBinding Command="{Binding ExitCommand}" Key="{Binding ExitCommand.GestureKey}" Modifiers="{Binding ExitCommand.GestureModifier}"/>
        public Key GestureKey { get; set; }
        public ModifierKeys GestureModifier { get; set; }
        public MouseAction MouseGesture { get; set; }

        #endregion Properties

        #region Constructors

        public RelayCommandAsync(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommandAsync(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute", @"Execute Method cannot be null");
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        #endregion ICommand Members

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return !_isExecuting && CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            _isExecuting = true;
            try
            {
                RaiseCanExecuteChanged();
                Execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }

    public interface IRaiseCanExecuteChanged
    {
        void RaiseCanExecuteChanged();
    }

    // And an extension method to make it easy to raise changed events
    public static class CommandExtensions
    {
        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            var canExecuteChanged = command as IRaiseCanExecuteChanged;
            canExecuteChanged?.RaiseCanExecuteChanged();
        }
    }

    #endregion Async command handling
}
