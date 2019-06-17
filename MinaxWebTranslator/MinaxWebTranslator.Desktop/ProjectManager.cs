using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.IO;
using Minax.Web;
using Minax.Web.Translation;
using MinaxWebTranslator.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Minax.Domain.Translation.SupportedLanguagesExtensions;

namespace MinaxWebTranslator.Desktop
{
	internal class ProjectManager
	{
		public static string ProjectSeparatorInSetting => "⇭⍢";

		public static ProjectManager Instance { get; } = new ProjectManager();

		public ReadOnlyObservableList<ProjectModel> Projects => mProjects;

		public ProjectModel CurrentProject { get; private set; }

		public MappingMonitor MappingMonitor => mMonitor;

		public bool AutoRemoveMonitoringWhenFileChanged {
			get => autoRemoveMonitoringWhenFileChanged;
			set {
				if( autoRemoveMonitoringWhenFileChanged == value )
					return;

				autoRemoveMonitoringWhenFileChanged = value;
				if( mMonitor != null )
					mMonitor.AutoRemoveMonitoringWhenFileChanged = autoRemoveMonitoringWhenFileChanged;
			}
		}

		public ObservableList<string> CustomGlossaryFileListLocations { get; private set; } = new ObservableList<string>();


		#region "private/internal data/helpers"

		private static readonly ObservableCollection<ProjectModel> sProjects = new ObservableCollection<ProjectModel>();
		private ReadOnlyObservableList<ProjectModel> mProjects;
		private Minax.Domain.Translation.MappingMonitor mMonitor = null;
		private bool autoRemoveMonitoringWhenFileChanged = false;

		static ProjectManager()
		{
			Instance.mProjects = new ReadOnlyObservableList<ProjectModel>( sProjects );
		}

		~ProjectManager()
		{
			if( mMonitor != null ) {
				mMonitor.Stop();
				mMonitor.ClearAllMonitoring();
				mMonitor.Dispose();
			}
			mMonitor = null;
		}

		private void _CheckSettings()
		{
			if( Properties.Settings.Default.RecentProjects == null )
				Properties.Settings.Default.RecentProjects = new System.Collections.Specialized.StringCollection();
			if( Properties.Settings.Default.CustomGlossaryFileListLocations == null )
				Properties.Settings.Default.CustomGlossaryFileListLocations = new StringCollection();
		}

		#endregion


		public bool RestoreListFromSettings()
		{
			sProjects.Clear();
			_CheckSettings();

			string[] seps = { ProjectSeparatorInSetting };
			foreach( var file in Properties.Settings.Default.RecentProjects ) {
				var items = file.Split( seps, StringSplitOptions.RemoveEmptyEntries );
				if( items == null || items.Length <= 1 || File.Exists( items[1] ) == false )
					continue;

				sProjects.Add( new ProjectModel {
					ProjectName = items[0],
					FullPathFileName = items[1],
					FileName = Path.GetFileName( items[1] )
				} );
			}

			if( CustomGlossaryFileListLocations == null )
				CustomGlossaryFileListLocations = new ObservableList<string>();

			CustomGlossaryFileListLocations.Clear();
			foreach( var loc in Properties.Settings.Default.CustomGlossaryFileListLocations ) {
				CustomGlossaryFileListLocations.Add( loc );
			}

			return true;
		}

		public bool SaveListToSettings()
		{
			//if( sProjects.Count <= 0 ) // BUG!!!
			//	return true;

			_CheckSettings();

			List<string> recentFiles = new List<string>();
			foreach( var proj in sProjects ) {
				// add separator string between ProjectName and FullPathFileName
				recentFiles.Add( $"{proj.ProjectName}{ProjectSeparatorInSetting}{proj.FullPathFileName}" );
			}

			Properties.Settings.Default.RecentProjects.Clear();
			Properties.Settings.Default.RecentProjects.AddRange( recentFiles.ToArray() );

			Properties.Settings.Default.CustomGlossaryFileListLocations.Clear();
			Properties.Settings.Default.CustomGlossaryFileListLocations.AddRange( CustomGlossaryFileListLocations.ToArray() );

			Properties.Settings.Default.Save();
			Properties.Settings.Default.Save();
			return true;
		}

