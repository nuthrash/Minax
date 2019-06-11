using MahApps.Metro.Controls;
using Minax.Collections;
using Minax.Domain.Translation;
using System;
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
	public partial class MappingDockingPanel : LayoutAnchorable
	{
		internal bool IsProjectChanged => isProjChanged;

		public event PropertyChangedEventHandler StatusChanged;

		public MappingDockingPanel( MetroWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();
		}

		private readonly MetroWindow mMainWindow;
		private bool isProjChanged = false;

		private void _SetProjChanged()
		{
			isProjChanged = true;
			StatusChanged?.Invoke( this, new PropertyChangedEventArgs(nameof(IsProjectChanged) ) );
		}

		private void BtnMappingAllToolClearSorting_Click( object sender, RoutedEventArgs e )
		{
			DgMappingAll.ClearSort();
		}

		private void BtnMappingProjConfNew_Click( object sender, RoutedEventArgs e )
		{
			/*
			var list = DgMappingProjConf.ItemsSource as ObservableList<MappingMonitor.MappingModel>;
			if( list == null )
				return;

			var inputSettings = new MetroDialogSettings {
				AffirmativeButtonText = "OK", DefaultButtonFocus = MessageDialogResult.Affirmative,
				DialogResultOnCancel = MessageDialogResult.Canceled, OwnerCanCloseWithDialog = true,
				NegativeButtonText = "Cancel",
			};
			var newOrig = await this.ShowInputAsync( "Add New Mapping", "New mapping text:", inputSettings );

			// show warning about OriginalText is all white spaces
			if( string.IsNullOrWhiteSpace( newOrig ) ) {
				await this.ShowMessageAsync( "Original Text Error", "The Mapping text shall not be full of white space!!" );
				return;
			}

			// show warning about OriginalText is only one word
			if( newOrig.Length <= 1 ) {
				await this.ShowMessageAsync( "Original Text Warning", "The Mapping text might too short to replaced many words incorrectly!!" );
			}

			// check orig is existed
			var first = list.FirstOrDefault( item => item.OriginalText == newOrig );
			if( first != null ) {
				await this.ShowMessageAsync( "Duplicate Text", $"Sorry! The original text of new mapping \"{newOrig}\" duplicated with existed item!!" );
				DgMappingProjConf.SelectedItem = first;
				return;
			}

			// add new mapping entry to collection
			var model = new MappingMonitor.MappingModel { OriginalText = newOrig, ProjectBasedFileName = mProject.FileName };
			model.PropertyChanged += MappingModel_PropertyChanged;
			mProject?.Project?.MappingTable?.Add( model );
			list.Add( model );
			_SetProjChanged();
			*/

		}

		private void BtnMappingProjConfDeleteEntry_Click( object sender, RoutedEventArgs e )
		{
			var entry = DgMappingProjConf.SelectedItem as MappingMonitor.MappingModel;
			var list = DgMappingProjConf.ItemsSource as ObservableList<MappingMonitor.MappingModel>;
			if( entry == null || list == null || list.Contains( entry ) == false )
				return;

			list.Remove( entry );
			_SetProjChanged();
		}

		private void BtnMappingProjConfMoveUp_Click( object sender, RoutedEventArgs e )
		{
			var entry = DgMappingProjConf.SelectedItem as MappingMonitor.MappingModel;
			var list = DgMappingProjConf.ItemsSource as ObservableList<MappingMonitor.MappingModel>;
			if( entry == null || list == null || list.Contains( entry ) == false )
				return;

			var idx = list.IndexOf( entry );
			if( idx <= 0 )
				return;

			list.Move( idx, idx - 1 );
			_SetProjChanged();
		}

		private void BtnMappingProjConfMoveDown_Click( object sender, RoutedEventArgs e )
		{
			var entry = DgMappingProjConf.SelectedItem as MappingMonitor.MappingModel;
			var list = DgMappingProjConf.ItemsSource as ObservableList<MappingMonitor.MappingModel>;
			if( entry == null || list == null || list.Contains( entry ) == false )
				return;

			var idx = list.IndexOf( entry );
			if( idx < 0 || idx >= list.Count - 1 )
				return;

			list.Move( idx, idx + 1 );
			_SetProjChanged();
		}

		private void BtnMappingGlossariesToolClearSorting_Click( object sender, RoutedEventArgs e )
		{
			DgMappingGlossaries.ClearSort();
		}
	}
}
