using AvalonDock.Layout;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Translation;
using MinaxWebTranslator.Desktop.Models;
using MinaxWebTranslator.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MinaxWebTranslator.Desktop
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		internal TranslatorSelector CurrentXlator {
			get => mCurrentXlator;
			private set {
				if( mCurrentXlator == value )
					return;

				mCurrentXlator = value;
				mCurrentRemoteType = mCurrentXlator.RemoteType;
				// update MenuItem's Icon to current xlator's icon
				MiTranslatorSelector.Icon = new Image() { Source = mCurrentXlator.Icon, Height = 24, Width = 24 };

				// tell other pages/panels the xlator has been changed
				_ = MessageHub.SendMessageAsync( this, MessageType.XlatorSelected, mCurrentXlator );
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			// manually add Docking Components, for _RestoreDockingLayout() to load their positions
			mSrcPanel = new Views.SourceDockingPanel( this );
			mTgtPanel = new Views.TargetDockingPanel( this );
			mMappingPanel = new Views.MappingDockingPanel( this );
			mSummaryPanel = new Views.SummaryDockingPanel( this );
			mQuickXlatePanel = new Views.QuickTranslationDockingPanel( this );
			_InitializeDockingPanels();

			// manually add TranslatorSelector panel
			mXlatorSelectorPanel = new Views.TranslatorSelectorPanel();
			FoTranslator.Content = mXlatorSelectorPanel;

			// add CommandBindings, Project Save (Ctrl + S)
			RoutedCommand saveProjectCmd = new RoutedCommand(), openProjectCmd = new RoutedCommand();
			saveProjectCmd.InputGestures.Add( new KeyGesture( Key.S, ModifierKeys.Control ) );
			CommandBindings.Add( new CommandBinding( saveProjectCmd, MiProjectSave_Click ) );
			openProjectCmd.InputGestures.Add( new KeyGesture( Key.O, ModifierKeys.Control ) );
			CommandBindings.Add( new CommandBinding( openProjectCmd, MiProjectOpen_Click ) );

			this.DataContext = new MainWindowViewModel();
		}

		private void MetroWindow_Loaded( object sender, RoutedEventArgs e )
		{
			// prepare Recent Porject list
			MiProjRecent.Items.Clear();
			ProjectManager.Instance.RestoreListFromSettings();
			MiProjRecent.ItemsSource = ProjectManager.Instance.Projects;

			CbGlossarySyncFile.Items.Clear();

			// prepare StatusBar message list
			CbStatusMessages.ItemsSource = mStatusMessages;
			if( mStatusMessages.Count > 0 )
				CbStatusMessages.SelectedIndex = 0;

			// prepare IProgress instance
			sTransProgress.ProgressChanged += ( s1, e1 ) => {
				var value = e1.PercentOrErrorCode;

				if( value >= 0 && value <= 100 ) {
					_UpdateStatus( e1.Message );
				}
				else {
					if( string.IsNullOrWhiteSpace( e1.Message ) == false ) {
						_UpdateStatus( string.Format( Languages.Global.Str2ErrorCodeMessage, e1.PercentOrErrorCode, e1.Message ) );
					}
				}
			};

			GdMain.IsEnabled = false;
#if DEBUG
			MiLayoutRestoreFromSettings.Visibility = Visibility.Visible;
			MiLayoutSaveToSettings.Visibility = Visibility.Visible;
#endif

			_PrepareWebBrowser();
			_PrepareSysFonts();
			_RestoreAppSettings();

			CurrentXlator = mXlatorSelectorPanel.SelectedTranslator;
			_UpdateStatus( Languages.Global.Str0AppReady );

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
		}

		private async void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			if( false == await _CheckAndAskProjSaving( false, true ) ) {
				e.Cancel = true;
				return;
			}

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;

			_CloseProject();
			//_SaveAppSettings();

			// wait for other panel unbind ItemsSource!!
			await Task.Delay( 100 );
		}

		#region "private data/methods/helpers"

		private readonly Views.SourceDockingPanel mSrcPanel = null;
		private readonly Views.TargetDockingPanel mTgtPanel = null;
		private readonly Views.MappingDockingPanel mMappingPanel = null;
		private readonly Views.SummaryDockingPanel mSummaryPanel = null;
		private readonly Views.QuickTranslationDockingPanel mQuickXlatePanel = null;
		private readonly Views.TranslatorSelectorPanel mXlatorSelectorPanel = null;

		private Views.AboutCreditsPanel mAboutPanel = null;

		private ProjectModel mProject;
		private string mProjFileName;
		private bool mProjChanged, mAutoMergeWhenFileChanged;
		private TranslatorSelector mCurrentXlator = null;
		private RemoteType mCurrentRemoteType = RemoteType.Excite;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		// GUI Progress Indicator
		private static readonly Progress<Minax.ProgressInfo> sTransProgress = new Progress<Minax.ProgressInfo>();
		// scroll sync.
		private volatile bool mIsScrolling = false;
		private Timer mScrollDeferredSyncer = null;

		// Mahapps ShowMessage's config for Yes/No
		private static readonly MetroDialogSettings sMetroDlgYesNoSettings = new MetroDialogSettings {
			AffirmativeButtonText = Languages.Global.Str0Yes, NegativeButtonText = Languages.Global.Str0No,
			DefaultButtonFocus = MessageDialogResult.Affirmative,
			DialogResultOnCancel = MessageDialogResult.Canceled, OwnerCanCloseWithDialog = true,
		};
		// Mahapps ShowMessage's config for Yes/No/Cancel
		private static readonly MetroDialogSettings sMetroDlgYesNoCancelSettings = new MetroDialogSettings {
			AffirmativeButtonText = Languages.Global.Str0Yes, NegativeButtonText = Languages.Global.Str0No,
			FirstAuxiliaryButtonText = Languages.Global.Str0Cancel,
			DefaultButtonFocus = MessageDialogResult.Affirmative,
			DialogResultOnCancel = MessageDialogResult.Canceled, OwnerCanCloseWithDialog = true,
		};

		//private int mStatusMsgCntMax = 1000; // for AppSettings
		private readonly ObservableList<string> mStatusMessages = new ObservableList<string>() { ItemsCountMaximum = 1000 };
		private readonly List<Control> mSysFontGlyphsSrc = new List<Control>(), mSysFontGlyphsTgt = new List<Control>();

		private void _InitializeDockingPanels()
		{
			AdlapgMain.Children.Add( new LayoutAnchorablePane( mSrcPanel ) );
			AdlapgMain.Children.Add( new LayoutAnchorablePane( mTgtPanel ) );
			AdlapgRight.Children.Add( mMappingPanel );
			AdlapgRight.Children.Add( mSummaryPanel );
			AdlapgRight.Children.Add( mQuickXlatePanel );

			//mQuickXlatePanel.Title = Languages.WebXlator.Str0QuickXlation;

			// manually add ScrollChanged event to sync. scrolling by interacting each other in realtime.
			var scrollHandler = new ScrollChangedEventHandler( RtbSrcTgt_ScrollChanged );
			mSrcPanel.RtbSource.AddHandler( ScrollViewer.ScrollChangedEvent, scrollHandler );
			mTgtPanel.RtbTarget.AddHandler( ScrollViewer.ScrollChangedEvent, scrollHandler );
		}

		private void _PrepareWebBrowser()
		{
			// set WebBrowser's IE version to higher for modern HTML
			var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
			using( var Key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey( @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true ) ) {
				Key.SetValue( appName, 99999, Microsoft.Win32.RegistryValueKind.DWord );
			}

#if DEBUG
			appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".vhost.exe"; // for visual studio
			using( var Key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey( @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true ) ) {
				Key.SetValue( appName, 99999, Microsoft.Win32.RegistryValueKind.DWord );
			}
#endif
		}

		private void _PrepareSysFonts()
		{
			mSysFontGlyphsSrc.Clear();

			// localization of font name for user friendly
			var locLang= System.Windows.Markup.XmlLanguage.GetLanguage( System.Globalization.CultureInfo.CurrentUICulture.Name );
			int locIdx = 0;
			foreach ( var fn in Fonts.SystemFontFamilies ) {
				var lbl = new Label();
				lbl.FontFamily = fn;
				lbl.FontSize = 14.0;

				if( fn.FamilyNames.ContainsKey(locLang) ) {
					// insert localized font name into front of list
					lbl.Content = fn.FamilyNames[locLang];
					mSysFontGlyphsSrc.Insert( locIdx++, lbl );
				} else {
					// use the first FamilyName from this FontFamily's supported locales
					lbl.Content = fn.FamilyNames[fn.FamilyNames.Keys.First()];
					//lbl.Content = fn.Source;
					mSysFontGlyphsSrc.Add(lbl);
				}
			}

			mSysFontGlyphsTgt.Clear();
			mSysFontGlyphsTgt.AddRange( mSysFontGlyphsSrc.ToArray() );

			// binding to ItemSource of CbSourceTextAreaFont and CbTargetTextAreaFont
			CbSourceTextAreaFont.ItemsSource = null;
			CbSourceTextAreaFont.Items.Clear();
			CbSourceTextAreaFont.ItemsSource = mSysFontGlyphsSrc;
			CbTargetTextAreaFont.ItemsSource = null;
			CbTargetTextAreaFont.Items.Clear();
			CbTargetTextAreaFont.ItemsSource = mSysFontGlyphsTgt;
		}


		private void _RestoreAppSettings()
		{
			_RestoreDockingLayout();

			var glossaryFileList = _BuildGlossaryFileList();
			CbGlossarySyncFile.SelectedItem = null;
			CbGlossarySyncFile.ItemsSource = glossaryFileList;
			CbGlossarySyncFile.SelectedIndex = 0;

			// Restore XlatorCrypto, API key, APP key.....
			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.XlatorCrypto ) ) {
				Properties.Settings.Default.XlatorCrypto = (DateTime.Now.Ticks + (new Random().Next( 99999, 99999999 ))).ToString();
				Properties.Settings.Default.Save();
				Properties.Settings.Default.Save();
			}

			TranslatorHelpers.SourceLanguage = SupportedSourceLanguage.Japanese;
			TranslatorHelpers.TargetLanguage = SupportedTargetLanguage.ChineseTraditional;
			TranslatorHelpers.AutoScrollToTop = false;

			/*
			FontFamily ffSrc, ffDst;
			switch( TranslatorHelpers.SourceLanguage ) {
				case SupportedSourceLanguage.ChineseTraditional:
					ffSrc = new FontFamily( "Microsoft JhengHei UI" );
					break;
				case SupportedSourceLanguage.Japanese:
					ffSrc = new FontFamily( "MS UI Gothic" );
					break;
				default:
					// for English and ChineseTraditional
					ffSrc = new FontFamily( Languages.Global.Str0DefaultTextFontFamilyName );
					break;
			}

			// for English and ChineseTraditional
			ffDst = new FontFamily( Languages.Global.Str0DefaultTextFontFamilyName );
			this.FontFamily = ffDst;
			mSrcPanel.RtbSource.FontFamily = ffSrc;
			mTgtPanel.RtbTarget.FontFamily = ffDst;
			*/

			// source/target panels min. font size
			double srcFontMin = Properties.Settings.Default.SourceTextAreaFontSizeMin;
			double tgtFontMin = Properties.Settings.Default.TargetTextAreaFontSizeMin;
			if( ManudSourceTextAreaFontSizeMin.Minimum > srcFontMin ) {
				// srcFontMin is too small than UI's value
				ManudSourceTextAreaFontSizeMin.Value = ManudSourceTextAreaFontSizeMin.Minimum;
				srcFontMin = ManudSourceTextAreaFontSizeMin.Minimum;
				Properties.Settings.Default.SourceTextAreaFontSizeMin = srcFontMin;
			} else {
				ManudSourceTextAreaFontSizeMin.Value = srcFontMin;
			}
			if( ManudTargetTextAreaFontSizeMin.Minimum > tgtFontMin ) {
				// tgtFontMin is too small than UI's value
				ManudTargetTextAreaFontSizeMin.Value = ManudTargetTextAreaFontSizeMin.Minimum;
				tgtFontMin = ManudTargetTextAreaFontSizeMin.Minimum;
				Properties.Settings.Default.SourceTextAreaFontSizeMin = tgtFontMin;
			}
			else {
				ManudTargetTextAreaFontSizeMin.Value = tgtFontMin;
			}

			if( mSrcPanel.RtbSource.FontSize < srcFontMin ) {
				mSrcPanel.RtbSource.FontSize = srcFontMin;
			}
			if( mTgtPanel.RtbTarget.FontSize < tgtFontMin ) {
				mTgtPanel.RtbTarget.FontSize = tgtFontMin;
			}

			for( int i = 0; i < mSysFontGlyphsSrc.Count; ++i ) {
				if( mSysFontGlyphsSrc[i].FontFamily.Source == Properties.Settings.Default.SourceTextAreaFontFamily ) {
					CbSourceTextAreaFont.SelectedIndex = i;
					break;
				}
			}
			if( CbSourceTextAreaFont.SelectedIndex < 0 ) {
				var defSrcFontName = Languages.Global.Str0DefaultTextFontFamilyName;
				for( int i = 0; i < mSysFontGlyphsTgt.Count; ++i ) {
					if( mSysFontGlyphsTgt[i].FontFamily.Source == defSrcFontName ) {
						CbSourceTextAreaFont.SelectedIndex = i;
						Properties.Settings.Default.SourceTextAreaFontFamily = defSrcFontName;
						break;
					}
				}
			}
			if( CbSourceTextAreaFont.SelectedIndex < 0) {
				CbSourceTextAreaFont.SelectedIndex = 0;
				Properties.Settings.Default.SourceTextAreaFontFamily = mSysFontGlyphsSrc[0].FontFamily.Source;
			}
			//mSrcPanel.RtbSource.FontFamily = mSysFontGlyphsSrc[CbSourceTextAreaFont.SelectedIndex].FontFamily;
			mSrcPanel.RtbSource.FontFamily = (CbSourceTextAreaFont.SelectedItem as Control).FontFamily;


			for ( int i = 0; i < mSysFontGlyphsTgt.Count; ++i ) {
				if( mSysFontGlyphsTgt[i].FontFamily.Source == Properties.Settings.Default.TargetTextAreaFontFamily ) {
					CbTargetTextAreaFont.SelectedIndex = i;
					break;
				}
			}
			if( CbTargetTextAreaFont.SelectedIndex < 0 ) {
				var defTgtFontName = Languages.Global.Str0DefaultTextFontFamilyName;
				for( int i = 0; i < mSysFontGlyphsTgt.Count; ++i ) {
					if( mSysFontGlyphsTgt[i].FontFamily.Source == defTgtFontName ) {
						CbTargetTextAreaFont.SelectedIndex = i;
						Properties.Settings.Default.TargetTextAreaFontFamily = defTgtFontName;
						break;
					}
				}
			}
			if (CbTargetTextAreaFont.SelectedIndex < 0) {
				CbTargetTextAreaFont.SelectedIndex = 0;
				Properties.Settings.Default.TargetTextAreaFontFamily = mSysFontGlyphsTgt[0].FontFamily.Source;
			}
			//mTgtPanel.RtbTarget.FontFamily = mSysFontGlyphsTgt[CbTargetTextAreaFont.SelectedIndex].FontFamily;
			mTgtPanel.RtbTarget.FontFamily = (CbTargetTextAreaFont.SelectedItem as Control).FontFamily;


			MatsMonitorAutoMergeWhenFileChanged.IsOn = Properties.Settings.Default.MonitorAutoMergeWhenFileChanged == true;
			if( Properties.Settings.Default.RemeberRecentProjects == true ) {
				MatsRemeberRecentProjects.IsOn = true;
				MiProjRecent.Visibility = Visibility.Visible;
				MiProjClearRecent.Visibility = Visibility.Visible;
			}
			else {
				MatsRemeberRecentProjects.IsOn = false;
				MiProjRecent.Visibility = Visibility.Collapsed;
				MiProjClearRecent.Visibility = Visibility.Collapsed;
			}

			int max = Properties.Settings.Default.RecentProjectCountMax;
			if( max < ManudRecentProjectMax.Minimum || max > ManudRecentProjectMax.Maximum )
				ManudRecentProjectMax.Value = ManudRecentProjectMax.Minimum;
			else
				ManudRecentProjectMax.Value = max;

			MatsRemeberRecentProjects.Toggled -= MatsRemeberRecentProjects_CheckedChanged;
			MatsRemeberRecentProjects.Toggled += MatsRemeberRecentProjects_CheckedChanged;
			//MatsRemeberRecentProjects.Unchecked -= MatsRemeberRecentProjects_CheckedChanged;
			//MatsRemeberRecentProjects.Unchecked += MatsRemeberRecentProjects_CheckedChanged;
		}

		private void _SaveAppSettings()
		{
			Properties.Settings.Default.RemeberRecentProjects = MatsRemeberRecentProjects.IsOn == true;
			Properties.Settings.Default.MonitorAutoMergeWhenFileChanged = MatsMonitorAutoMergeWhenFileChanged.IsOn == true;
			Properties.Settings.Default.RecentProjectCountMax = (int)ManudRecentProjectMax.Value.GetValueOrDefault();

			// Source Panel and Target Panel
			Properties.Settings.Default.SourceTextAreaFontFamily = mSysFontGlyphsSrc[CbSourceTextAreaFont.SelectedIndex].FontFamily.Source;
			Properties.Settings.Default.TargetTextAreaFontFamily = mSysFontGlyphsTgt[CbTargetTextAreaFont.SelectedIndex].FontFamily.Source;
			Properties.Settings.Default.SourceTextAreaFontSizeMin = ManudSourceTextAreaFontSizeMin.Value.GetValueOrDefault();
			Properties.Settings.Default.TargetTextAreaFontSizeMin = ManudTargetTextAreaFontSizeMin.Value.GetValueOrDefault();

			_SaveDockingLayout();
		}

		private void _SaveDockingLayout()
		{
			var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );

			// Serialize to Settings
			var base64 = string.Empty;
			using( var ms = new System.IO.MemoryStream() )
			using( var sw = new System.IO.StreamWriter( ms, Encoding.UTF8 ) ) {
				serializer.Serialize( sw );
				base64 = Convert.ToBase64String( ms.ToArray() );
			}
			Properties.Settings.Default.AvalonDockLayout = base64;
			Properties.Settings.Default.Save();
			Properties.Settings.Default.Save();
		}
		private void _RestoreDockingLayout()
		{
			var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			if( string.IsNullOrWhiteSpace( Properties.Settings.Default.AvalonDockLayout ) ) {
				// Deserialize from Default docking layout file in Resources
				_RestoreDefaultDockingLayout();

				// then, save restored layout to Settings
				var base64 = string.Empty;
				using( var ms = new System.IO.MemoryStream() )
				using( var sw = new System.IO.StreamWriter( ms, Encoding.UTF8 ) ) {
					serializer.Serialize( sw );
					base64 = Convert.ToBase64String( ms.ToArray() );
				}
				Properties.Settings.Default.AvalonDockLayout = base64;
				Properties.Settings.Default.Save();
				Properties.Settings.Default.Save();
			}
			else {
				// Deserialize from Settings resource
				var text = Convert.FromBase64String( Properties.Settings.Default.AvalonDockLayout );
				using( var ms = new System.IO.MemoryStream( text ) )
				using( var sr = new System.IO.StreamReader( ms, Encoding.UTF8 ) ) {
					serializer.Deserialize( sr );
				}
			}
		}
		private void _RestoreDefaultDockingLayout()
		{
			var xmlDoc = new System.Xml.XmlDocument();
			xmlDoc.LoadXml( Encoding.UTF8.GetString( Properties.Resources.DefaultAvalonDockLayout ) );

			// update each docking panel's Title to l10n string
			var node = xmlDoc.SelectSingleNode( "//LayoutAnchorable[@ContentId='sourceWindow']" );
			if( node != null )
				node.Attributes["Title"].Value = mSrcPanel.Title;
			node = xmlDoc.SelectSingleNode( "//LayoutAnchorable[@ContentId='targetWindow']" );
			if( node != null )
				node.Attributes["Title"].Value = mTgtPanel.Title;
			node = xmlDoc.SelectSingleNode( "//LayoutAnchorable[@ContentId='mappingWindow']" );
			if( node != null )
				node.Attributes["Title"].Value = mMappingPanel.Title;
			node = xmlDoc.SelectSingleNode( "//LayoutAnchorable[@ContentId='summaryWindow']" );
			if( node != null )
				node.Attributes["Title"].Value = mSummaryPanel.Title;
			node = xmlDoc.SelectSingleNode( "//LayoutAnchorable[@ContentId='quickTranslationWindow']" );
			if( node != null )
				node.Attributes["Title"].Value = mQuickXlatePanel.Title;

			var xmlStream = new System.IO.MemoryStream();
			xmlDoc.Save( xmlStream );

			xmlStream.Flush();
			xmlStream.Position = 0;

			var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			serializer.Deserialize( xmlStream );
		}

		private ObservableList<string> _BuildGlossaryFileList()
		{
			ObservableList<string> list = new ObservableList<string>( ProjectManager.Instance.CustomGlossaryFileListLocations.ToList() );

			list.Insert( 0, Properties.Settings.Default.DefaultGlossaryFileListLocation );
			return list;
		}

		private async void _UpdateStatus( string msg )
		{
			if( string.IsNullOrWhiteSpace( msg ) )
				return;

			if( !this.Dispatcher.CheckAccess() ) {
				await this.Dispatcher.InvokeAsync( () => {
					mStatusMessages.Insert( 0, $"{DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.ff" )}  {msg}" );
					mStatusMessages.CheckMaxAndRemoveExtra();
					CbStatusMessages.SelectedIndex = 0;
				} );
			}
			else {
				mStatusMessages.Insert( 0, $"{DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.ff" )}  {msg}" );
				mStatusMessages.CheckMaxAndRemoveExtra();
				CbStatusMessages.SelectedIndex = 0;
			}
		}

		private async void _SetProjChanged( bool changed = true )
		{
			if( mProject == null )
				return;

			if( changed ) {
				if( mProjChanged )
					return;
				this.Title = $"*{mProject.ProjectName} - Minax Web Translator";
			}
			else {
				if( !mProjChanged )
					return;
				this.Title = $"{mProject.ProjectName} - Minax Web Translator";
			}

			mProjChanged = changed;
			await MessageHub.SendMessageAsync( this, MessageType.ProjectChanged, mProject );
		}

		private void _DumpProj2ModelMappingTable( ProjectModel model )
		{
			if( model == null )
				return;

			var projColl = ProjectManager.Instance.MappingMonitor?.GetMappingCollection( mProjFileName );
			if( projColl != null ) {
				// Althrough we can save MappingMonitor.MappingModel to XML file directly, but it would be filled
				// with many un-necessary runtime data, so ignore extra runtime properties/fields of MappingModel!!
				model.Project.MappingTable = projColl.Select( entry => entry.ToMappingEntry() ).ToList();
			}
			if( model.Project.MappingTable == null )
				model.Project.MappingTable = new List<MappingEntry>();
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

		private async Task<bool> _ReloadAllMappingData( ProjectModel model, bool sendDataReloadMsg = true )
		{
			if( model == null || model.Project == null || model.FullPathFileName == null ||
				model.Project.SourceLanguage == SupportedSourceLanguage.AutoDetect )
				return false;

			// model shall open first
			if( model.IsCurrent == false ) {
				return false;
			}

			// de-subscribe all event of mapping data
			_UnsubscribeAllChangedEvents();

			string xlatorFolder = "Excite";
			if( Profiles.DefaultEngineFolders.ContainsKey( mCurrentRemoteType ) )
				xlatorFolder = Profiles.DefaultEngineFolders[mCurrentRemoteType];

			await ProjectManager.Instance.OpenAndMonitorGlossaryFiles( model, xlatorFolder,
						model.Project.SourceLanguage, model.Project.TargetLanguage );

			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon == null )
				return false;

			if( sendDataReloadMsg )
				await MessageHub.SendMessageAsync( this, MessageType.DataReload, mProject );

			// finally, subscribe MappingMonitor MonitorChanged event for handling file/mapping changed flow
			mon.MonitorDeferredChanged -= MappingMonitor_Changed;
			mon.MonitorDeferredChanged += MappingMonitor_Changed;

			return true;
		}

		private async Task<bool> _RefreshProjectAndGlossaryMappings( bool sendDataReloadMsg = true )
		{
			if( mProject == null )
				return false;

			if( mProjChanged ) {
				if( false == await _CheckAndAskProjSaving( true ) )
					return false;
			}

			ProjectManager.Instance.MappingMonitor?.Stop();

			// reload Mapping Tables
			var ctrl = await this.ShowProgressAsync( Languages.Global.Str0OperationProcessing,
										Languages.ProjectGlossary.Str0PlzWaitReloadMappingTables, true );
			ProjectManager.Instance.MappingMonitor?.ReloadFileList();
			var rst = await _ReloadAllMappingData( mProject, sendDataReloadMsg );
			await ctrl.CloseAsync();
			if( rst ) {
				ShowAutoCloseMessage( Languages.Global.Str0OperationFinished, Languages.ProjectGlossary.Str0ReloadingGlossaryFileSucceed );
			}
			else {
				await this.ShowMessageAsync( Languages.Global.Str0OperationFinished, Languages.ProjectGlossary.Str0ReloadingGlossaryFileFailed );
			}
			return true;
		}

		#endregion


		#region "private manually event handlers"

		private void MsgHub_MessageRecevied( object sender, MessageType type, object data )
		{
			if( sender == this )
				return;

			switch( type ) {
				case MessageType.ProjectChanged:
					_SetProjChanged();
					break;

				case MessageType.XlatorSelected:
					if( data is TranslatorSelector translatorSelector ) {
						mCurrentXlator = translatorSelector;
						if( mCurrentXlator != null )
							mCurrentRemoteType = mCurrentXlator.RemoteType;
					}
					break;

				case MessageType.XlatingProgress:
					if( data is Minax.ProgressInfo pi ) {
						var value = pi.PercentOrErrorCode;
						if( value >= 0 && value <= 100 ) {
							_UpdateStatus( pi.Message );
						}
						else {
							if( string.IsNullOrWhiteSpace( pi.Message ) == false ) {
								_UpdateStatus( string.Format( Languages.WebXlator.Str2TranslatingErrorMessage, value, pi.Message ) );
							}
						}
					}
					break;

				case MessageType.StatusMessage:
					if( data is string statusMsg ) {
						_UpdateStatus( statusMsg );
					}
					break;
			}
		}

		private void MappingModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			_SetProjChanged();

		}

		private void MappingModelTables_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if( sender is ObservableList<MappingMonitor.MappingModel> == false )
				return;

			_SetProjChanged();

			// no need update ListCollectionView in ItemsSource
		}

		private async void MappingMonitor_Changed( object sender, MappingMonitor.MonitorEvents @event, MappingMonitor.MappingEventArgs args )
		{
			if( mProject == null || mProject.Project == null )
				return;

			var mon = ProjectManager.Instance.MappingMonitor;

			switch( @event ) {
				case MappingMonitor.MonitorEvents.FileAll:
					// ignore
					break;

				case MappingMonitor.MonitorEvents.FileDeleted:
					if( args == null || string.IsNullOrWhiteSpace( args.FullPath ) ||
						mon.MonitoringFileList.Contains( args.FullPath ) == false )
						return; // ignore non-monitoring file when it deleted

					Application.Current.Dispatcher.Invoke( (Action)async delegate {
						if( args.FullPath == mProject.FullPathFileName ) {
							_SetProjChanged();
							return; // ignore project file itself
						}

						var rst = MessageDialogResult.Negative;

						if( mAutoMergeWhenFileChanged == false ) {
							rst = await this.ShowMessageAsync( Languages.ProjectGlossary.Str0KeepMappingConfirm,
										string.Format( Languages.ProjectGlossary.Str1GlossaryFileDeletedKeepAsk, args.FullPath ),
										MessageDialogStyle.AffirmativeAndNegative, sMetroDlgYesNoSettings );
						}
						if( rst == MessageDialogResult.Negative ) {
							// remove Mapping from Glossary Tab
							await MessageHub.SendMessageAsync( this, MessageType.GlossaryDeleted, args );

							_UpdateStatus( string.Format( Languages.ProjectGlossary.Str1GlossaryFileRemoved, args.FullPath ) );
						}
					} );
					break;

				case MappingMonitor.MonitorEvents.FileChanged:
					// reload file MappingEntry content
					if( args == null || string.IsNullOrWhiteSpace( args.FullPath ) )
						return;

					Application.Current.Dispatcher.Invoke( (Action)async delegate {
						var isProj = args.FullPath == mProject.FullPathFileName;
						if( mon.MonitoringFileList.Contains( args.FullPath ) ) {
							// the Changed file is Monitoring file, then delete original MappingModel data

							MessageDialogResult rst = MessageDialogResult.Canceled;
							if( args.FullPath == mProject.FullPathFileName ) {
								if( mProjChanged ) {
									rst = await this.ShowMessageAsync( Languages.ProjectGlossary.Str0SaveProjectConfirm,
										string.Format( Languages.ProjectGlossary.Str1OverwriteProjectFileAsk, args.FullPath ),
										MessageDialogStyle.AffirmativeAndNegative, sMetroDlgYesNoSettings );

									if( rst == MessageDialogResult.Affirmative ) {
										await this._CheckAndAskProjSaving( true );
										return;
									}
								}
								else {
									rst = await this.ShowMessageAsync( Languages.ProjectGlossary.Str0UpdateMappingConfirm,
											string.Format( Languages.ProjectGlossary.Str1UpdateProjectFileMappingAsk, args.FullPath ),
											MessageDialogStyle.AffirmativeAndNegative,
											sMetroDlgYesNoSettings );
									if( rst != MessageDialogResult.Affirmative )
										return;

									// update new Project's MappingTable
									// un-subscribe project object events!!!!!!
									var projModels = mon.GetMappingCollection( args.FullPath );
									if( projModels != null ) {
										foreach( var model in projModels ) {
											model.PropertyChanged -= MappingModel_PropertyChanged;
										}
										projModels.CollectionChanged -= MappingModelTables_CollectionChanged;
									}
									mProject.IsCurrent = false;
									mProject.Project.MappingTable?.Clear();
									mProject.Project = null;
									//await _ReloadAllMappingData( mProject );
									if( true == await this._LoadProj( args.FullPath ) ) {
										// update DataGrid
										await MessageHub.SendMessageAsync( this, MessageType.ProjectUpdated, mProject );
									}
								}
							}
							else {
								// an existed Glossary file was updated!
								if( mAutoMergeWhenFileChanged ) {
									rst = MessageDialogResult.Affirmative;
								}
								else {
									rst = await this.ShowMessageAsync( Languages.ProjectGlossary.Str0UpdateMappingConfirm,
										string.Format( Languages.ProjectGlossary.Str1UpdateGlossaryFileMappingAsk, args.FullPath ),
										MessageDialogStyle.AffirmativeAndNegative,
										sMetroDlgYesNoSettings );
								}
								if( rst != MessageDialogResult.Affirmative )
									return;

								await MessageHub.SendMessageAsync( this, MessageType.GlossaryUpdated, args );

								_UpdateStatus( string.Format( Languages.ProjectGlossary.Str1GlossaryFileUpdatedAndMerged, args.FullPath ) );
							}
						}
						else {
							if( false == ProjectManager.Instance.IsFileConcernedByProject(
											mProject, args.FullPath,
											Profiles.DefaultEngineFolders[mCurrentRemoteType] ) )
								return;

							// this file is a whole new Glossary file!! (not the proj. file itself!!)
							var list = ProjectManager.Instance.TryParseAndExtractMappingEntries( args.FullPath );
							if( list == null )
								return;

							var rst = MessageDialogResult.Affirmative;
							if( mAutoMergeWhenFileChanged == false ) {
								rst = await this.ShowMessageAsync( Languages.ProjectGlossary.Str0AddMappingConfirm,
										string.Format( Languages.ProjectGlossary.Str1AddGlossaryFileMappingAsk, args.FullPath ),
										MessageDialogStyle.AffirmativeAndNegative,
										sMetroDlgYesNoSettings );
							}
							if( rst != MessageDialogResult.Affirmative )
								return;

							if( false == mon.AddMonitoring( args.FullPath, list ) )
								return;

							await MessageHub.SendMessageAsync( this, MessageType.GlossaryNew, args );

							_UpdateStatus( string.Format( Languages.ProjectGlossary.Str1GlossaryFileAddedAndMerged, args.FullPath ) );
						}

					} );
					break;

				case MappingMonitor.MonitorEvents.FileCreated:
					// a new Glossary file was created...
					if( false == ProjectManager.Instance.IsFileConcernedByProject(
								mProject, args.FullPath, Profiles.DefaultEngineFolders[mCurrentRemoteType] ) )
						return;

					// this file is a whole new Glossary file!! (not the proj. file itself!!)
					var list2 = ProjectManager.Instance.TryParseAndExtractMappingEntries( args.FullPath );
					if( list2 == null )
						return;

					Application.Current.Dispatcher.Invoke( (Action)async delegate {
						var rst = MessageDialogResult.Affirmative;

						if( mAutoMergeWhenFileChanged == false ) {
							rst = await this.ShowMessageAsync( Languages.ProjectGlossary.Str0AddMappingConfirm,
										string.Format( Languages.ProjectGlossary.Str1AddGlossaryFileMappingAsk, args.FullPath ),
										MessageDialogStyle.AffirmativeAndNegative,
										sMetroDlgYesNoSettings );
						}
						if( rst != MessageDialogResult.Affirmative )
							return;

						if( false == mon.AddMonitoring( args.FullPath, list2 ) )
							return;

						await MessageHub.SendMessageAsync( this, MessageType.GlossaryNew, args );

						_UpdateStatus( string.Format( Languages.ProjectGlossary.Str1GlossaryFileAddedAndMerged, args.FullPath ) );
					} );

					break;

				case MappingMonitor.MonitorEvents.FileRenamed:
					// let MappingMonitor change content, but GUI shall refresh
					if( args.OldFullPath == mProjFileName ) {
						mProjFileName = args.FullPath;
						mProject.FullPathFileName = mProjFileName;
						mProject.FileName = System.IO.Path.GetFileName( mProjFileName );
						if( mProject.Project.MappingTable != null && mProject.Project.MappingTable.Count > 0 &&
							mProject.Project.MappingTable[0] is MappingMonitor.MappingModel m1 &&
							m1.ProjectBasedFileName != mProject.FileName ) {
							foreach( MappingMonitor.MappingModel model in mProject.Project.MappingTable ) {
								model.ProjectBasedFileName = mProject.FileName;
							}
						}
						await MessageHub.SendMessageAsync( this, MessageType.ProjectRenamed, args );
					}
					else
						await MessageHub.SendMessageAsync( this, MessageType.GlossaryRenamed, args );

					_UpdateStatus( string.Format( Languages.Global.Str2OldFileRenamed, args.OldFullPath, args.FullPath ) );
					break;
			}
		}

		private async void MenuItemRecentProject_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			MenuItem mi = sender as MenuItem;
			if( mi == null || mi.DataContext is ProjectModel == false )
				return;

			var model = mi.DataContext as ProjectModel;
			if( string.IsNullOrWhiteSpace( model.FullPathFileName ) )
				return;

			// skip click current project
			if( model.FullPathFileName.Equals( mProjFileName ) )
				return;

			if( false == await _CheckAndAskProjSaving() ) { // cancel action

				return;
			}
			else {
				if( mProject != null && mProject.Project != null ) {
					// restore changed ProjectName to original Project.Name
					mProject.ProjectName = mProject.Project.Name;
				}
				_SetProjChanged( false );
			}

			await _LoadProj( model.FullPathFileName );
		}

		private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
		{
			Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
			e.Handled = true;
		}

		private async void MatsRemeberRecentProjects_CheckedChanged( object sender, RoutedEventArgs e )
		{
			if( MatsRemeberRecentProjects.IsOn == true ) {
				MiProjRecent.Visibility = Visibility.Visible;
				MiProjClearRecent.Visibility = Visibility.Visible;
			}
			else {
				if( ProjectManager.Instance.Projects.Count > 0 ) {
					var rst = await this.ShowMessageAsync( Languages.Global.Str0DeleteConfirm,
											Languages.ProjectGlossary.Str0ClearExistedRecentProjectListAsk,
											MessageDialogStyle.AffirmativeAndNegative );

					if( rst == MessageDialogResult.Affirmative )
						ProjectManager.Instance.ClearRecentProjects();
				}
				MiProjRecent.Visibility = Visibility.Collapsed;
				MiProjClearRecent.Visibility = Visibility.Collapsed;
			}
		}

		private void RtbSrcTgt_ScrollChanged( object sender, ScrollChangedEventArgs e )
		{
			if( mIsScrolling || mTgtPanel.SyncTargetScroll != true ||
				(e.VerticalChange == 0 && e.HorizontalChange == 0) ) {
				e.Handled = true;
				return;
			}

			mIsScrolling = true;

			var rtbSender = sender as RichTextBox;
			RichTextBox rtbSrc = mSrcPanel.RtbSource, rtbTgt = mTgtPanel.RtbTarget;
			var rtbToSync = (sender == rtbTgt) ? rtbSrc : rtbTgt;

			if( mScrollDeferredSyncer != null )
				mScrollDeferredSyncer.Dispose();
			mScrollDeferredSyncer = new Timer( ( sub ) => {
				Dispatcher.BeginInvoke( (Action)delegate {
					if( rtbSender.VerticalOffset <= 1.0 ) {
						rtbToSync.ScrollToVerticalOffset( 0 );
					}
					//else if( rtbToSync.ViewportHeight >= rtbSender.VerticalOffset ) {
					//	rtbToSync.ScrollToVerticalOffset( rtbSender.VerticalOffset );
					//}
					else if( rtbSender.VerticalOffset + rtbSender.ViewportHeight >= rtbSender.ExtentHeight ) {
						rtbToSync.ScrollToEnd();
					}
					else {
						var srcUnit = rtbSrc.ExtentHeight / rtbSrc.LineCount();
						var tgtUnit = rtbTgt.ExtentHeight / rtbTgt.LineCount();

						if( rtbSender == rtbSrc ) {
							rtbToSync.ScrollToVerticalOffset( tgtUnit * rtbSender.VerticalOffset / srcUnit );
						}
						else {
							rtbToSync.ScrollToVerticalOffset( srcUnit * rtbSender.VerticalOffset / tgtUnit );
						}
					}

					if( mScrollDeferredSyncer != null )
						mScrollDeferredSyncer.Dispose();
					mScrollDeferredSyncer = null;

					rtbToSync.InvalidateVisual();
					Task.Delay( 100 ).Wait();
					mIsScrolling = false;
				} );

			}, null, 100, Timeout.Infinite );

		}


		#endregion

		#region "internal module methods"

		internal void ShowAutoCloseMessage( string title, string message, long autoCloseInterval = 4000 )
		{
			FoMessage.AutoCloseInterval = autoCloseInterval;
			LblAutoCloseTitle.Content = title;
			TbAutoCloseMessage.Text = message;

			FoMessage.IsOpen = true;
		}

		private async void _OpenProject()
		{
			if( false == await _CheckAndAskProjSaving() )
				return;

			var prevFn = mProjFileName;
			var dlg = new Microsoft.Win32.OpenFileDialog();
			if( prevFn != null )
				dlg.InitialDirectory = System.IO.Path.GetDirectoryName( prevFn );
			else
				dlg.InitialDirectory = Environment.CurrentDirectory;

			dlg.Title = Languages.ProjectGlossary.Str0SelectExistedProjectFile;
			dlg.AddExtension = true;
			dlg.CheckFileExists = true;
			dlg.DefaultExt = TranslationProject.FileExtension;
			dlg.Filter = Languages.ProjectGlossary.Str0TranslationProjectExt;
			if( dlg.ShowDialog() != true )
				return;

			// try to load Project file
			var rst = await _LoadProj( dlg.FileName );
			if( rst )
				ShowAutoCloseMessage( Languages.Global.Str0OperationSuccessful, string.Format( Languages.ProjectGlossary.Str1OpenProjectSucceed, mProject.ProjectName ) );
			else
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, string.Format( Languages.ProjectGlossary.Str1OpenProjectFailed, dlg.FileName ) );
		}

		private async Task<bool> _LoadProj( string fullPathFileName )
		{
			// close previous project
			_CloseProject();

			// try to load Project file
			var newProj = ProjectManager.Instance.OpenProject( fullPathFileName );
			if( newProj == null ) {
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, string.Format( Languages.ProjectGlossary.Str1OpenProjectSucceed, fullPathFileName ) );
				return false;
			}

			// replace new opened project to current project
			mProject = newProj;
			mProjFileName = fullPathFileName;

			if( false == await _ReloadAllMappingData( mProject ) ) {
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0LoadProjectFilesFailed );
				return false;
			}

			TranslatorHelpers.DescendedModels = ProjectManager.Instance.MappingMonitor.DescendedModels;
			TranslatorHelpers.SourceLanguage = mProject.Project.SourceLanguage;
			TranslatorHelpers.TargetLanguage = mProject.Project.TargetLanguage;

			GbOptionProject.IsEnabled = true;
			TbProjectName.Text = mProject.ProjectName;
			TbGlossaryPath.Text = ProjectManager.Instance.MappingMonitor.GlossaryPath;
			TbProjectDesc.Text = mProject.Project.Description;
			TbProjectRemoteSite.Text = mProject.Project.RemoteSite;

			for( int i = 0; i < CbSourceLang.Items.Count; ++i ) {
				SupportedSourceLanguage srcLang = (SupportedSourceLanguage)((CbSourceLang.Items[i] as ComboBoxItem).Tag);
				if( mProject.Project.SourceLanguage == srcLang ) {
					CbSourceLang.SelectedIndex = i;
					break;
				}
			}
			if( CbSourceLang.SelectedIndex < 0 ) {
				CbSourceLang.SelectedIndex = 0;
			}

			for( int i = 0; i < CbTargetLang.Items.Count; ++i ) {
				SupportedTargetLanguage tgtLang = (SupportedTargetLanguage)((CbTargetLang.Items[i] as ComboBoxItem).Tag);
				if( mProject.Project.TargetLanguage == tgtLang ) {
					CbTargetLang.SelectedIndex = i;
					break;
				}
			}
			if( CbTargetLang.SelectedIndex < 0 ) {
				CbTargetLang.SelectedIndex = 0;
			}

			// all done
			if( MatsRemeberRecentProjects.IsOn == true )
				ProjectManager.Instance.SaveListToSettings();
			mProjChanged = false;
			GdMain.IsEnabled = true;

			await MessageHub.SendMessageAsync( this, MessageType.ProjectOpened, mProject );

			this.Title = $"{mProject.ProjectName} - Minax Web Translator";
			return true;
		}

		private async void _CloseProject()
		{
			if( false == await _CheckAndAskProjSaving() )
				return;

			// unsubscribe all events
			_UnsubscribeAllChangedEvents();

			if( mProject != null ) {
				ProjectManager.Instance.CloseProject( mProject );
			}

			var closedProject = mProject;
			mProject = null;
			mProjFileName = null;
			mProjChanged = false;
			GdMain.IsEnabled = false;
			this.Title = $"Minax Web Translator";

			GbOptionProject.IsEnabled = false;

			// clear closed project's used model list for Summary
			TranslatorHelpers.CurrentUsedModels.Clear();
			await MessageHub.SendMessageAsync( this, MessageType.ProjectClosed, closedProject );
		}

		private async void _SaveProject()
		{
			if( mProject == null )
				return;

			_DumpProj2ModelMappingTable( mProject );

			mProject.Project.Description = TbProjectDesc.Text;
			mProject.Project.RemoteSite = TbProjectRemoteSite.Text;

			if( ProjectManager.Instance.SaveProject( mProject, mProject.FullPathFileName ) == false ) {
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0SaveCurrentProjectFailed );
				return;
			}

			if( MatsRemeberRecentProjects.IsOn == true )
				ProjectManager.Instance.SaveListToSettings();

			_SetProjChanged( false );
			_SaveDockingLayout();

			this.ShowAutoCloseMessage( Languages.Global.Str0OperationSuccessful, Languages.ProjectGlossary.Str0CurrentProjectSaved );
			await MessageHub.SendMessageAsync( this, MessageType.ProjectSaved, mProject );
		}

		private async Task<bool> _CheckAndAskProjSaving( bool forceSaving = false, bool appExit = false )
		{
			if( mProject != null && mProjChanged ) {
				if( forceSaving == false ) {
					if( appExit ) {
						var rst1 = MessageBox.Show( Languages.Global.Str0SaveConfirm,
									Languages.ProjectGlossary.Str0SaveModifiedProjectAsk,
									MessageBoxButton.YesNoCancel, MessageBoxImage.Question );
						if( rst1 == MessageBoxResult.Cancel )
							return false;
						if( rst1 == MessageBoxResult.No )
							return true;
					}
					else {
						// NOTE: DO NOT await MahApps dialog here, otherwise the App shutdown event cannot be captured correctly!!!
						var rst = await this.ShowMessageAsync( Languages.Global.Str0SaveConfirm,
											Languages.ProjectGlossary.Str0SaveModifiedProjectAsk,
											MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
											sMetroDlgYesNoCancelSettings );
						if( rst == MessageDialogResult.FirstAuxiliary ) // Cancel
							return false;
						if( rst == MessageDialogResult.Negative )
							return true;
					}
				}

				// rst == MesssageBoxResult.Yes
				if( mProjFileName == null ) {
					var dlg = new Microsoft.Win32.SaveFileDialog();
					dlg.Title = Languages.ProjectGlossary.Str0SelectTranslationProjectFileName;
					dlg.AddExtension = true;
					dlg.DefaultExt = TranslationProject.FileExtension;
					dlg.Filter = Languages.ProjectGlossary.Str0TranslationProjectExt;
					dlg.CheckFileExists = false;
					if( dlg.ShowDialog() != true )
						return false;
					mProjFileName = dlg.FileName;
				}

				// save current MappingList to Project object's MappingTable
				_DumpProj2ModelMappingTable( mProject );

				mProject.ProjectName = TbProjectName.Text;
				mProject.Project.Name = mProject.ProjectName;
				mProject.Project.SourceLanguage = (SupportedSourceLanguage)((CbSourceLang.SelectedItem as ComboBoxItem).Tag);
				mProject.Project.TargetLanguage = (SupportedTargetLanguage)((CbTargetLang.SelectedItem as ComboBoxItem).Tag);
				mProject.Project.Description = TbProjectDesc.Text;
				mProject.Project.RemoteSite = TbProjectRemoteSite.Text;

				if( ProjectManager.Instance.SaveProject( mProject, mProjFileName ) == false ) {
					this.ShowAutoCloseMessage( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0SaveProjectFailed );
					return false;
				}

				await Task.Delay( 100 ); // useless delay, just for delaying App shutdown event
			}

			if( MatsRemeberRecentProjects.IsOn == true )
				ProjectManager.Instance.SaveListToSettings();
			_SaveAppSettings();
			_SetProjChanged( false );

			return true;
		}


		#endregion

		private async void BtnGlossaryCreateEmptyFolders_Click( object sender, RoutedEventArgs e )
		{
			var result = ProjectManager.Instance.CreateProjectFolders( ProjectManager.Instance.MappingMonitor.BaseProjectPath );
			if( result )
				ShowAutoCloseMessage( Languages.Global.Str0OperationSuccessful, Languages.ProjectGlossary.Str0CreateGlossaryFolderSucceed );
			else
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0CreateGlossaryFolderFailed );
		}

		private async void BtnGlossaryDownloadByFileList_Click( object sender, RoutedEventArgs e )
		{
			Minax.IO.OverwritePolicy policy = Minax.IO.OverwritePolicy.Skip;

			switch( CbGlossaryOverwritePolicy.SelectedIndex ) {
				case 1:
					policy = Minax.IO.OverwritePolicy.ForceOverwriteWithoutAsking;
					break;
				case 2:
					policy = Minax.IO.OverwritePolicy.AlwaysAsking;
					break;
				case 3:
					policy = Minax.IO.OverwritePolicy.FileSizeLarger;
					break;
					//case 4: // NOT SUPPORT!!
					//	policy = Minax.IO.OverwritePolicy.FileDateNew;
					//	break;
			}

			FoOptions.IsOpen = false;
			if( mProject != null && mProjChanged ) {
				if( false == await _CheckAndAskProjSaving() )
					return;
			}

			ProjectManager.Instance.MappingMonitor?.Stop();

			var ctrl = await this.ShowProgressAsync( Languages.Global.Str0OperationProcessing,
						Languages.ProjectGlossary.Str0DownloadingRemoteGlossaryFiles, true );

			var rst = await ProjectManager.Instance.FetchFilesByFileListLink(
					CbGlossarySyncFile.SelectedItem.ToString(),
					ProjectManager.Instance.MappingMonitor.BaseProjectPath, policy,
					mCancelTokenSrource, sTransProgress, this );

			await ctrl.CloseAsync();
			if( rst == false ) {
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0DownloadRemoteGlossaryFilesByLinkFailed );
				ProjectManager.Instance.MappingMonitor?.Start();
				return;
			}

			// reload Mapping Tables
			ctrl = await this.ShowProgressAsync( Languages.Global.Str0OperationProcessing,
							Languages.ProjectGlossary.Str0DownloadGlossaryFilesSucceedWaitReloading, true );
			ProjectManager.Instance.MappingMonitor?.ReloadFileList();
			rst = await _ReloadAllMappingData( mProject );
			await ctrl.CloseAsync();
			if( rst ) {
				ShowAutoCloseMessage( Languages.Global.Str0OperationFinished, Languages.ProjectGlossary.Str0DownloadAndMergeRemoteGlossaryFilesSucceed );
			}
			else {
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0MergeGlossaryFilesFailed );
			}
		}

		private async void BtnGlossarySyncFile_Click( object sender, RoutedEventArgs e )
		{
			var rsDialog = new Views.RemoteSyncFileListSettingsDialog() {
				ParentWindow = this,
			};
			var dialog = (BaseMetroDialog)rsDialog;

			await this.ShowMetroDialogAsync( dialog );
			await dialog.WaitUntilUnloadedAsync();

			// update ComboBox List CbGlossarySyncFile
			// BUG: when await this.xxxDialog.WaitUntilUnloadedAsync(), when it call this.ShowxxxDialogAsync()
			//   MahApps would unload first dialog to show other dialog!!!
			var glossaryFileList = _BuildGlossaryFileList();
			CbGlossarySyncFile.SelectedItem = null;
			CbGlossarySyncFile.ItemsSource = glossaryFileList;
			CbGlossarySyncFile.SelectedIndex = 0;
		}

		private void MatsMonitorAutoMergeWhenFileChanged_Toggled( object sender, EventArgs e )
		{
			mAutoMergeWhenFileChanged = MatsMonitorAutoMergeWhenFileChanged.IsOn == true;
			ProjectManager.Instance.AutoRemoveMonitoringWhenFileChanged = mAutoMergeWhenFileChanged;
		}

		private void MiOptions_Click( object sender, RoutedEventArgs e )
		{
			FoOptions.IsOpen = !FoOptions.IsOpen;
		}

		private async void FoOptions_ClosingFinished( object sender, RoutedEventArgs e )
		{
			if( mProject == null )
				return;

			var changed = false;
			if( mProject.ProjectName != TbProjectName.Text ) {
				if( string.IsNullOrWhiteSpace( TbProjectName.Text ) ) {
					TbProjectName.Text = mProject.ProjectName;
					await this.ShowMessageAsync( Languages.Global.Str0FieldError,
							Languages.ProjectGlossary.Str0ProjectNameFieldCantEmptyOrWhitespaces );
					return;
				}

				mProject.ProjectName = TbProjectName.Text;
				// NOTE: DO NOT set Project.Name here for recovery original Project.Name to mProject.ProjectName when cancel saving
				//if( mProject.Project != null )
				//	mProject.Project.Name = mProject.ProjectName;

				changed = true;
			}

			var langChanged = false;
			if( (CbSourceLang.SelectedItem as ComboBoxItem).Tag is SupportedSourceLanguage srcLang ) {
				if( mProject.Project.SourceLanguage != srcLang ) {
					mProject.Project.SourceLanguage = srcLang;
					langChanged = true;
				}
			}

			if( (CbTargetLang.SelectedItem as ComboBoxItem).Tag is SupportedTargetLanguage tgtLang ) {
				if( mProject.Project.TargetLanguage != tgtLang ) {
					mProject.Project.TargetLanguage = tgtLang;
					langChanged = true;
				}
			}

			if( mProject.Project.Description != TbProjectDesc.Text ) {
				mProject.Project.Description = TbProjectDesc.Text;
				changed = true;
			}
			if( mProject.Project.RemoteSite != TbProjectRemoteSite.Text ) {
				mProject.Project.RemoteSite = TbProjectRemoteSite.Text;
				changed = true;
			}

			if( langChanged ) {
				TranslatorHelpers.SourceLanguage = mProject.Project.SourceLanguage;
				TranslatorHelpers.TargetLanguage = mProject.Project.TargetLanguage;

				_SetProjChanged();
				await _RefreshProjectAndGlossaryMappings();
			}
			else if( changed ) {
				_SetProjChanged();
			}

			// update source/target panels font settings
			if( Properties.Settings.Default.SourceTextAreaFontSizeMin != ManudSourceTextAreaFontSizeMin.Value.GetValueOrDefault() ||
				Properties.Settings.Default.TargetTextAreaFontSizeMin != ManudTargetTextAreaFontSizeMin.Value.GetValueOrDefault() ||
				Properties.Settings.Default.SourceTextAreaFontFamily != mSysFontGlyphsSrc[CbSourceTextAreaFont.SelectedIndex].FontFamily.Source ||
				Properties.Settings.Default.TargetTextAreaFontFamily != mSysFontGlyphsTgt[CbTargetTextAreaFont.SelectedIndex].FontFamily.Source) {
				_SaveAppSettings();
				_RestoreAppSettings();
			}

		}

		private void MiLayoutRestoreDefault_Click( object sender, RoutedEventArgs e )
		{
			//var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			//using( var stream = new System.IO.MemoryStream( Properties.Resources.DefaultAvalonDockLayout ) )
			//	serializer.Deserialize( stream );
			_RestoreDefaultDockingLayout();
		}

		private void MiLayoutSaveToSettings_Click( object sender, RoutedEventArgs e )
		{
			// first force save current docking state to App settings
			_SaveDockingLayout();

			// then, ask want to save as a .adconf file?
			var dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.Title = "Do you want to save AvalonDocking Conf. file?";
			dlg.AddExtension = true;
			dlg.DefaultExt = ".adconf";
			dlg.Filter = "AvalonDocking Conf. File(*.adconf)|*.adconf";
			dlg.CheckFileExists = false;
			if( dlg.ShowDialog() != true )
				return;

			var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
			serializer.Serialize( dlg.FileName );
		}

		private void MiLayoutRestoreFromSettings_Click( object sender, RoutedEventArgs e )
		{
			// ask if want to restore from .adconf file
			var dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Title = "Do you want to restore AvalonDocking Conf. from file?";
			dlg.AddExtension = true;
			dlg.DefaultExt = ".adconf";
			dlg.Filter = "AvalonDocking Conf. File(*.adconf)|*.adconf";
			dlg.CheckFileExists = true;
			if( dlg.ShowDialog() == true ) {
				var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer( dockManager );
				serializer.Deserialize( dlg.FileName );
				return;
			}

			// otherwise, load from App settings.
			_RestoreDockingLayout();
		}

		private async void MiExit_Click( object sender, RoutedEventArgs e )
		{
			// check project changed status
			if( false == await _CheckAndAskProjSaving() ) {
				return;
			}

			mProjChanged = false;
			this.Close();
		}

		private async void MiProjClearRecent_Click( object sender, RoutedEventArgs e )
		{
			var result = await this.ShowMessageAsync( Languages.Global.Str0DeleteConfirm,
										Languages.ProjectGlossary.Str0ClearExistedRecentProjectListAsk,
										MessageDialogStyle.AffirmativeAndNegative, sMetroDlgYesNoSettings );
			if( result != MessageDialogResult.Affirmative )
				return;

			// clear binding list
			ProjectManager.Instance.ClearRecentProjects();
		}

		private async void MiProjectNew_Click( object sender, RoutedEventArgs e )
		{
			var prevFn = mProjFileName;
			var dlg = new Microsoft.Win32.SaveFileDialog();
			if( prevFn != null )
				dlg.InitialDirectory = System.IO.Path.GetDirectoryName( prevFn );
			else
				dlg.InitialDirectory = Environment.CurrentDirectory;

			dlg.Title = Languages.ProjectGlossary.Str0SelectTranslationProjectFileName;
			dlg.AddExtension = true;
			dlg.DefaultExt = TranslationProject.FileExtension;
			dlg.Filter = Languages.ProjectGlossary.Str0TranslationProjectExt;
			dlg.CheckFileExists = false;
			if( dlg.ShowDialog() != true )
				return;

			var rst = await _LoadProj( dlg.FileName );
			if( rst )
				ShowAutoCloseMessage( Languages.Global.Str0OperationSuccessful, string.Format( Languages.ProjectGlossary.Str1CreateProjectSucceed, mProject.ProjectName ) );
			else
				await this.ShowMessageAsync( Languages.Global.Str0OperationFailed, string.Format( Languages.ProjectGlossary.Str1CreateProjectFailed, dlg.FileName ) );
		}
		private void MiProjectOpen_Click( object sender, RoutedEventArgs e )
		{
			_OpenProject();
		}

		private void MiProjectSave_Click( object sender, RoutedEventArgs e )
		{
			_SaveProject();
		}

		private void MiProjectClose_Click( object sender, RoutedEventArgs e )
		{
			_CloseProject();
		}

		private void MiTranslatorSelector_Click( object sender, RoutedEventArgs e )
		{
			if( FoTranslator.IsOpen ) {
				FoTranslator.IsOpen = false;
			}
			else {
				mXlatorSelectorPanel.ReloadSettings();
				FoTranslator.IsOpen = true;
			}
		}
		private async void FoTranslator_ClosingFinished( object sender, RoutedEventArgs e )
		{
			// update and SendMessage when Translator changed
			if( CurrentXlator != mXlatorSelectorPanel.SelectedTranslator ) {
				mCurrentRemoteType = mXlatorSelectorPanel.SelectedTranslator.RemoteType;
				await _RefreshProjectAndGlossaryMappings( false );
			}
			CurrentXlator = mXlatorSelectorPanel.SelectedTranslator;
		}

		private void MiAbout_Click( object sender, RoutedEventArgs e )
		{
			if( mAboutPanel == null ) {
				mAboutPanel = new Views.AboutCreditsPanel( this );
				FoAbout.Content = mAboutPanel;
			}

			FoAbout.IsOpen = !FoAbout.IsOpen;
		}

	}
}