		public async Task<ProjectModel> OpenProject( string fullPathFileName )
		{
			if( string.IsNullOrWhiteSpace( fullPathFileName ) )
				return null;

			ProjectModel projModel = sProjects.FirstOrDefault( p => p.FullPathFileName == fullPathFileName );

			if( projModel == null ) {
				projModel = new ProjectModel {
					FileName = Path.GetFileName( fullPathFileName ),
					FullPathFileName = fullPathFileName,
				};
				sProjects.Add( projModel );
			}

			if( projModel.Project == null ) {
				if( File.Exists( fullPathFileName ) ) {
					// restore existed project file
					var projObj = TranslationProject.DeserializeFromXml( fullPathFileName );
					if( projObj == null )
						return null;

					projModel.Project = projObj;
				}
				else {
					// create a new project file
					projModel.Project = new TranslationProject {
						Name = Path.GetFileNameWithoutExtension( fullPathFileName ),
					};
					// then, save it to disk
					TranslationProject.SerializeToXml( projModel.Project, fullPathFileName );
				}
			}

			// projModel prepared
			projModel.ProjectName = projModel.Project.Name;
			await MarkInUsed( projModel );

			// prepare mMonitor
			var baseProjectPath = Path.GetDirectoryName( fullPathFileName );
			if( mMonitor == null ) {
				mMonitor = new MappingMonitor( baseProjectPath );
			}
			else {
				mMonitor.Stop();

				// mMonitor was created, and might be monitoring the same or other folder...
				if( mMonitor.BaseProjectPath != baseProjectPath ) {

					// TODO: un-subscribe all event!!!!!!

					// mMonitor is monitoring other folder, so Dispose it first
					mMonitor.ClearAllMonitoring();
					mMonitor.Dispose();
					mMonitor = new MappingMonitor( baseProjectPath );
				}
				else {
					// mMonitor is monitoring same folder, but need to remove old project and add new project!
					// TODO: un-subscribe all event!!!!!!
					
					// remove last file, which is cooresponding previous project file for first priority!
					mMonitor.RemoveMonitoring( mMonitor.MonitoringFileList.Last() );
				}
			}

			// add new project's OberserableCollection table to monitor...
			mMonitor.AutoRemoveMonitoringWhenFileChanged = AutoRemoveMonitoringWhenFileChanged;
			mMonitor.AddMonitoring( fullPathFileName, projModel.Project.MappingTable );
			mMonitor.Start();

			return projModel;
		}

		public bool SaveProject( ProjectModel model, string fullPathFileName )
		{
			if( model == null || string.IsNullOrWhiteSpace( fullPathFileName ) )
				return false;

			if( model.Project == null )
				model.Project = new TranslationProject();

			if( model.Project.MappingTable != null && model.Project.MappingTable.Count > 0 ) {
				if( model.Project.MappingTable[0] is MappingMonitor.MappingModel ) {
					model.Project.MappingTable = model.Project.MappingTable.Select( x => (x as MappingMonitor.MappingModel)?.ToMappingEntry() ).ToList();
				}
			}

			mMonitor?.Stop();
			var ret = TranslationProject.SerializeToXml( model.Project, fullPathFileName );
			mMonitor?.Start();

			return ret;
		}

		public bool CloseProject( ProjectModel projModel )
		{
			if( projModel == null || sProjects.Contains( projModel ) == false )
				return false;

			// DO NOT Remove ProjectModel from sProjects!!
			//sProjects.Remove( projModel );
			projModel.InUsed = false;
			projModel.Project.MappingTable?.Clear();
			projModel.Project = null;

			//if( sProjects.Count <= 0 && mMonitor != null ) {
			if( mMonitor != null ) {
				mMonitor.Stop();
				mMonitor.ClearAllMonitoring();
				mMonitor.Dispose();
			}
			mMonitor = null;
			return true;
		}

