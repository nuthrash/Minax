using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MinaxWebTranslator.Desktop.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public bool IsBusy {
			get => isBusy;
			set => SetProperty( ref isBusy, value );
		}
		private bool isBusy = false;

		public string Title {
			get => title;
			set => SetProperty( ref title, value );
		}
		private string title = string.Empty;

		public bool IsDataEmpty {
			get => isDataEmpty;
			set => SetProperty( ref isDataEmpty, value );
		}
		private bool isDataEmpty = true;


		protected bool SetProperty<T>( ref T backingStore, T value = default( T ),
										[CallerMemberName]string propertyName = "",
										Action onChanged = null )
		{
			if( EqualityComparer<T>.Default.Equals( backingStore, value ) )
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged( propertyName );
			return true;
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
		{
			this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion
	}
}
