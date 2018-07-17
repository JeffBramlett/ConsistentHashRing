using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConsistentHashRingApp.ViewModels
{
    /// <summary>
    /// ViewModel base class
    /// </summary>
    public abstract class AbstractViewModel : NotifyOnChangeObject,  IDisposable
    {
        #region Fields

        private readonly AbstractViewModel _parentViewModel;

        /// <summary>
        /// Message handler for creating new HttpClients
        /// </summary>
        private HttpMessageHandler _messageHandler;

        private Dictionary<string, IList<string>> _errorsDictionary;

        #endregion

        #region public Properties

        /// <summary>
        /// Message handler to use for HttpClient (could be a Mock for unit testing)
        /// </summary>
        private HttpMessageHandler MsgHandler
        {
            get
            {
                _messageHandler = _messageHandler ?? new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                };
                return _messageHandler;
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// The parent View Model for this ViewModel (injected)
        /// </summary>
        protected AbstractViewModel ParentViewModel
        {
            get { return _parentViewModel; }
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// Error maintenance and management dictionary
        /// </summary>
        private Dictionary<string, IList<string>> ErrorsDictionary
        {
            get
            {
                _errorsDictionary = _errorsDictionary ?? new Dictionary<string, IList<string>>();
                return _errorsDictionary;
            }
        }
        #endregion

        #region Ctors

        /// <summary>
        /// Default ctor, optional HttpMessageHandler
        /// </summary>
        /// <param name="navigationView">inject the Navigation view</param>
        /// <param name="parentViewModel">inject the parent view model</param>
        /// <param name="injectedHandler">inject the HttpMessageHandler for unit tests</param>
        protected AbstractViewModel(AbstractViewModel parentViewModel, HttpMessageHandler injectedHandler = null)
        {
            _messageHandler = injectedHandler;
            _parentViewModel = parentViewModel;
        }

        #endregion

        #region Virtual LifeCycle Events

        /// <summary>
        /// Initialize the ViewModel (call this from view event for Init)
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// Load the ViewModel (call this from view event for Load
        /// </summary>
        public virtual void Load()
        {

        }

        /// <summary>
        /// Perform any unloading (call this from view event for unload)
        /// </summary>
        public virtual void Unload()
        {

        }

        #endregion

        #region Protected Command Creation

        /// <summary>
        /// Create Command with Action delegate
        /// </summary>
        /// <param name="cmdExe">the Action delegate</param>
        /// <returns>new ICommand object</returns>
        protected ICommand CreateCommand(Action cmdExe)
        {
            ICommand cmd = new DelegateCommand(cmdExe);
            return cmd;
        }

        /// <summary>
        /// Create command with Action delegate and Func delegate
        /// </summary>
        /// <param name="cmdExe">the command execution</param>
        /// <param name="canExecuteCmd">the determining delegate for if the command should be executed</param>
        /// <returns>new ICommand object</returns>
        protected ICommand CreateCommand(Action cmdExe, Func<bool> canExecuteCmd)
        {
            ICommand cmd = new DelegateCommand(cmdExe, canExecuteCmd);
            return cmd;
        }

        /// <summary>
        /// Create command with Generic type delegate
        /// </summary>
        /// <typeparam name="T">the type of object for input</typeparam>
        /// <param name="cmdExe">the Action delegate taking the type as argument</param>
        /// <returns>new ICommand delegate</returns>
        protected ICommand CreateCommand<T>(Action<T> cmdExe)
        {
            ICommand cmd = new DelegateCommand<T>(cmdExe);
            return cmd;
        }

        /// <summary>
        /// Create command with Generic Action delegate and Func 
        /// </summary>
        /// <typeparam name="T">the type of argument for the cmd and for the func</typeparam>
        /// <param name="cmdExe">the execution delegate accepting object of type T</param>
        /// <param name="canExecuteCmd">determines if execution can occur on object of type T</param>
        /// <returns></returns>
        protected ICommand CreateCommand<T>(Action<T> cmdExe, Func<T, bool> canExecuteCmd)
        {
            ICommand cmd = new DelegateCommand<T>(cmdExe, canExecuteCmd);
            return cmd;
        }

        #endregion

        #region Disposable

        /// <summary>
        /// flag to prevent ObjectAlreadyDisposedException.  Disposal to occur only once.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// IDisposable implementation (deterministic dispose)
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execution Dispose method
        /// </summary>
        /// <param name="isFinalize">true if comes from the destructor (finalizer), false for deterministic dispose (same as Dispose())</param>
        protected void Dispose(bool isFinalize)
        {
            if (_isDisposed)
                return;

            GeneralDispose();

            if (isFinalize)
            {
                FinalizeDispose();
            }

            _isDisposed = true;

        }

        /// <summary>
        /// Virtual method "holder" for deterministic dispose (called in Dispose(bool isFinalize)
        /// </summary>
        public virtual void GeneralDispose()
        {
        }

        /// <summary>
        /// Virtual method "holder" for finalizer dispose
        /// </summary>
        public virtual void FinalizeDispose()
        {
        }

        #endregion

        #region Validation Error Handling
        /// <summary>
        /// Event for errors occurring
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// add specific error for a class Member
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberExpression"></param>
        /// <param name="message"></param>
        protected void AddError<T>(Expression<Func<T>> memberExpression, string message)
        {
            // Must have member expression to find property name
            if (memberExpression == null)
            {
                throw new ArgumentNullException();
            }

            var bodyExpr = memberExpression.Body as MemberExpression;

            // Member expression must have a body (a property)
            if (bodyExpr == null)
            {
                throw new ArgumentNullException();
            }

            string name = bodyExpr.Member.Name;

            if (!ErrorsDictionary.ContainsKey(name))
            {
                ErrorsDictionary.Add(name, new List<string>());
            }

            ErrorsDictionary[name].Add(message);

            RaiseErrorsChanged(name);
        }

        /// <summary>
        /// Clear errors for a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberExpression"></param>
        protected void ClearErrors<T>(Expression<Func<T>> memberExpression)
        {
            // Must have member expression to find property name
            if (memberExpression == null)
            {
                throw new ArgumentNullException();
            }

            var bodyExpr = memberExpression.Body as MemberExpression;

            // Member expression must have a body (a property)
            if (bodyExpr == null)
            {
                throw new ArgumentNullException();
            }

            string name = bodyExpr.Member.Name;

            if(ErrorsDictionary.ContainsKey(name))
                ErrorsDictionary.Remove(name);

            RaiseErrorsChanged(name);
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            return ErrorsDictionary[propertyName];
        }

        public bool HasErrors
        {
            get { return ErrorsDictionary.Keys.Any(); }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            var errorChanged = ErrorsChanged;
            if (errorChanged != null)
            {
                DataErrorsChangedEventArgs decea = new DataErrorsChangedEventArgs(propertyName);

                errorChanged(this, decea);
            }

            RaisePropertyChanged(() => HasErrors);
        }
        #endregion
    }
}