		public async Task<bool> MarkInUsed( ProjectModel proj )
		{
			if( proj == null || sProjects.Count <= 0 || sProjects.Contains( proj ) == false )
				return false;
			if( proj.InUsed ) //|| sProjects.IndexOf( proj ) == 0 )
				return true;

			foreach( var p in sProjects ) {
				p.InUsed = false;
			}
			proj.InUsed = true;
			CurrentProject = proj;
			sProjects.Remove( proj );
			sProjects.Insert( 0, proj );

			await MessageHub.SendMessageAsync( this, MessageType.ProjectOpened, CurrentProject );

			return true;
		}

		/// <summary>
		/// Clear recenet project list in settings, but not delete phyical files
		/// </summary>
		public void ClearRecentProjects()
		{
			ProjectModel proj = sProjects.FirstOrDefault( p => p.InUsed );
			sProjects.Clear();
			if( proj != null )
				sProjects.Add( proj );

			SaveListToSettings();
		}

		/// <summary>
		/// Create some extra folders in project's base path
		/// </summary>
		/// <param name="projBasePath">Base path of project folder</param>
		/// <returns>true for success</returns>
		public bool CreateProjectFolders( string projBasePath )
		{
			if( string.IsNullOrWhiteSpace( projBasePath ) )
				return false;

			try {
				var path = Path.GetDirectoryName( projBasePath );
				if( Directory.Exists( path ) == false )
					Directory.CreateDirectory( path );

				// create "glossary" folder for storing glossary files
				if( GlossaryUtils.ExtractSampleFolders( path ) == false )
					return false;

			}
			catch( UnauthorizedAccessException uaex ) {
				Trace.WriteLine( $"Cannot create folder in {projBasePath}! Exception: {uaex.InnerException}" );
				return false;
			}
			catch( Exception ex ) {
				Trace.WriteLine( $"Something wrong! Exception: {ex.InnerException}" );
				return false;
			}

			return true;
		}

		public bool IsFileConcernedByProject( ProjectModel projModel, string fullPathFileName, string engineFolderName = "Excite" )
		{
			if( projModel == null || projModel.Project == null ||
				projModel.Project.SourceLanguage == SupportedSourceLanguage.AutoDetect ||
				string.IsNullOrWhiteSpace( fullPathFileName ) )
				return false;

			if( fullPathFileName == projModel.FullPathFileName )
				return true;

			return GlossaryUtils.IsFileConcernedByProject( projModel.FullPathFileName, fullPathFileName,
									engineFolderName, 
									projModel.Project.SourceLanguage, projModel.Project.TargetLanguage );
		}

