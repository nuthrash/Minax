using Minax.Collections;
using Minax.Domain.Translation;
using MinaxWebTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MinaxWebTranslator.Views
{
	[XamlCompilation( XamlCompilationOptions.Compile )]
	public partial class MappingGlossarySubPage : ContentPage
	{
		internal ProjectModel CurrentProject {
			get => mProject;
			set {
				if( mProject == value )
					return;

				mProject = value;

				ReloadMapping();
			}
		}

		internal TranslatorSelector CurrentTranslator {
			get => mCurrentXaltor;
			set {
				if( mCurrentXaltor == value )
					return;

				mCurrentXaltor = value;
				ReloadMapping();
			}
		}

		public MappingGlossarySubPage()
		{
			InitializeComponent();

			mVm = new ViewModels.GeneralViewModel {
				IsDataEmpty = true, TextWidthRequest = 50.0,
				TextColumnGridLength = new GridLength( 50, GridUnitType.Absolute )
			};
			this.BindingContext = mVm;

			LblTmp.SizeChanged += ( s1, e1 ) => {
				mVm.TextWidthRequest = (double)Math.Floor( LblTmp.Width ) + 4.0;
				mVm.TextColumnGridLength = new GridLength( mVm.TextWidthRequest, GridUnitType.Absolute );
				this.ForceLayout();
			};

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		private ProjectModel mProject;
		private TranslatorSelector mCurrentXaltor;
		private ViewModels.GeneralViewModel mVm;
		private GridLength _gridLenStar = new GridLength( 1.0, GridUnitType.Star );

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			if( sender == this )
				return;

			switch( type ) {
				case MessageType.AppClosed:
				case MessageType.AppClosing:
				case MessageType.ProjectClosed:
					CurrentProject = null;
					break;

				case MessageType.ProjectOpened:
					if( data is ProjectModel pm )
						CurrentProject = pm;
					else
						CurrentProject = ProjectManager.Instance.CurrentProject;

					break;


				case MessageType.ProjectChanged:
					if( data is ProjectModel projModel ) {
						CurrentProject = projModel;
					}
					break;
				case MessageType.DataReload:
					if( data is ProjectModel reloadModel ) {

						ReloadMapping();
					}
					break;

				// File Changed/Deleted/Updated
				case MessageType.GlossaryNew:
				case MessageType.GlossaryDeleted:
				case MessageType.GlossaryRenamed:
				case MessageType.GlossaryUpdated:
					ReloadMapping();
					break;

				case MessageType.XlatorSelected:
					if( data is TranslatorSelector ts )
						CurrentTranslator = ts;
					break;
			}
		}

		private void Label_SizeChanged( object sender, EventArgs e )
		{
			var label = (Label)sender;
			var grid = (Grid)label.Parent;
			if( mVm.TextColumnGridLength.Value <= LvMappingGlossary.Width / 2 - 2 ) {
				var column = grid.ColumnDefinitions[0];
				column.Width = mVm.TextColumnGridLength;
			}
			else {
				// divide to 1/2
				grid.ColumnDefinitions[0].Width = _gridLenStar;
				grid.ColumnDefinitions[1].Width = _gridLenStar;
			}
		}

		internal void ReloadMapping()
		{
			LvMappingGlossary.IsRefreshing = true;
			LvMappingGlossary.ItemsSource = new List<MappingMonitor.MappingModel>();

			if( mProject == null ) {
				mVm.IsDataEmpty = true;
				LvMappingGlossary.IsRefreshing = false;
				return;
			}

			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return;

			// trigger SizeChanged for longest OriginalText to get measured TextWidthRequest
			string maxStr = "";

			// collect glossary Mapping Tables (not contain project Mapping Table)
			ObservableCollection<ObservableList<MappingMonitor.MappingModel>> it = new ObservableCollection<ObservableList<MappingMonitor.MappingModel>>();
			foreach( var fullPath in mon.MonitoringFileList ) {
				if( fullPath == mProject.FullPathFileName )
					continue;

				var fileName = fullPath.Replace( mon.BaseProjectPath, "" );
				var l1 = mon.GetMappingCollection( fullPath );
				if( l1 == null || l1.Count <= 0 )
					continue;

				var max = l1.OrderByDescending( x => x.OriginalText.Length ).First();
				if( max != null && max.OriginalText.Length > maxStr.Length )
					maxStr = max.OriginalText;

				ObservableList<MappingMonitor.MappingModel> coll = new ObservableList<MappingMonitor.MappingModel>( l1 );
				coll.GroupedLongName = fileName;
				coll.GroupedShortName = $"{fileName[0]}";
				it.Add( coll );
			}

			// trigger SizeChanged for longest OriginalText to get measured TextWidthRequest
			LblTmp.Text = maxStr;
			LvMappingGlossary.ItemsSource = it;
			LvMappingGlossary.IsRefreshing = false;

			mVm.IsDataEmpty = maxStr == "";
		}
	}
}