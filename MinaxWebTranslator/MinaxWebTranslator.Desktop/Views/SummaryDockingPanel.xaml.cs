using MahApps.Metro.Controls;
using Minax.Domain.Translation;
using MinaxWebTranslator.Desktop.Converters;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AvalonDock.Layout;


namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Dockable panel for summary used MappingModel after translating
	/// </summary>
	public partial class SummaryDockingPanel : LayoutAnchorable
	{
		public SummaryDockingPanel() : this( Application.Current.MainWindow as MainWindow )
		{ }

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
					// ReloadList()
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
