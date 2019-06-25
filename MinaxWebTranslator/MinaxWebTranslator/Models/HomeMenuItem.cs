using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace MinaxWebTranslator.Models
{
    public enum MenuItemType
    {
		/// <summary>
		/// Main Detail page
		/// </summary>
        Main,
		/// <summary>
		/// Other detail page
		/// </summary>
        Others,
		/// <summary>
		/// RecentProject page request
		/// </summary>
		RecentProjectRequest,
		/// <summary>
		/// RecentProject page return selected project item
		/// </summary>
		RecentProjectSelected,
		/// <summary>
		/// TranslatorSelector page request
		/// </summary>
		TranslatorSelectorRequest,
		/// <summary>
		/// TranslatorSelector page closed
		/// </summary>
		TranslatorSelectorClosed,
		/// <summary>
		/// ProjectSetting page request
		/// </summary>
		ProjectSettingsRequest,
		/// <summary>
		/// ProjectSetting page closed
		/// </summary>
		ProjectSettingsClosed,
		/// <summary>
		/// Project New page/action request
		/// </summary>
		ProjectNew,
		/// <summary>
		/// Project Open page/action request
		/// </summary>
		ProjectOpen,
		/// <summary>
		/// Project Save page/action request
		/// </summary>
		ProjectSave,
		/// <summary>
		/// Project Close page/action request
		/// </summary>
		ProjectClose,
	}

    public class HomeMenuItem : INotifyPropertyChanged
    {
        public MenuItemType Id {
			get => id;
			set => SetProperty( ref id, value );
		}
		private MenuItemType id;

		public ImageSource Icon {
			get => icon;
			set => SetProperty( ref icon, value );
		}
		private ImageSource icon;

        public string Title {
			get => title;
			set => SetProperty( ref title, value );
		}
		private string title;


		public event PropertyChangedEventHandler PropertyChanged;

		protected bool SetProperty<T>( ref T backingStore, T value,
			[CallerMemberName]string propertyName = "" )
		{
			if( EqualityComparer<T>.Default.Equals( backingStore, value ) )
				return false;

			backingStore = value;
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
			return true;
		}
	}
}
