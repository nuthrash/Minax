using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MinaxWebTranslator.Desktop.Commands
{
	/// <summary>
	/// SimpleCommand for simplifying ICommand
	/// </summary>
	public class SimpleCommand : ICommand
	{
		public event EventHandler CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public Func<object, bool> CanExecuteDelegate { get; set; }

		public Action<object> ExecuteDelegate { get; set; }

		public SimpleCommand( Func<object, bool> canExecute = null, Action<object> execute = null )
		{
			this.CanExecuteDelegate = canExecute;
			this.ExecuteDelegate = execute;
		}

		public bool CanExecute( object parameter )
		{
			var canExecute = this.CanExecuteDelegate;
			return canExecute == null || canExecute( parameter );
		}

		public void Execute( object parameter )
		{
			this.ExecuteDelegate?.Invoke( parameter );
		}
	}
}