		public async Task<bool> OpenAndMonitorGlossaryFiles( ProjectModel projModel, string engineFolderName = "Excite",
										SupportedSourceLanguage srcLang = SupportedSourceLanguage.Japanese,
										SupportedTargetLanguage tgtLang = SupportedTargetLanguage.ChineseTraditional )
		{
			if( projModel == null )
				return false;

			var baseProjectPath = Path.GetDirectoryName( projModel.FullPathFileName );
			if( mMonitor == null ) {
				mMonitor = new MappingMonitor( baseProjectPath );
			}
#if NETCOREAPP3_0
			return await GlossaryUtils.OpenAndMonitorGlossaryFiles( projModel.FullPathFileName, projModel.Project.MappingTable,
									mMonitor, engineFolderName, srcLang, tgtLang );
#else
			// NET47
			if( mMonitor == null || Directory.Exists( mMonitor.GlossaryPath ) == false ||
				string.IsNullOrWhiteSpace( engineFolderName ) ||
				srcLang == SupportedSourceLanguage.AutoDetect )
				return false;

			mMonitor.Stop();
			//mMonitor.ClearAllMonitoring();
			foreach( var fn in mMonitor.MonitoringFileList.ToList() ) {
				if( fn == projModel.FullPathFileName )
					continue;

				mMonitor.RemoveMonitoring( fn );
			}

			var glossaryPath = mMonitor.GlossaryPath;
			await Task.Delay( 50 );

			// collect mapping files
			var listAllEngine = Directory.GetFiles( glossaryPath, "*.*", SearchOption.TopDirectoryOnly ).NumericSort();
			var enginePath = Path.Combine( glossaryPath, engineFolderName );
			var langPath = Path.Combine( enginePath, $"{srcLang.ToString()}2{tgtLang.ToString()}" ); // like "Excite/Japanese2ChineseTraditional"

			IEnumerable<string> listAllLang = null, listLang = null;
			if( Directory.Exists( enginePath ) )
				listAllLang = Directory.GetFiles( enginePath, "*.*", SearchOption.TopDirectoryOnly ).NumericSort();
			if( Directory.Exists( langPath ) )
				listLang = Directory.GetFiles( langPath, "*.*", SearchOption.TopDirectoryOnly ).NumericSort();

			if( listLang == null )
				listLang = new string[] { };

			// add file names to list by descended priority
			var listFiles = new List<string>( listLang );
			if( listAllLang != null )
				listFiles.AddRange( listAllLang );
			if( listAllEngine != null )
				listFiles.AddRange( listAllEngine );

			// the listFiles is sorted by the sequence:
			//     <glossaryPath>/<engineFolder>/<langFolder>/*.* => <glossaryPath>/<engineFolder>/*.* => <glossaryPath>/*.*,
			// the later is high priority for overwriting previous terms!!

			List<(string, List<MappingEntry>)> glossaries = new List<(string, List<MappingEntry>)>();
			// detect and prepare mapping files
			var detector = new FileHelpers.Detection.SmartFormatDetector();
			var engine = new FileHelpers.DelimitedFileEngine<MappingEntry>( System.Text.Encoding.UTF8 );
			foreach( var field in engine.Options.Fields )
				field.IsOptional = true;
			engine.Options.Fields[0].IsOptional = false; // OriginalText
			// Switch error mode off
			engine.ErrorManager.ErrorMode = FileHelpers.ErrorMode.IgnoreAndContinue;

			foreach( var fileName in listFiles ) {
				try {
					var formats = detector.DetectFileFormat( fileName );

					if( formats != null && formats.Length > 0 )
						engine.Options.Delimiter = formats[0].ClassBuilderAsDelimited.Delimiter;
					else
						engine.Options.Delimiter = "\t"; // DetectFileFormat() seems cannot detect \t, hmm...

					var list = engine.ReadFileAsList( fileName );
					if( list == null || list.Count <= 0 )
						continue;

					if( list != null ) {
						// ignore first line for Header fields
						var first = list[0];
						if( first.OriginalText == nameof( MappingEntry.OriginalText ) &&
							first.MappingText == nameof( MappingEntry.MappingText ) ) {
							list.RemoveAt( 0 );
						}

						glossaries.Add( (fileName, list) );
						continue;
					}

					// TODO: try to parse .xlxs file...

				}
				catch {
					continue;
				}
			}

			// prepared, then clear existed project.conf mapping
			var projMappingColl = mMonitor.GetMappingCollection( projModel.FullPathFileName );
			mMonitor.RemoveMonitoring( projModel.FullPathFileName );

			foreach( var glossary in glossaries ) {
				mMonitor.AddMonitoring( glossary.Item1, glossary.Item2 );
			}

			// finally, add back project.conf's mapping for highest priority
			if( projMappingColl == null )
				mMonitor.AddMonitoring( projModel.FullPathFileName, projModel.Project.MappingTable );
			else
				mMonitor.AddMonitoring( projModel.FullPathFileName, projMappingColl );

			mMonitor.Start();
			return true;
#endif
		}

