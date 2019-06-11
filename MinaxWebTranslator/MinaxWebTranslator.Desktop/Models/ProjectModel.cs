using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MinaxWebTranslator.Desktop.Models
{
	/// <summary>
	/// Project model with notification
	/// </summary>
	internal class ProjectModel : INotifyPropertyChanged
	{
		public string ProjectName {
			get => projName;
			set => SetProperty( ref projName, value );
		}
		public string FileName {
			get => fileName;
			set => SetProperty( ref fileName, value );
		}
		public string FullPathFileName {
			get => fullPathFileName;
			set => SetProperty( ref fullPathFileName, value );
		}

		public TranslationProject Project {
			get => proj;
			set => SetProperty( ref proj, value );
		}
		public bool InUsed {
			get => inUsed;
			set => SetProperty( ref inUsed, value );
		}

		private bool inUsed = false;
		private string projName, fileName, fullPathFileName;
		private TranslationProject proj;

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
