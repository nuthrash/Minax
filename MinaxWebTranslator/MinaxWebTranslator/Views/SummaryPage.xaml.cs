using Minax.Collections;
using Minax.Domain.Translation;
using MinaxWebTranslator.Converters;
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
	public partial class SummaryPage : ContentPage
	{

		public SummaryPage()
		{
			InitializeComponent();

			mVm = new ViewModels.GeneralViewModel {
				IsDataEmpty = true, TextWidthRequest = 50.0,
				TextColumnGridLength = new GridLength( 50, GridUnitType.Absolute )
			};
			this.BindingContext = mVm;

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;

			// record perfer column 0 width
			LblTmp.SizeChanged += ( s1, e1 ) => {
				mVm.TextWidthRequest = (double)Math.Floor( LblTmp.Width ) + 4.0;
				mVm.TextColumnGridLength = new GridLength( mVm.TextWidthRequest, GridUnitType.Absolute );
				this.ForceLayout();
			};
		}

		//private ProjectModel mProject;
		private ViewModels.GeneralViewModel mVm;

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			switch( type ) {
				case MessageType.AppClosing:
				case MessageType.AppClosed:
				case MessageType.ProjectClosed:
				//case MessageType.XlatorSelected:
					LvMappingSummary.ItemsSource = null;
					break;

				case MessageType.ProjectOpened:
				case MessageType.DataReload:
					this.ReloadList();

					break;

				case MessageType.XlatingSections:
					if( data is bool onOffSections && onOffSections == false ) {
						this.ReloadList();
					}
					break;
				case MessageType.XlatingQuick:
					if( data is bool onOffQuick && onOffQuick == false ) {
						this.ReloadList();
					}
					break;
			}
		}

		internal void ReloadList()
		{
			LvMappingSummary.ItemsSource = new ObservableCollection<MappingMonitor.MappingModel>();

			if( ProjectManager.Instance == null )
				return;

			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return;

			var list = TranslatorHelpers.CurrentUsedModels;
			if( list == null || list.Count <= 0 )
				return;

			// trigger SizeChanged for longest OriginalText to get measured TextWidthRequest
			LblTmp.Text = list[0].OriginalText;

			ObservableCollection<ObservableList<MappingMonitor.MappingModel>> it = new ObservableCollection<ObservableList<MappingMonitor.MappingModel>>();

			// collect all Mapping entries by grouping in Category
			foreach( TextCategory cat in Enum.GetValues( typeof( TextCategory ) ) ) {
				var usedList = list.Where( x => x.Category == cat );
				if( usedList == null || usedList.Count() <= 0 )
					continue;

				ObservableList<MappingMonitor.MappingModel> coll = new ObservableList<MappingMonitor.MappingModel>( usedList );
				coll.GroupedLongName = Minax.Utils.GetTextCategoryL10nString( cat );
				coll.GroupedShortName = $"{coll.GroupedLongName[0]}";
				it.Add( coll );
			}

			// collect Category == null entries
			var catEmptyList = list.Where( x => x.Category == null );
			it.Add( new ObservableList<MappingMonitor.MappingModel>( catEmptyList ) {
				GroupedLongName = "NO CATEGORY",
			} );

			LvMappingSummary.ItemsSource = it;

			mVm.IsDataEmpty = false;
		}

		private void Label_SizeChanged( object sender, EventArgs e )
		{
			var label = (Label)sender;
			var grid = (Grid)label.Parent;
			var column = grid.ColumnDefinitions[0];
			column.Width = mVm.TextColumnGridLength;
		}

	}
}