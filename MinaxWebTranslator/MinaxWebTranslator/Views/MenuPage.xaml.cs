using Minax.Collections;
using MinaxWebTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MenuPage : ContentPage
    {
		public MenuPage()
        {
            InitializeComponent();

			// Project group
			mProjectGroup = new ObservableList<HomeMenuItem> {
				GroupedLongName = "Project",
				GroupedShortName = "P",
			};

			//mProjectGroup.Add( new HomeMenuItem { Id = MenuItemType.ProjectNew, Title = "New" } );
			mProjectGroup.Add( new HomeMenuItem { Id = MenuItemType.ProjectOpen, Title = "Open" } );
			//mProjectGroup.Add( new HomeMenuItem { Id = MenuItemType.ProjectSave, Title = "Save" } );
			//mProjectGroup.Add( new HomeMenuItem { Id = MenuItemType.ProjectClose, Title = "Close" } );
			//mProjectGroup.Add( mProjectSettingsMenu );
			mProjectGroup.Add( new HomeMenuItem { Id = MenuItemType.RecentProjectRequest, Title = "Recent Project(s)" } );


			// Options group
			var optionMenuItems = new ObservableList<HomeMenuItem> {
				GroupedLongName = "Options",
				GroupedShortName = "O",
			};

			mXlatorMenu = new HomeMenuItem { Id = MenuItemType.TranslatorSelectorRequest, Title = "Translator" };
			optionMenuItems.Add( mXlatorMenu );
			optionMenuItems.Add( new HomeMenuItem { Id = MenuItemType.Others, Title = "About" } );


			mGroupMenuItems.Add( mProjectGroup );
			mGroupMenuItems.Add( optionMenuItems );

			LvMenu.ItemsSource = mGroupMenuItems;
			LvMenu.SelectedItem = mGroupMenuItems[0];
			LvMenu.ItemSelected += async ( sender, e ) => {
				if( e.SelectedItem == null )
					return;

				var id = ((HomeMenuItem)e.SelectedItem).Id;
				// tell MainPage selected MenuItemType
				await MessageHub.SendMessageAsync( this, MessageType.MenuNavigate, id );
				LvMenu.SelectedItem = null;
			};

			// subscribe MessageHub event
			MessageHub.MessageReceived += ( sender, type, data ) => {
				if( sender == this )
					return;

				switch( type ) {
					case MessageType.XlatorSelected:
						// update Translator menu icon when new Translator selected
						if( data is Models.TranslatorSelector ts ) {
							mXlatorMenu.Icon = ts.Icon;
						}
						break;

					case MessageType.ProjectClosed:
						// hide Project Setting MenuItem when project closed
						if( mProjectGroup.Contains( mProjectSaveMenu ) )
							mProjectGroup.Remove( mProjectSaveMenu );
						if( mProjectGroup.Contains( mProjectCloseMenu ) )
							mProjectGroup.Remove( mProjectCloseMenu );
						if( mProjectGroup.Contains( mProjectSettingsMenu ) )
							mProjectGroup.Remove( mProjectSettingsMenu );
						LvMenu.ItemsSource = mGroupMenuItems;
						break;

					case MessageType.ProjectOpened:
						// show Project Setting MenuItem when project opened
						if( mProjectGroup.Contains( mProjectSaveMenu ) == false )
							mProjectGroup.Insert( 1, mProjectSaveMenu );
						if( mProjectGroup.Contains( mProjectCloseMenu ) == false )
							mProjectGroup.Insert( 2, mProjectCloseMenu );
						if( mProjectGroup.Contains( mProjectSettingsMenu ) == false )
							mProjectGroup.Insert( 3, mProjectSettingsMenu );

						LvMenu.ItemsSource = mGroupMenuItems;
						break;
				}
			};
		}

		private ObservableCollection<ObservableList<HomeMenuItem>> mGroupMenuItems = new ObservableCollection<ObservableList<HomeMenuItem>>();
		private ObservableList<HomeMenuItem> mProjectGroup;

		private HomeMenuItem mXlatorMenu = null;
		private static readonly HomeMenuItem mProjectSettingsMenu = new HomeMenuItem {
			Id = MenuItemType.ProjectSettingsRequest, Title = "Settings"
		},
			mProjectSaveMenu = new HomeMenuItem { Id = MenuItemType.ProjectSave, Title = "Save" },
			mProjectCloseMenu = new HomeMenuItem { Id = MenuItemType.ProjectClose, Title = "Close" };

	}
}