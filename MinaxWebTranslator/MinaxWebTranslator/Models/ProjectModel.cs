using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace MinaxWebTranslator.Models
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

		// full-path combined from ProjectBasePath + CoverImageRelativePath
		public string CoverImageFullPath {
			get => coverImageFullPath;
			set => SetProperty( ref coverImageFullPath, value );
		}
		// Allocated ImageSource object of CoverImageFullPath
		public ImageSource CoverImageSource {
			get => coverImageSource;
			set => SetProperty( ref coverImageSource, value );
		}

		public bool IsCurrent {
			get => isCurrent;
			set => SetProperty( ref isCurrent, value );
		}

		public TranslationProject Project {
			get => proj;
			set => SetProperty( ref proj, value );
		}

		private bool isCurrent = false;
		private string projName, fileName, fullPathFileName, coverImageFullPath;
		private TranslationProject proj;
		private ImageSource coverImageSource;

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

		public string ToSettingString()
		{
			return CovertToSettingString( this );
		}

		public static string CovertToSettingString( ProjectModel pm )
		{
			if( pm == null )
				return "";
			var sep = TranslationProject.FieldSeparator;
			return $"{pm.ProjectName}{sep}{pm.FullPathFileName}{sep}{pm.CoverImageFullPath}";
		}

		public static ProjectModel ConvertFromSettingString( string settingStr )
		{
			ProjectModel pm = new ProjectModel();

			var fields = settingStr.Split( TranslationProject.FieldSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries );

			if( fields == null )
				return pm;

			if( fields.Length >= 1 )
				pm.ProjectName = fields[0];
			if( fields.Length >= 2 )
				pm.FullPathFileName = fields[1];
			if( fields.Length >= 3 )
				pm.CoverImageFullPath = fields[2];

			if( pm.FullPathFileName != null ) {
				try {
					pm.FileName = System.IO.Path.GetFileName( pm.FullPathFileName );
				}
				catch { }
			}

			return pm;
		}
	}
}
