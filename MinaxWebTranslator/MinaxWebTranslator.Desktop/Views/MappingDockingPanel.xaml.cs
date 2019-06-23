﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Translation;
using MinaxWebTranslator.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
using static Minax.Domain.Translation.SupportedLanguagesExtensions;

namespace MinaxWebTranslator.Desktop.Views
{
	/// <summary>
	/// Dockable panel for Mapping tables
	/// </summary>
	public partial class MappingDockingPanel : LayoutAnchorable
	{
		internal bool IsProjectChanged => mProjChanged;

		public MappingDockingPanel( MainWindow mainWindow )
		{
			mMainWindow = mainWindow;

			InitializeComponent();

			DgMappingAll.Items.Clear();
			DgMappingGlossaries.Items.Clear();
			DgMappingProjConf.Items.Clear();
			DgMappingAll.ItemsSource = null;
			DgMappingGlossaries.ItemsSource = null;
			DgMappingProjConf.ItemsSource = null;

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		// default Input Dialog setting for MahApps
		private readonly MetroDialogSettings sInputSettings = new MetroDialogSettings {
			AffirmativeButtonText = Languages.Global.Str0Ok, NegativeButtonText = Languages.Global.Str0Cancel,
			DefaultButtonFocus = MessageDialogResult.Affirmative,
			DialogResultOnCancel = MessageDialogResult.Canceled, OwnerCanCloseWithDialog = true,
		};

		private readonly MainWindow mMainWindow;
		private ProjectModel mProject;
		private bool mProjChanged = false;


		private TranslatorSelector mCurrentXlator = null;
		private RemoteType mCurrentRemoteTranslator = RemoteType.Excite;


		private async void _SetProjChanged()
		{
			mProjChanged = true;
			await MessageHub.SendMessageAsync( this, MessageType.ProjectChanged, mProject );
		}

		private async Task<bool> _ReloadAllMappingData( ProjectModel model )
		{
			if( model == null || model.Project == null || model.FullPathFileName == null ||
				model.Project.SourceLanguage == SupportedSourceLanguage.AutoDetect )
				return false;

			// model shall open first
			if( model.IsCurrent == false ) {
				return false;
			}

			if( mProject == null )
				mProject = model;

			// de-subscribe all event of mapping data
			_UnsubscribeAllChangedEvents();

			// update TcMapping TabItems
			TcMapping.Items.Clear();
			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return false;

			DgMappingProjConf.ItemsSource = new List<MappingMonitor.MappingModel>();
			DgMappingGlossaries.ItemsSource = DgMappingProjConf.ItemsSource;
			DgMappingAll.ItemsSource = DgMappingProjConf.ItemsSource;

			await Task.Delay( 200 );

			// collect all glossary files' mapping models
			var fileList = new List<string>( mon.MonitoringFileList );
			if( fileList.Contains( model.FullPathFileName ) )
				fileList.Remove( model.FullPathFileName );

			// all glossary files and project mapping table are ready
			// binding glossaries to DgMappingGlossaries
			if( fileList.Count > 0 ) {
				var glossaries = new ObservableList<MappingMonitor.MappingModel>();
				foreach( var fn in fileList ) {
					var coll = mon.GetMappingCollection( fn );
					if( coll == null )
						continue;

					glossaries.AddRange( coll );
					_SubscribeChangedEvents( coll );
				}

				var cvs = new CollectionViewSource() { Source = glossaries };
				cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
				DgMappingGlossaries.ItemsSource = cvs.View;
				TiMappingGlossaries.Visibility = Visibility.Visible;
				if( TcMapping.Items.Contains(TiMappingGlossaries) == false )
					TcMapping.Items.Insert( 0, TiMappingGlossaries );
			}

			bool hasGlossaryEntry = TcMapping.Items.Count > 0;
			var projModels = mon.GetMappingCollection( model.FullPathFileName );
			if( projModels == null ) {
				projModels = new ObservableList<MappingMonitor.MappingModel>();
			}
			_SubscribeChangedEvents( projModels );

			DgMappingProjConf.ItemsSource = projModels;
			TiMappingProjConf.Header = model.ProjectName;
			TiMappingProjConf.IsSelected = true;
			if( TcMapping.Items.Contains(TiMappingProjConf) == false )
				TcMapping.Items.Insert( 0, TiMappingProjConf );

			if( hasGlossaryEntry ) {
				CollectionViewSource cvs = new CollectionViewSource();
				cvs.Source = mon.DescendedModels;
				cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
				DgMappingAll.ItemsSource = cvs.View;
				TiMappingAll.Visibility = Visibility.Visible;
				if( TcMapping.Items.Contains(TiMappingAll) == false )
					TcMapping.Items.Insert( 0, TiMappingAll );
			}

			return true;
		}

		private void _ReloadDataGridCvs( DataGrid dg, IReadOnlyList<MappingMonitor.MappingModel> newColl )
		{
			var cv = dg.ItemsSource as ICollectionView;
			var cvs = new CollectionViewSource();

			dg.ItemsSource = null;
			if( newColl != null ) {
				cvs.Source = newColl;
				cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
			}
			else if( cv != null && cv.SourceCollection is IReadOnlyCollection<MappingMonitor.MappingModel> aoc ) {
				var newAoc = new ObservableList<MappingMonitor.MappingModel>();
				newAoc.AddRange( aoc );
				cvs.Source = newAoc;
				cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
			}

			dg.ItemsSource = cvs.View;
		}

		private void _SubscribeChangedEvents( IReadOnlyObservableList<MappingMonitor.MappingModel> projModels )
		{
			foreach( var entry in projModels ) {
				entry.PropertyChanged -= MappingModel_PropertyChanged;
				entry.PropertyChanged += MappingModel_PropertyChanged;
			}
			projModels.CollectionChanged -= MappingModelTables_CollectionChanged;
			projModels.CollectionChanged += MappingModelTables_CollectionChanged;

		}

		private void _UnsubscribeAllChangedEvents()
		{
			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return;

			foreach( var fn in mon.MonitoringFileList ) {
				var coll = mon.GetMappingCollection( fn );
				if( coll == null )
					continue;

				coll.CollectionChanged -= MappingModelTables_CollectionChanged;
				foreach( var entry in coll ) {
					entry.PropertyChanged -= MappingModel_PropertyChanged;
				}
			}
		}

		private void _ModifyBindingWhenFileChanged( MessageType type, object data )
		{
			if( data == null || ProjectManager.Instance.MappingMonitor == null )
				return;

			var mon = ProjectManager.Instance.MappingMonitor;
			var args = data as MappingMonitor.MappingEventArgs;
			ObservableList<MappingMonitor.MappingModel> glossaries = null;

			switch( type ) {
				case MessageType.ProjectRenamed:
					// most things were done by MainWindow, so just change title here
					TiMappingProjConf.Header = mProject.ProjectName;
					break;

				case MessageType.ProjectUpdated:
					if( args != null )
						DgMappingProjConf.ItemsSource = mon.GetMappingCollection( args.FullPath );
					break;

				case MessageType.GlossaryRenamed:
					_ReloadDataGridCvs( DgMappingGlossaries, null );
					_ReloadDataGridCvs( DgMappingAll, null );
					break;

				case MessageType.GlossaryNew:
					if( args == null )
						break;
					var cvNew = DgMappingGlossaries.ItemsSource as ICollectionView;
					var collNew = mon.GetMappingCollection( args.FullPath );
					if( collNew == null )
						break;

					if( cvNew == null || cvNew.SourceCollection is ObservableList<MappingMonitor.MappingModel> == false ) {
						glossaries = new ObservableList<MappingMonitor.MappingModel>();
						glossaries.AddRange( collNew );
						var cvs = new CollectionViewSource { Source = glossaries };
						cvs.GroupDescriptions.Add( new PropertyGroupDescription( nameof( MappingMonitor.MappingModel.ProjectBasedFileName ) ) );
						DgMappingGlossaries.ItemsSource = cvs.View;
					}
					else {
						glossaries = cvNew.SourceCollection as ObservableList<MappingMonitor.MappingModel>;
						glossaries.AddRange( collNew );
					}

					break;

				case MessageType.GlossaryDeleted:
					var cv = DgMappingGlossaries.ItemsSource as ICollectionView;
					if( cv == null || cv.IsEmpty || args == null )
						return;

					var coll = mon.GetMappingCollection( args.FullPath );
					if( coll == null )
						return;

					// unbind MappingAll DataGrid ItemsSource first, otherwise its CVS would be incorrect
					DgMappingAll.ItemsSource = null;
					glossaries = cv.SourceCollection as ObservableList<MappingMonitor.MappingModel>;

					if( glossaries != null ) {
						glossaries.RemoveItems( coll );
					}

					// remove old Mapping file
					mon.RemoveMonitoring( args.FullPath );

					// bind MappingAll DataGrid ItemsSource with new CV
					_ReloadDataGridCvs( DgMappingAll, mon.DescendedModels );

					break;

				case MessageType.GlossaryUpdated:
					if( args == null )
						break;

					DgMappingGlossaries.ItemsSource = null;
					DgMappingAll.ItemsSource = null;

					// remove old Mapping file
					mon.RemoveMonitoring( args.FullPath );

					// try to parse updated file
					var list = ProjectManager.Instance.TryParseAndExtractMappingEntries( args.FullPath );
					if( list != null ) {
						mon.AddMonitoring( args.FullPath, list );
					}

					glossaries = new ObservableList<MappingMonitor.MappingModel>();
					// last is project conf. file, so ignore it
					for( int i = 0; i < mon.MonitoringFileList.Count - 1; ++i ) {
						var aoc = mon.GetMappingCollection( mon.MonitoringFileList[i] );
						if( aoc != null )
							glossaries.AddRange( aoc );
					}

					if( glossaries.Count > 0 ) {
						_ReloadDataGridCvs( DgMappingGlossaries, glossaries );
						_ReloadDataGridCvs( DgMappingAll, mon.DescendedModels );
					}

					break;
			}

			if( glossaries != null && glossaries.Count > 0 ) {
				TiMappingGlossaries.Visibility = Visibility.Visible;
				if( TcMapping.Items.Contains( TiMappingGlossaries ) == false )
					TcMapping.Items.Add( TiMappingGlossaries );
				if( TcMapping.Items.Contains( TiMappingAll ) == false )
					TcMapping.Items.Insert( 0, TiMappingAll );
			} else {
				// hide the Glossary and All Tab
				if( TcMapping.Items.Contains( TiMappingGlossaries ) )
					TcMapping.Items.Remove( TiMappingGlossaries );
				if( TcMapping.Items.Contains( TiMappingAll ) )
					TcMapping.Items.Remove( TiMappingAll );
			}

		}

		private async void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			if( sender == this )
				return;

			switch( type ) {
				case MessageType.ProjectOpened:
					if( data is ProjectModel pm ) {
						mProject = pm;
					}
					else {
						mProject = ProjectManager.Instance.CurrentProject;
					}

					if( mProject != null ) {
						await _ReloadAllMappingData( mProject );

						GdMappingProjConf.IsEnabled = true;
					} else {
						DgMappingAll.ItemsSource = null;
						DgMappingGlossaries.ItemsSource = null;
						DgMappingProjConf.ItemsSource = null;
						GdMappingProjConf.IsEnabled = false;
					}

					break;

				case MessageType.AppClosed:
				case MessageType.AppClosing:
				case MessageType.ProjectClosed:
					DgMappingAll.ItemsSource = null;
					DgMappingGlossaries.ItemsSource = null;
					DgMappingProjConf.ItemsSource = null;
					mProject = null;
					GdMappingProjConf.IsEnabled = false;
					break;

				case MessageType.ProjectSaved:
					mProjChanged = false;
					break;

				case MessageType.ProjectChanged:
					if( data is ProjectModel projModel ) {
						if( projModel != mProject )
							mProject = projModel;
						TiMappingProjConf.Header = mProject.ProjectName;
					}
					break;
				case MessageType.DataReload:
					if( data is ProjectModel reloadModel ) {
						//var rst = await _ReloadAllMappingData( reloadModel );
						await _ReloadAllMappingData( reloadModel );
					}
					break;

				// File Changed/Deleted/Updated
				case MessageType.ProjectRenamed:
				case MessageType.ProjectUpdated:
				case MessageType.GlossaryNew:
				case MessageType.GlossaryDeleted:
				case MessageType.GlossaryRenamed:
				case MessageType.GlossaryUpdated:
					_ModifyBindingWhenFileChanged( type, data );
					break;

				case MessageType.XlatorSelected:
					if( data is TranslatorSelector translatorSelector ) {
						if( mCurrentXlator == translatorSelector )
							break;

						// different selector shall reload list!!
						mCurrentXlator = translatorSelector;
						if( mCurrentXlator != null )
							mCurrentRemoteTranslator = mCurrentXlator.RemoteType;
						await _ReloadAllMappingData( mProject );
					}
					break;

				case MessageType.XlatingQuick:
				case MessageType.XlatingSections:
					if( data is bool onOffXlating ) {
						GdMappingProjConf.IsEnabled = !onOffXlating;
					}
					break;
			}
		}

