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
	public partial class MappingAllSubPage : ContentPage
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


		public MappingAllSubPage()
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

		private double _colSize = 0.0;
		private List<ColumnDefinition> _columns = new List<ColumnDefinition>();
		private GridLength _gridLenStar = new GridLength( 1.0, GridUnitType.Star );

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			if( sender == this )
				return;

			switch( type ) {
				case MessageType.ProjectOpened:
					if( data is ProjectModel pm ) {
						CurrentProject = pm;
					}
					else {
						CurrentProject = ProjectManager.Instance.CurrentProject;
					}

					break;

				case MessageType.AppClosed:
				case MessageType.AppClosing:
				case MessageType.ProjectClosed:
					CurrentProject = null;
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
				case MessageType.ProjectRenamed:
				case MessageType.ProjectUpdated:
				case MessageType.GlossaryNew:
				case MessageType.GlossaryDeleted:
				case MessageType.GlossaryRenamed:
				case MessageType.GlossaryUpdated:
				case MessageType.XlatorSelected:
					ReloadMapping();
					break;
				
			}
		}


		private void Label_SizeChanged( object sender, EventArgs e )
		{
			var label = (Label)sender;
			var grid = (Grid)label.Parent;

			if( mVm.TextColumnGridLength.Value <= LvMappingAll.Width / 2 - 2 ) {
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
			if( mProject == null ) {
				LvMappingAll.ItemsSource = null;
				mVm.IsDataEmpty = true;
				return;
			}

			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return;
			var list = mon.DescendedModels;
			if( list == null || list.Count <= 0 )
				return;

			// trigger SizeChanged for longest OriginalText to get measured TextWidthRequest
			LblTmp.Text = list[0].OriginalText;

			ObservableCollection<ObservableList<MappingMonitor.MappingModel>> it = new ObservableCollection<ObservableList<MappingMonitor.MappingModel>>();

			// collect all Mapping entries by grouping in reversed seq.
			foreach( var fullPath in mon.MonitoringFileList.Reverse() ) {
				var fileName = fullPath.Replace( mon.BaseProjectPath, "" );
				var l1 = list.Where( x => x.ProjectBasedFileName == fileName );
				if( l1 == null || l1.Count() <= 0 )
					continue;

				ObservableList<MappingMonitor.MappingModel> coll = new ObservableList<MappingMonitor.MappingModel>( l1 );
				coll.GroupedLongName = fileName;
				coll.GroupedShortName = $"{fileName[0]}";
				it.Add( coll );
			}

			LvMappingAll.ItemsSource = it;

			mVm.IsDataEmpty = false;
		}
	}
}