		public List<MappingEntry> TryParseAndExtractMappingEntries( string fullPathFileName )
		{
			if( File.Exists( fullPathFileName ) == false )
				return null;

#if NETCOREAPP3_0
			return GlossaryUtils.TryParseAndExtractMappingEntries( fullPathFileName );
#else
			// NET47

			// detect and prepare mapping files
			var detector = new FileHelpers.Detection.SmartFormatDetector();
			var engine = new FileHelpers.DelimitedFileEngine<MappingEntry>( System.Text.Encoding.UTF8 );
			foreach( var field in engine.Options.Fields )
				field.IsOptional = true;
			engine.Options.Fields[0].IsOptional = false; // OriginalText
			// Switch error mode off
			engine.ErrorManager.ErrorMode = FileHelpers.ErrorMode.IgnoreAndContinue;

			try {
				var formats = detector.DetectFileFormat( fullPathFileName );

				if( formats != null && formats.Length > 0 )
					engine.Options.Delimiter = formats[0].ClassBuilderAsDelimited.Delimiter;
				else
					engine.Options.Delimiter = "\t"; // DetectFileFormat() seems cannot detect \t, hmm...

				var list = engine.ReadFileAsList( fullPathFileName );
				if( list != null ) {
					// ignore first line for Header fields
					var first = list[0];
					if( first.OriginalText == nameof( MappingEntry.OriginalText ) &&
						first.MappingText == nameof( MappingEntry.MappingText ) ) {
						list.RemoveAt( 0 );
					}

					return list;
				}

				// TODO: try to parse .xlxs file...

			}
			catch {
				return null;
			}

			return null;
#endif
		}

		public async Task<bool> FetchFilesByFileListLink( string fileListLink, string targetPath,
											OverwritePolicy policy, CancellationTokenSource cancelToken,
											IProgress<Minax.ProgressInfo> progress,
											MetroWindow mainWindow )
		{
			if( policy != OverwritePolicy.AlwaysAsking ) {
				return await Minax.Web.Utils.FetchFilesByFileListLink( fileListLink, targetPath, policy,
										cancelToken, progress );
			}

			// policy == OverwritePolicy.AlwaysAsking

			if( string.IsNullOrWhiteSpace( fileListLink ) ||
				string.IsNullOrWhiteSpace( targetPath ) ||
				mainWindow == null )
				return false;

			try {
				Uri uri = new Uri( fileListLink );

				if( uri.Scheme == Uri.UriSchemeFtp ) {
					return await FetchFilesByFileListLink( uri, targetPath, policy, cancelToken, progress, mainWindow );
				}

				if( Directory.Exists( targetPath ) == false )
					Directory.CreateDirectory( targetPath );

				using( var client = new HttpClient() ) {
					var response = await client.GetAsync( uri );
					if( response == null || response.IsSuccessStatusCode == false )
						return false;

					var responseString = await response.Content.ReadAsStringAsync();
					if( string.IsNullOrWhiteSpace( responseString ) )
						return true;

					var remoteRelFiles = responseString.Split( new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries );
					if( remoteRelFiles == null || remoteRelFiles.Length <= 0 )
						return true;

					foreach( var relFn in remoteRelFiles ) {
						try {
							var locFn = Path.GetFullPath( Path.Combine( targetPath, relFn ) );
							if( Directory.Exists( locFn ) ) {
								continue;
							}

							Uri relUri = new Uri( uri, relFn );

							response = await client.GetAsync( relUri );
							if( response == null || response.IsSuccessStatusCode == false ||
								response.Content.Headers.ContentLength <= 0 )
								continue;

							if( File.Exists( locFn ) ) {
								var rst = await mainWindow.ShowMessageAsync( "Overwrite Confirm",
											$"Glossary File \"{locFn}\" existed, do you want to overwrite it?",
											MessageDialogStyle.AffirmativeAndNegative );
								if( rst != MessageDialogResult.Affirmative )
									continue;
							}

							using( var stream = new FileStream( locFn, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) ) {
								await response.Content.CopyToAsync( stream );
							}
						}
						catch { }
					}
				}
			}
			catch( Exception ex ) {
				progress?.Report( new Minax.ProgressInfo {
					PercentOrErrorCode = -1,
					Message = ex.Message,
					InfoObject = ex
				} );
				return false;
			}


			return true;
		}


