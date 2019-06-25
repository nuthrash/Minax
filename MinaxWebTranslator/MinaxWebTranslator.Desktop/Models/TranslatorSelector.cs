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
		/// <summary>
		/// Remote Translator/Translation API type
		/// </summary>
		public RemoteType RemoteType {
			get => rt;
			set => SetProperty( ref rt, value );
		}

		/// <summary>
		/// MenuItem/ListViewItem header 
		/// </summary>
		public string Header {
			get => header;
			set => SetProperty( ref header, value );
		}
		/// <summary>
		/// Checked in true means on occupied/selected
		/// </summary>
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

		/// <summary>
		/// Extra description text such as hyperlink
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// When SeparatorVisibility is Visible, means this Item is a Separator not a normal Translator
		/// </summary>
		public System.Windows.Visibility SeparatorVisibility { get; set; } = System.Windows.Visibility.Collapsed;

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
