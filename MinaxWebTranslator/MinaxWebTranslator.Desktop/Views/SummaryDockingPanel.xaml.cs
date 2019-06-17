using MahApps.Metro.Controls;
using Minax.Domain.Translation;
using MinaxWebTranslator.Desktop.Converters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;


namespace MinaxWebTranslator.Desktop.Views
{
	public partial class SummaryDockingPanel : LayoutAnchorable
	{
		public SummaryDockingPanel( MetroWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();

			DgMappingSummary.Items.Clear();
			DgMappingSummary.ItemsSource = null;

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
			CbSummaryGroupBy.SelectionChanged -= CbSummaryGroupBy_SelectionChanged;
			CbSummaryGroupBy.SelectionChanged += CbSummaryGroupBy_SelectionChanged;
		}

		private readonly MetroWindow mMainWindow;

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			switch( type ) {
				case MessageType.AppClosing:
				case MessageType.AppClosed:
				case MessageType.ProjectClosed:
				//case MessageType.XlatorSelected:
					DgMappingSummary.ItemsSource = null;
					break;

				case MessageType.ProjectOpened:
				case MessageType.DataReload:
					var cvs = new CollectionViewSource { Source = TranslatorHelpers.CurrentUsedModels };
					switch( CbSummaryGroupBy.SelectedIndex ) {
						case 1: // File Name
							cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
							break;

						default: // Category
							cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.Category ),
															new TextCategoryL10nItemsConverter() ) );
							break;
					}
					DgMappingSummary.ItemsSource = cvs.View;

					break;

				case MessageType.XlatingSections:
					if( data is bool onOffSections && onOffSections == false ) {
						DgMappingSummary.UpdateLayout();
					}
					break;
				case MessageType.XlatingQuick:
					if( data is bool onOffQuick && onOffQuick == false ) {
						DgMappingSummary.UpdateLayout();
					}
					break;
			}
		}

		private void BtnMappingSummaryToolClearSorting_Click( object sender, RoutedEventArgs e )
		{
			DgMappingSummary.ClearSort();
		}

		private void CbSummaryGroupBy_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			var cv = DgMappingSummary.ItemsSource as ICollectionView;
			if( cv == null || cv.SourceCollection == null )
				return;

			cv.GroupDescriptions.Clear();
			switch( CbSummaryGroupBy.SelectedIndex ) {
				case 1: // File Name
					cv.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
					break;

				default: // Category
					cv.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.Category ),
													new TextCategoryL10nItemsConverter() ) );
					break;
			}

			DgMappingSummary.UpdateLayout();
		}
	}
}