		public static async Task<bool> FetchFilesByFileListLink( Uri ftpFileListLink, string targetPath,
											OverwritePolicy policy, CancellationTokenSource cancelToken,
											IProgress<Minax.ProgressInfo> progress,
											MetroWindow mainWindow )
		{
			if( policy != OverwritePolicy.AlwaysAsking ) {
				return await Minax.Web.Utils.FetchFilesByFileListLink( ftpFileListLink, targetPath, policy,
										cancelToken, progress );
			}

			if( ftpFileListLink == null || ftpFileListLink.Scheme != Uri.UriSchemeFtp ||
				string.IsNullOrWhiteSpace( targetPath ) || mainWindow == null )
				return false;

			try {
				if( Directory.Exists( targetPath ) == false )
					Directory.CreateDirectory( targetPath );

				FtpWebRequest request = (FtpWebRequest)WebRequest.Create( ftpFileListLink );
				request.Method = WebRequestMethods.Ftp.DownloadFile;
				request.Timeout = 5000;

				FtpWebResponse response = await request.GetResponseAsync() as FtpWebResponse;
				if( response == null || response.ContentLength <= 0 ||
					response.StatusCode != FtpStatusCode.CommandOK ) {
					progress?.Report( new Minax.ProgressInfo {
						PercentOrErrorCode = 100,
						Message = $"Cannot fetch remote glossary list file.",
						InfoObject = response,
					} );
					return false;
				}

				string responseString = null;
				var stream = response.GetResponseStream();
				using( var reader = new StreamReader( stream, Encoding.UTF8 ) ) {
					responseString = await reader.ReadToEndAsync();
				}
				response.Close();

				if( string.IsNullOrWhiteSpace( responseString ) ) {
					progress?.Report( new Minax.ProgressInfo {
						PercentOrErrorCode = 100,
						Message = $"No glossary file can fetch.",
					} );
					return false;
				}

				var remoteRelFiles = responseString.Split( new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries );
				if( remoteRelFiles == null || remoteRelFiles.Length <= 0 ) {
					progress?.Report( new Minax.ProgressInfo {
						PercentOrErrorCode = 100,
						Message = $"No glossary file can fetch.",
					} );
					return true;
				}

				for( int i = 0; i < remoteRelFiles.Length; ++i ) {
					var relFn = remoteRelFiles[i];
					try {
						var locFn = Path.GetFullPath( Path.Combine( targetPath, relFn ) );
						if( Directory.Exists( locFn ) ) {
							continue;
						}

						Uri relUri = new Uri( ftpFileListLink, relFn );
						request = (FtpWebRequest)WebRequest.Create( relUri );
						request.Method = WebRequestMethods.Ftp.DownloadFile;
						request.Timeout = 5000;

						response = await request.GetResponseAsync() as FtpWebResponse;
						if( response == null || response.ContentLength <= 0 ||
							response.StatusCode != FtpStatusCode.CommandOK )
							continue;

						if( File.Exists( locFn ) ) {
							var rst = await mainWindow.ShowMessageAsync( "Overwrite Confirm",
											$"Glossary File \"{locFn}\" existed, do you want to overwrite it?",
											MessageDialogStyle.AffirmativeAndNegative );
							if( rst != MessageDialogResult.Affirmative )
								continue;
						}

						using( var fileStream = new FileStream( locFn, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) ) {
							await response.GetResponseStream().CopyToAsync( fileStream );
							response.Close();
						}

						progress?.Report( new Minax.ProgressInfo {
							PercentOrErrorCode = (i + 1) / remoteRelFiles.Length,
							Message = $"Fetched {i + 1}/{remoteRelFiles.Length} files.",
						} );
					}
					catch { }
				}
			}
			catch( Exception ex ) {
				progress?.Report( new Minax.ProgressInfo {
					PercentOrErrorCode = -1,
					Message = ex.Message,
					InfoObject = ex
				} );
				return false;
			}

			progress?.Report( new Minax.ProgressInfo {
				PercentOrErrorCode = 100,
				Message = $"All glossary files fetched.",
			} );

			return true;
		}
	}
}
