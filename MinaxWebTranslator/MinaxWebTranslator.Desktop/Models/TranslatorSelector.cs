using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace MinaxWebTranslator.Desktop.Models
{
	/// <summary>
	/// Translator selector with notification
	/// </summary>
	internal class TranslatorSelector : INotifyPropertyChanged
	{
		public RemoteType RemoteType {
			get => rt;
			set => SetProperty( ref rt, value );
		}

		public string Header {
			get => header;
			set => SetProperty( ref header, value );
		}
		public bool Checked {
			get => check;
			set => SetProperty( ref check, value );
		}
		/// <summary>
		/// BitmapImage objecet, null means Separator
		/// </summary>
		public ImageSource Icon {
			get => icon;
			set => SetProperty( ref icon, value );
		}

		public string Description { get; set; }
		public bool IsSeparator { get; set; } = false;

		private RemoteType rt = RemoteType.None;
		private string header = null;
		private bool check = false;
		private ImageSource icon = null;

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

		protected void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
		{
			this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion
	}
}
