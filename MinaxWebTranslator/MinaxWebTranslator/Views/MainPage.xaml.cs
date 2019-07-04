using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MinaxWebTranslator.Models;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Translation;
using System.Threading;
using System.Linq;
using System.IO;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;
using Plugin.Toast;

namespace MinaxWebTranslator.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : MasterDetailPage
    {
		public string PublicDocumentsPath { get; private set; }
		internal bool NeedSave { get; private set; } // for App.OnSleep()

		internal TranslatorSelector CurrentXlator {
			get => mCurrentXlator;
			private set {
				if( mCurrentXlator == value )
					return;

				mCurrentXlator = value;
				mCurrentRemoteTranslator = mCurrentXlator.RemoteType;

				// tell other pages/panels the xlator has been changed
				_ = MessageHub.SendMessageAsync( this, MessageType.XlatorSelected, mCurrentXlator );
			}
		}

		public MainPage( string publicDocuemntPath = null )
        {
			PublicDocumentsPath = publicDocuemntPath;

            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;

            MenuPages.Add( MenuItemType.Main, (NavigationPage)Detail );

			//mMenuPage = Master as MenuPage;
			mDetailPage = ((NavigationPage)Detail).RootPage as DetailPage;

			// alloc. some important ContentPages
			mXlatorPage = new TranslatorSelectorPage();
			mDetailPage.CurrentTranslator = mXlatorPage.SelectedTranslator;
			mProjSettingsPage = new ProjectSettingsPage( this );
			mDetailPage.IsEnabled = false;

			CurrentXlator = mXlatorPage.SelectedTranslator;

			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
			MessageHub.MessageReceived += MsgHub_MessageRecevied;
        }
		~MainPage()
		{
			MessageHub.MessageReceived -= MsgHub_MessageRecevied;
		}

		#region "private data"

		private Dictionary<MenuItemType, NavigationPage> MenuPages = new Dictionary<MenuItemType, NavigationPage>();
		private ProjectModel mProject;
		private string mProjFileName;
		private bool mProjChanged, mAutoMergeWhenFileChanged;
		private TranslatorSelector mCurrentXlator = null;
		private RemoteType mCurrentRemoteTranslator = RemoteType.Excite;
		private CancellationTokenSource mCancelTokenSrource = new CancellationTokenSource();
		// GUI Progress Indicator
		private static readonly Progress<Minax.ProgressInfo> sTransProgress = new Progress<Minax.ProgressInfo>();

		// some important ContentPages
		//private MenuPage mMenuPage = null;
		private DetailPage mDetailPage = null;
		private TranslatorSelectorPage mXlatorPage = null;
		private RecentProjectsPage mRecentPage = null;
		private ProjectSettingsPage mProjSettingsPage = null;

		#endregion // "private data"

		// for OnSleep()
		internal async void SaveAll()
		{
			if( mProject != null && mProjFileName != null )
				ProjectManager.Instance.SaveProject( mProject, mProjFileName );

			ProjectManager.Instance.SaveListToSettings();
			_SaveAppSettings();
			_SetProjChanged( false );
			await MessageHub.SendMessageAsync( this, MessageType.ProjectSaved, null );
		}

		internal async Task NavigateFromMenu( MenuItemType id )
		{
			IsPresented = false; // close the flyout menu

			if( !MenuPages.ContainsKey( id ) ) {
				switch( id ) {
					case MenuItemType.Others:
						MenuPages.Add( id, new NavigationPage( new DetailOthersPage() ) {
							Title = Languages.Global.Str0OthersAbout
						} );
						break;

					case MenuItemType.RecentProjectRequest:
						mRecentPage = new RecentProjectsPage( this );
						MenuPages.Add( id, new NavigationPage( mRecentPage ) { Title = Languages.ProjectGlossary.Str0RecentProjects } );
						break;
					case MenuItemType.RecentProjectSelected:
						if( MenuPages.ContainsKey( MenuItemType.RecentProjectRequest ) == false ||
							((NavigationPage)Detail).CurrentPage != MenuPages[MenuItemType.RecentProjectRequest] )
							break;

						if( false == await _CheckAndAskProjSaving() ) {
							// cancel selected
							mRecentPage.SelectedProject = mProject;
							return;
						} else {
							if( mProject != null && mProject.Project != null ) {
								// restore changed ProjectName to original Project.Name
								mProject.ProjectName = mProject.Project.Name;
							}
							_SetProjChanged( false );
						}

						if( mProject != null )
							mProject.PropertyChanged -= ProjectModel_PropertyChanged;

						if( mRecentPage != null ) {
							var projModel = mRecentPage.SelectedProject;
							if( projModel == null )
								break;

							var rst = await _LoadProj( projModel.FullPathFileName );
						}

						if( mProject != null ) {
							ProjectManager.Instance.SaveListToSettings();
						}

						// POP the Recent Project NavigationPage which calling RecentProjectRequest
						await this.Detail.Navigation.PopAsync();
						return;

					case MenuItemType.TranslatorSelectorRequest:
						MenuPages.Add( id, new NavigationPage( mXlatorPage ) { Title = Languages.WebXlator.Str0XlatorSelector } );
						break;
					case MenuItemType.TranslatorSelectorClosed:
						if( CurrentXlator != mXlatorPage.SelectedTranslator ) {
							mCurrentRemoteTranslator = mXlatorPage.SelectedTranslatorType;
							await _RefreshProjectAndGlossaryMappings( true );
						}

						CurrentXlator = mXlatorPage.SelectedTranslator;
						return;

					case MenuItemType.ProjectNew:
						// TODO: find or build a suitable SaveFileDialog for Xamarin.Forms!!
						return;
					case MenuItemType.ProjectOpen:
						_OpenProject();
						return;
					case MenuItemType.ProjectSave:
						_SaveProject();
						return;
					case MenuItemType.ProjectClose:
						// notify MenuPage to collapse Save/Close/Setting items
						_CloseProject( true );
						return;

					case MenuItemType.ProjectSettingsRequest:
						mProjSettingsPage.CurrentProject = mProject;
						MenuPages.Add( id, new NavigationPage( mProjSettingsPage ) { Title = Languages.ProjectGlossary.Str0ProjectAndAppSettings } );
						break;
					case MenuItemType.ProjectSettingsClosed:
						if( mProjSettingsPage.NeedReloading ) {
							// when glossary file downloaded from remote site...
							ProjectManager.Instance.SaveProject( mProject, mProjFileName );
							await MessageHub.SendMessageAsync( this, MessageType.ProjectSaved, mProject );
							// perform reloading Mapping and Project
							await _SetupByProject();

							// trigger DetailPage to reload all mapping methods
							await MessageHub.SendMessageAsync( this, MessageType.DataReload, mProject );
						} else {
							var changed = false;
							if( mProject.ProjectName != mProjSettingsPage.CurrentProjectName ) {
								if( string.IsNullOrWhiteSpace( mProjSettingsPage.CurrentProjectName ) ) {
									await DisplayAlert( Languages.Global.Str0FieldError,
										Languages.ProjectGlossary.Str0ProjectNameFieldCantEmptyOrWhitespaces, Languages.Global.Str0Ok );
									mProjSettingsPage.CurrentProject = mProject;
									return;
								}

								mProject.ProjectName = mProjSettingsPage.CurrentProjectName;
								// NOTE: DO NOT set Project.Name here for recovery original Project.Name to mProject.ProjectName when cancel saving
								//if( mProject.Project != null )
								//	mProject.Project.Name = mProject.ProjectName;

								changed = true;
							}

							var langChanged = false;
							if( mProject.Project.SourceLanguage != mProjSettingsPage.CurrentSourceLanguage ) {
								mProject.Project.SourceLanguage = mProjSettingsPage.CurrentSourceLanguage;
								langChanged = true;
							}
							if( mProject.Project.TargetLanguage != mProjSettingsPage.CurrentTargetLanguage ) {
								mProject.Project.TargetLanguage = mProjSettingsPage.CurrentTargetLanguage;
								langChanged = true;
							}

							if( mProject.Project.Description != mProjSettingsPage.CurrentProjectDescription ) {
								mProject.Project.Description = mProjSettingsPage.CurrentProjectDescription;
								changed = true;
							}
							if( mProject.Project.RemoteSite != mProjSettingsPage.CurrentProjectRemoteSite ) {
								mProject.Project.RemoteSite = mProjSettingsPage.CurrentProjectRemoteSite;
								changed = true;
							}

							if( langChanged ) {
								TranslatorHelpers.SourceLanguage = mProject.Project.SourceLanguage;
								TranslatorHelpers.TargetLanguage = mProject.Project.TargetLanguage;
								_SetProjChanged();
								await _RefreshProjectAndGlossaryMappings();
							} else if( changed ) {
								_SetProjChanged();
							}
						}

						mProjSettingsPage.NeedReloading = false;
						return;
				}
			}

			// each NavigationPage in MenuPages dict. would be pushed to Detail.Navigation.NavigationStack,
			// not replace the DetailPage
			if( MenuPages.ContainsKey( id ) ) {
				NavigationPage newPage = MenuPages[id];
				if( Detail == newPage )
					return;

				foreach( var ng in Detail.Navigation.NavigationStack ) {
					if( ng == newPage )
						return;
				}

				await Detail.Navigation.PushAsync( newPage, true );
			}
		}

		private void _RestoreAppSettings()
		{

		}

		private void _SaveAppSettings()
		{
			Properties.Settings.Default.Save();
		}

		private async void _OpenProject()
		{
			if( false == await _CheckAndAskProjSaving() )
				return;

			try {
				FileData fileData = await CrossFilePicker.Current.PickFile();
				if( fileData == null )
					return;

				var fullPathFileName = Uri.UnescapeDataString( fileData.FilePath );
				var data = fileData.DataArray;
				string fileName = fileData.FileName;
				TranslationProject projObj = null;
				using( var ms = new System.IO.MemoryStream( data ) )
					projObj = Minax.Utils.DeserializeFromXml<TranslationProject>( ms );

				if( projObj == null ) {
					await DisplayAlert( Languages.Global.Str0PlzSelectAgain,
							string.Format( Languages.ProjectGlossary.Str1SelectedFileInvalid, fullPathFileName ),
							Languages.Global.Str0Confirm );
					return;
				}

				// check file source
				var projFileName = fullPathFileName;
				if( fullPathFileName.StartsWith( @"content://com.google.android.apps.docs.storage" ) ||
					File.Exists( fullPathFileName ) == false ) {
					// selected from Google Drive app, so try to store it to a common public document folder
					string docPath = System.Environment.GetFolderPath( Environment.SpecialFolder.CommonDocuments );
					if( string.IsNullOrWhiteSpace( docPath ) && string.IsNullOrWhiteSpace( PublicDocumentsPath ) == false )
						docPath = PublicDocumentsPath;

					var projPath = Path.Combine( docPath, "MinaxWebTranslator" );
					projFileName = Path.Combine( projPath, fileData.FileName );
					bool rst = false;
					rst = await DisplayAlert( Languages.Global.Str0SaveAsConfirm,
							string.Format( Languages.Global.Str2SelectedFileSaveToPublicFolderAsk, fileName, projFileName ),
							Languages.Global.Str0Yes, Languages.Global.Str0Cancel );
					if( rst != true )
						return;

					if( File.Exists( projFileName ) ) {
						rst = await DisplayAlert( Languages.Global.Str0OverwriteConfirm,
								string.Format( Languages.ProjectGlossary.Str1OverwriteProjectFileInPublicFolderAsk, projFileName),
								Languages.Global.Str0Yes, Languages.Global.Str0No );
						if( rst != true )
							return;

						File.Delete( projFileName );
					}

					if( Directory.Exists( projPath ) == false ) {
						Directory.CreateDirectory( projPath );
					}

					if( false == TranslationProject.SerializeToXml( projObj, projFileName ) ) {
						await DisplayAlert( Languages.Global.Str0OperationFailed,
								string.Format( Languages.ProjectGlossary.Str1SaveProjectFileFailed, projFileName),
								Languages.Global.Str0Ok );
						return;
					}
				}

				_CloseProject();

				// manually add PorjectModel instance to ProjectManager
				mProject = ProjectManager.Instance.AddProject( projFileName, projObj );
				if( mProject != null ) {
					ProjectManager.Instance.MarkAsCurrent( mProject );
					ProjectManager.Instance.SaveListToSettings();
					mProjFileName = projFileName;
				}

				// done, update binding and menu
				await _SetupByProject();
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.WriteLine( "MainPage._OpenProject() got exception, EX: " + ex.Message );
			}
		}

		private async Task<bool> _LoadProj( string fullPathFileName )
		{
			// close previous project
			_CloseProject();

			var newProj = ProjectManager.Instance.OpenProject( fullPathFileName );
			if( newProj == null ) {
				await DisplayAlert( Languages.Global.Str0OperationFailed, Languages.ProjectGlossary.Str0OpenProjectFailed, Languages.Global.Str0Ok );
				return false;
			}

			// replace new opened project to current project
			mProject = newProj;
			mProjFileName = fullPathFileName;

			await _SetupByProject();
			return true;
		}

		private async Task<bool> _SetupByProject( bool sendDataReloadMsg = false )
		{
			if( mProject == null )
				return false;

			mProjFileName = mProject.FullPathFileName;

			// de-subscribe all event of mapping data
			_UnsubscribeAllChangedEvents();

			string xlatorFolder = "Excite";
			if( Profiles.DefaultEngineFolders.ContainsKey( mXlatorPage.SelectedTranslatorType ) )
				xlatorFolder = Profiles.DefaultEngineFolders[mXlatorPage.SelectedTranslatorType];

			await ProjectManager.Instance.OpenAndMonitorGlossaryFiles( mProject, xlatorFolder,
						mProject.Project.SourceLanguage, mProject.Project.TargetLanguage );

			var mon = ProjectManager.Instance.MappingMonitor;
			if( mon != null ) {
				foreach( var fn in mon.MonitoringFileList ) {
					var list = mon.GetMappingCollection( fn );
					if( list != null )
						_SubscribeChangedEvents( list );
				}

				// subscribe MappingMonitor MonitorChanged event for handling file/mapping changed flow
				mon.MonitorDeferredChanged -= MappingMonitor_Changed;
				mon.MonitorDeferredChanged += MappingMonitor_Changed;

				TranslatorHelpers.DescendedModels = mon.DescendedModels;
			}
			
			TranslatorHelpers.SourceLanguage = mProject.Project.SourceLanguage;
			TranslatorHelpers.TargetLanguage = mProject.Project.TargetLanguage;

			this.Title = mProject.Project.Name;
			mProjSettingsPage.CurrentProject = mProject;

			await MessageHub.SendMessageAsync( this, MessageType.ProjectOpened, mProject );
			if( sendDataReloadMsg )
				await MessageHub.SendMessageAsync( this, MessageType.DataReload, mProject );


			mDetailPage.IsEnabled = true;
			return true;
		}

		private async void _CloseProject( bool sendMessage = false )
		{
			if( false == await _CheckAndAskProjSaving() )
				return;

			if( mProject == null )
				return;

			_UnsubscribeAllChangedEvents();
			if( ProjectManager.Instance.MappingMonitor != null ) {
				// un-subscribe MappingMonitor MonitorChanged event
				ProjectManager.Instance.MappingMonitor.MonitorDeferredChanged -= MappingMonitor_Changed;
			}

			_SetProjChanged( false );

			ProjectManager.Instance.CloseProject( mProject );
			var closedProj = mProject;

			mProject = null;
			mProjFileName = null;

			mDetailPage.IsEnabled = false;
			if( sendMessage )
				await MessageHub.SendMessageAsync( this, MessageType.ProjectClosed, closedProj );
		}

		private async void _SaveProject()
		{
			if( mProject == null )
				return;

			_DumpProjModelMappingTable( mProject );

			if( ProjectManager.Instance.SaveProject( mProject, mProject.FullPathFileName ) == false ) {
				await DisplayAlert( Languages.Global.Str0OperationFailed,
						string.Format(Languages.ProjectGlossary.Str1SaveProjectFileFailed, mProject.ProjectName),
						Languages.Global.Str0Ok );
				return;
			}

			_SetProjChanged( false );

			CrossToastPopUp.Current.ShowToastMessage( Languages.ProjectGlossary.Str0CurrentProjectSaved );
			await MessageHub.SendMessageAsync( this, MessageType.ProjectSaved, mProject );
		}


		private void _SubscribeChangedEvents( ObservableList<MappingMonitor.MappingModel> list )
		{
			if( list == null || list.Count <= 0 )
				return;

			foreach( var model in list ) {
				model.PropertyChanged -= MappingModel_PropertyChanged;
				model.PropertyChanged += MappingModel_PropertyChanged;
			}

			list.CollectionChanged -= MappingModelTables_CollectionChanged;
			list.CollectionChanged += MappingModelTables_CollectionChanged;
		}

		private void _UnsubscribeAllChangedEvents()
		{
			if( mProject != null )
				mProject.PropertyChanged -= ProjectModel_PropertyChanged;

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

		private async void _SetProjChanged( bool changed = true )
		{
			if( mProject == null )
				return;

			mProjChanged = changed;
			NeedSave = changed;

			if( changed ) {
				this.Title = $"{mProject.ProjectName}* - Minax Web Translator";
			}
			else {
				this.Title = $"{mProject.ProjectName} - Minax Web Translator";
			}

			if( changed )
				await MessageHub.SendMessageAsync( this, MessageType.ProjectChanged, mProject );
		}

		private async Task<bool> _CheckAndAskProjSaving( bool forceSaving = false )
		{
			if( mProject != null && mProjChanged ) {
				if( forceSaving == false ) {
					var actions = await DisplayActionSheet( Languages.ProjectGlossary.Str0SaveModifiedProjectAsk,
											Languages.Global.Str0Cancel, null,
											Languages.Global.Str0Yes, Languages.Global.Str0No );
					if( actions == Languages.Global.Str0Cancel )
						return false;
					if( actions == Languages.Global.Str0No )
						return true;
				}

				if( mProjFileName == null ) {
					// TODO: show SaveFileDialog to get a new file name
				}

				// save current MappingList to Project object's MappingTable
				_DumpProjModelMappingTable( mProject );

				if( ProjectManager.Instance.SaveProject( mProject, mProjFileName ) == false ) {
					await DisplayAlert( Languages.Global.Str0OperationFailed,
							string.Format( Languages.ProjectGlossary.Str1SaveProjectFileFailed, mProjFileName ),
							Languages.Global.Str0Ok );
					return false;
				}
			}

			ProjectManager.Instance.SaveListToSettings();
			_SaveAppSettings();
			_SetProjChanged( false );

			return true;
		}

		private void _DumpProjModelMappingTable( ProjectModel model )
		{
			if( model == null || model.Project == null )
				return;

			var list = ProjectManager.Instance.MappingMonitor?.GetMappingCollection( model.FullPathFileName );
			if( list != null ) {
				model.Project.MappingTable = list.Select( entry => entry.ToMappingEntry() ).ToList();
			}
			if( model.Project.MappingTable == null )
				model.Project.MappingTable = new List<MappingEntry>();
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
			ProjectManager.Instance.MappingMonitor?.ReloadFileList();
			await _SetupByProject( sendDataReloadMsg );

			return true;
		}

		#region "private manually event handlers"

		private async void MsgHub_MessageRecevied( object sender, MessageType type, object data )
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
							mCurrentRemoteTranslator = mCurrentXlator.RemoteType;
					}
					break;

				case MessageType.MenuNavigate:
					if( data is MenuItemType mit ) {
						await NavigateFromMenu( mit );
					}
					break;
			}
		}

		// NOT SUPPORT in Xamarin.Forms (shall write platform-depend code...)
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

					Device.BeginInvokeOnMainThread( (Action)async delegate {
						if( args.FullPath == mProject.FullPathFileName ) {
							_SetProjChanged();
							return; // ignore project file itself
						}

						var rst = false;

						if( mAutoMergeWhenFileChanged == false ) {
							rst = await DisplayAlert( Languages.ProjectGlossary.Str0KeepMappingConfirm,
										string.Format( Languages.ProjectGlossary.Str1GlossaryFileDeletedKeepAsk, args.FullPath),
										Languages.Global.Str0Yes, Languages.Global.Str0No );
						}
						if( rst == false ) {
							// remove Mapping from Glossary Tab
							await MessageHub.SendMessageAsync( this, MessageType.GlossaryDeleted, args );
						}
					} );
					break;

				case MappingMonitor.MonitorEvents.FileChanged:
					// reload file MappingEntry content
					if( args == null || string.IsNullOrWhiteSpace( args.FullPath ) )
						return;

					Device.BeginInvokeOnMainThread( (Action)async delegate {
						var isProj = args.FullPath == mProject.FullPathFileName;
						if( mon.MonitoringFileList.Contains( args.FullPath ) ) {
							// the Changed file is Monitoring file, then delete original MappingModel data

							var rst = false;
							if( args.FullPath == mProject.FullPathFileName ) {
								if( mProjChanged ) {
									rst = await DisplayAlert( Languages.ProjectGlossary.Str0SaveProjectConfirm,
												string.Format( Languages.ProjectGlossary.Str1UpdateGlossaryFileMappingAsk, args.FullPath ),
												Languages.Global.Str0Yes, Languages.Global.Str0No );

									if( rst == true ) {
										await this._CheckAndAskProjSaving( true );
										return;
									}
								}
								else {
									rst = await DisplayAlert( Languages.ProjectGlossary.Str0UpdateMappingConfirm,
												string.Format( Languages.ProjectGlossary.Str1UpdateProjectFileMappingAsk, args.FullPath ),
												Languages.Global.Str0Yes, Languages.Global.Str0No );
									if( rst != true )
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
									if( true == await this._LoadProj( args.FullPath ) ) {
										// update DataGrid/ListView
										await MessageHub.SendMessageAsync( this, MessageType.ProjectUpdated, mProject );
									}
								}
							}
							else {
								// an existed Glossary file was updated!
								if( mAutoMergeWhenFileChanged ) {
									rst = true;
								}
								else {
									rst = await DisplayAlert( Languages.ProjectGlossary.Str0UpdateMappingConfirm,
												string.Format( Languages.ProjectGlossary.Str1UpdateGlossaryFileMappingAsk, args.FullPath ),
												Languages.Global.Str0Yes, Languages.Global.Str0No );
								}
								if( rst != true )
									return;

								await MessageHub.SendMessageAsync( this, MessageType.GlossaryUpdated, args );

							}
						}
						else {
							if( false == ProjectManager.Instance.IsFileConcernedByProject(
											mProject, args.FullPath,
											Profiles.DefaultEngineFolders[mCurrentRemoteTranslator] ) )
								return;

							// this file is a whole new Glossary file!! (not the proj. file itself!!)
							var list = ProjectManager.Instance.TryParseAndExtractMappingEntries( args.FullPath );
							if( list == null )
								return;

							//var rst = MessageDialogResult.Affirmative;
							var rst = true;
							if( mAutoMergeWhenFileChanged == false ) {
								rst = await DisplayAlert( Languages.ProjectGlossary.Str0AddMappingConfirm,
											string.Format( Languages.ProjectGlossary.Str1AddGlossaryFileMappingAsk, args.FullPath ),
											Languages.Global.Str0Yes, Languages.Global.Str0No );
							}
							if( rst != true )
								return;

							if( false == mon.AddMonitoring( args.FullPath, list ) )
								return;

							await MessageHub.SendMessageAsync( this, MessageType.GlossaryNew, args );
						}

					} );
					break;

				case MappingMonitor.MonitorEvents.FileCreated:
					// a new Glossary file was created...
					if( false == ProjectManager.Instance.IsFileConcernedByProject(
								mProject, args.FullPath, Profiles.DefaultEngineFolders[mCurrentRemoteTranslator] ) )
						return;

					// this file is a whole new Glossary file!! (not the proj. file itself!!)
					var list2 = ProjectManager.Instance.TryParseAndExtractMappingEntries( args.FullPath );
					if( list2 == null )
						return;

					Device.BeginInvokeOnMainThread( async () => {
						//var rst = MessageDialogResult.Affirmative;
						var rst = true;

						if( mAutoMergeWhenFileChanged == false ) {
							rst = await DisplayAlert( Languages.ProjectGlossary.Str0AddMappingConfirm,
											string.Format( Languages.ProjectGlossary.Str1AddGlossaryFileMappingAsk, args.FullPath ),
											Languages.Global.Str0Yes, Languages.Global.Str0No );
						}
						if( rst != true )
							return;

						if( false == mon.AddMonitoring( args.FullPath, list2 ) )
							return;

						await MessageHub.SendMessageAsync( this, MessageType.GlossaryNew, args );
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

					//_UpdateStatus( $"Old file \"{args.OldFullPath}\" has been renamed to \"{args.FullPath}\"" );
					break;
			}
		}


		private void ProjectModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			_SetProjChanged();
		}

		private void MappingModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			_SetProjChanged();
		}

		private void MappingModelTables_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			_SetProjChanged();
		}

		#endregion // "private manually event handlers"


		private void MasterDetailPage_Appearing( object sender, EventArgs e )
		{

		}

		private void MasterDetailPage_Disappearing( object sender, EventArgs e )
		{
			ProjectManager.Instance.SaveListToSettings();
		}
	}
}
