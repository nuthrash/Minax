using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using MinaxWebTranslator.Models;

namespace MinaxWebTranslator.ViewModels
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
		bool isBusy = false;
	
		public string Title {
			get => title;
			set => SetProperty( ref title, value );
		}
		string title = string.Empty;

		/// <summary>
		/// Is items empty
		/// </summary>
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


		protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

		
		#region "IDataErrorInfo members"
		public string Error => string.Empty;

		public string this[string columnName] {
			get {
				switch( columnName ) {
					case nameof( NonEmptyString ):
						if( string.IsNullOrWhiteSpace( NonEmptyString ) )
							return Languages.Global.Str0FieldCantEmptyOrWhitespaces;
						break;
				}
				return null;
			}
		}
		#endregion


		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
