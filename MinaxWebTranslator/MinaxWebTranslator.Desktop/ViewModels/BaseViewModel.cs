using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MinaxWebTranslator.Desktop.ViewModels
{
	/// <summary>
	/// Base ViewModel with general/comman commands or properties
	/// </summary>
	public class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
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

		public string DataErrorPlaceholder {
			get => dataErrorPlaceholder;
			set => SetProperty( ref dataErrorPlaceholder, value );
		}
		private string dataErrorPlaceholder;

		/// <summary>
		/// A string field cannot be empty
		/// </summary>
		public string NonEmptyString {
			get => nonEmptyString;
			set => SetProperty( ref nonEmptyString, value );
		}
		private string nonEmptyString;

		/// <summary>
		/// NonEmptyString field's placeholder/watermark
		/// </summary>
		public string NonEmptyMaxPlaceholder {
			get => nonEmptyMaxPlaceholder;
			set => SetProperty( ref nonEmptyMaxPlaceholder, value );
		}
		private string nonEmptyMaxPlaceholder;

		#region "IDataErrorInfo members"
		public string Error => string.Empty;

		public string this[string columnName] {
			get {
				switch( columnName ) {
					case nameof(NonEmptyString):
						if( string.IsNullOrWhiteSpace( NonEmptyString ) )
							return "This field cannot be empty or full of white text!";
						break;
				}
				return null;
			}
		}
		#endregion

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