		private void MappingModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			var model = sender as MappingMonitor.MappingModel;
			if( model == null )
				return;

			// sync. MappingModel with MappingEntry content
			if( mProject != null && mProject.Project != null && mProject.Project.MappingTable != null &&
				mProject.Project.MappingTable.Count > 0 &&
				mProject.Project.MappingTable[0] is MappingMonitor.MappingModel == false ) {
				var table = mProject.Project.MappingTable;
				table.Clear();
				table.AddRange( ProjectManager.Instance.MappingMonitor.GetMappingCollection( mProject.FullPathFileName ) );
			}

			_SetProjChanged();
		}

		private void MappingModelTables_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if( sender is ObservableList<MappingMonitor.MappingModel> coll ) {
				if( mProject != null && mProject.Project != null ) {
					var table = mProject.Project.MappingTable;
					table.Clear();
					table.AddRange( coll );
				}
			}

			_SetProjChanged();
			// no need update ListCollectionView in ItemsSource
		}

		private void BtnMappingAllToolClearSorting_Click( object sender, RoutedEventArgs e )
		{
			DgMappingAll.ClearSort();
		}

		private async void BtnMappingProjConfNew_Click( object sender, RoutedEventArgs e )
		{
			var list = DgMappingProjConf.ItemsSource as ObservableList<MappingMonitor.MappingModel>;
			if( list == null )
				return;

			var newOrig = await mMainWindow.ShowInputAsync( "Add New Mapping", "New mapping text:", sInputSettings );

			// show warning about OriginalText is all white spaces
			if( string.IsNullOrWhiteSpace( newOrig ) ) {
				await mMainWindow.ShowMessageAsync( "Original Text Error", "The Mapping text shall not be empty or full of white space!!" );
				return;
			}

			// show warning about OriginalText is only one word
			if( newOrig.Length <= 1 ) {
				await mMainWindow.ShowMessageAsync( "Original Text Warning", "The Mapping text might too short to replaced many words incorrectly!!" );
			}

			// check orig is existed
			var first = list.FirstOrDefault( item => item.OriginalText == newOrig );
			if( first != null ) {
				await mMainWindow.ShowMessageAsync( "Duplicate Text", $"Sorry! The original text of new mapping \"{newOrig}\" duplicated with existed item!!" );
				DgMappingProjConf.SelectedItem = first;
				return;
			}

			// add new mapping entry to collection
			var model = new MappingMonitor.MappingModel { OriginalText = newOrig, ProjectBasedFileName = mProject.FileName };
			model.PropertyChanged += MappingModel_PropertyChanged;
			mProject?.Project?.MappingTable?.Add( model );
			list.Add( model );
			_SetProjChanged();

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

		private void DgMappingProjConf_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if( DgMappingProjConf.SelectedItem == null ) {
				BtnMappingProjConfDeleteEntry.IsEnabled = false;
				BtnMappingProjConfMoveUp.IsEnabled = false;
				BtnMappingProjConfMoveDown.IsEnabled = false;
			}
			else {
				BtnMappingProjConfDeleteEntry.IsEnabled = true;
				BtnMappingProjConfMoveUp.IsEnabled = true;
				BtnMappingProjConfMoveDown.IsEnabled = true;
			}
		}
	}
}