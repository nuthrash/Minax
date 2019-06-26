using Minax.Collections;
using Minax.Domain.Translation;
using Minax.IO;
using Minax.Web.Translation;
using MinaxWebTranslator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MinaxWebTranslator
{
	/// <summary>
	/// App internal Project Manager for manipulating Project management and other works
	/// </summary>
	internal class ProjectManager
	{
		public static string ProjectSeparatorInSetting => "⇭⍢";

		public static ProjectManager Instance { get; } = new ProjectManager();

		public ReadOnlyObservableList<ProjectModel> RecentProjects => mProjects;

		public ProjectModel CurrentProject {
			get => mCurrentProject;
			private set {
				if( mCurrentProject == value )
					return;

				mCurrentProject = value;
				CurrentProjectChanged?.Invoke( this, null );
			}
		}
		public event EventHandler CurrentProjectChanged;

		/// <summary>
		/// MappingMonitor for monitoring file changed or Mapping changed with manipulating methods
		/// </summary>
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

		private static readonly ObservableList<ProjectModel> sProjects = new ObservableList<ProjectModel>();
		private ReadOnlyObservableList<ProjectModel> mProjects;
		private ProjectModel mCurrentProject;
		private MappingMonitor mMonitor = null;
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

		#endregion

		/// <summary>
		/// Restore recently project list from AppSettings
		/// </summary>
		/// <returns></returns>
		public bool RestoreListFromSettings()
		{
			sProjects.Clear();

			List<ProjectModel> pmList = new List<ProjectModel>();
			foreach( var file in Properties.Settings.Default.RecentProjects ) {
				var pm = ProjectModel.ConvertFromSettingString( file );
				if( pm == null || File.Exists( pm.FullPathFileName ) == false )
					continue;

				pmList.Add( pm );
			}
			sProjects.AddRange( pmList );

			if( CustomGlossaryFileListLocations == null )
				CustomGlossaryFileListLocations = new ObservableList<string>();

			CustomGlossaryFileListLocations.Clear();
			CustomGlossaryFileListLocations.AddRange( Properties.Settings.Default.CustomGlossaryFileListLocations );

			return true;
		}

		/// <summary>
		/// Save recently project lsit to AppSettings
		/// </summary>
		/// <returns></returns>
		public bool SaveListToSettings()
		{
			List<string> recentFiles = new List<string>();
			foreach( var proj in sProjects ) {
				// add separator string between ProjectName and FullPathFileName
				recentFiles.Add( proj.ToSettingString() );
			}

			Properties.Settings.Default.RecentProjects.Clear();
			Properties.Settings.Default.RecentProjects.AddRange( recentFiles.ToArray() );

			Properties.Settings.Default.CustomGlossaryFileListLocations.Clear();
			Properties.Settings.Default.CustomGlossaryFileListLocations.AddRange( CustomGlossaryFileListLocations.ToArray() );

			Properties.Settings.Default.Save();
			return true;
		}

		/// <summary>
		/// Open/Deserialize project from a full-path file name
		/// </summary>
		/// <param name="fullPathFileName">Project full-path file name</param>
		/// <returns>null or opened Project instance</returns>
		public ProjectModel OpenProject( string fullPathFileName )
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
			MarkAsCurrent( projModel );

			// prepare mMonitor
			var baseProjectPath = Path.GetDirectoryName( fullPathFileName ) + Path.DirectorySeparatorChar;
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

			// add new project's OberserableList table to monitor...
			mMonitor.AutoRemoveMonitoringWhenFileChanged = AutoRemoveMonitoringWhenFileChanged;
			mMonitor.AddMonitoring( fullPathFileName, projModel.Project.MappingTable );
			mMonitor.Start();

			return projModel;
		}

		/// <summary>
		/// Save/Serialize Project instance to file
		/// </summary>
		/// <param name="model">Source ProjectModel instance</param>
		/// <param name="fullPathFileName">Target full-path file name</param>
		/// <returns>true for success</returns>
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

			model.Project.Name = model.ProjectName;

			mMonitor?.Stop();
			var ret = TranslationProject.SerializeToXml( model.Project, fullPathFileName );
			mMonitor?.Start();

			return ret;
		}

		/// <summary>
		/// Close and un-set InCurrent of a Project instance
		/// </summary>
		/// <param name="projModel">Target ProjectModel</param>
		/// <returns>true for success</returns>
		public bool CloseProject( ProjectModel projModel )
		{
			if( projModel == null || sProjects.Contains( projModel ) == false )
				return false;

			// DO NOT Remove ProjectModel from sProjects!!
			//sProjects.Remove( projModel );
			projModel.IsCurrent = false;
			projModel.Project.MappingTable?.Clear();
			projModel.Project = null;

			if( mMonitor != null ) {
				mMonitor.Stop();
				mMonitor.ClearAllMonitoring();
				mMonitor.Dispose();
			}
			mMonitor = null;
			CurrentProject = null;
			return true;
		}

		/// <summary>
		/// Add ProjectModel instance to project list
		/// </summary>
		/// <param name="fullPathFileName">Full-path file name of project</param>
		/// <param name="projObj">ProjectModel instance</param>
		/// <returns>Configured ProjectModel instance</returns>
		public ProjectModel AddProject( string fullPathFileName, TranslationProject projObj )
		{
			foreach( var proj in sProjects ) {
				if( proj.FullPathFileName == fullPathFileName )
					return proj;
			}

			var model = new ProjectModel {
				FullPathFileName = fullPathFileName,
				Project = projObj,
				ProjectName = projObj.Name,
				FileName = Path.GetFileName( fullPathFileName ),
			};

			sProjects.Add( model );
			return model;
		}

		/// <summary>
		/// Mark ProjectModel instance as current
		/// </summary>
		/// <param name="proj">Target ProjectModel instance</param>
		/// <returns>true for success</returns>
		public bool MarkAsCurrent( ProjectModel proj )
		{
			if( proj == null || sProjects.Count <= 0 || sProjects.Contains( proj ) == false )
				return false;
			if( proj.IsCurrent )
				return true;

			foreach( var p in sProjects ) {
				p.IsCurrent = false;
			}
			proj.IsCurrent = true;
			CurrentProject = proj;
			sProjects.Remove( proj );
			sProjects.Insert( 0, proj );

			return true;
		}

		/// <summary>
		/// Clear recenet project list in settings, but not delete phyical files
		/// </summary>
		public void ClearRecentProjects()
		{
			ProjectModel proj = sProjects.FirstOrDefault( p => p.IsCurrent );
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

		/// <summary>
		/// Check if a file is concerned by a ProjectModel instance
		/// </summary>
		/// <param name="projModel">Source ProjectModel instance</param>
		/// <param name="fullPathFileName">Target full-path file name</param>
		/// <param name="engineFolderName">Remote Translator/Translation engine folder name for Glossary</param>
		/// <returns>true for concerned</returns>
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

		/// <summary>
		/// Open and start monitoring glossary files
		/// </summary>
		/// <param name="projModel">ProjectModel instance</param>
		/// <param name="engineFolderName">Remote Translator/Translation engine folder name for Glossary</param>
		/// <param name="srcLang">Mapping source language</param>
		/// <param name="tgtLang">Mapping target language</param>
		/// <returns>true for success</returns>
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

			return await GlossaryUtils.OpenAndMonitorGlossaryFiles( projModel.FullPathFileName, projModel.Project.MappingTable,
									mMonitor, engineFolderName, srcLang, tgtLang );
		}

		/// <summary>
		/// Try to parse and extract MappingEntry from a file 
		/// </summary>
		/// <param name="fullPathFileName">Target full-path file name</param>
		/// <returns>null or extracted MappingEntry list</returns>
		public List<MappingEntry> TryParseAndExtractMappingEntries( string fullPathFileName )
		{
			if( File.Exists( fullPathFileName ) == false )
				return null;

			return GlossaryUtils.TryParseAndExtractMappingEntries( fullPathFileName );
		}

		/// <summary>
		/// Fetch remote files by remote file list link by HTTP/HTTPS/FTP
		/// </summary>
		/// <param name="fileListLink">Source remote file list link</param>
		/// <param name="targetPath">Target path to place fetched files</param>
		/// <param name="policy">Overwrite policy for same file name</param>
		/// <param name="cancelToken">Cancellation token</param>
		/// <param name="progress">Progress instance</param>
		/// <param name="mainWindow">MainWindow instance</param>
		/// <returns>true for success</returns>
		public async Task<bool> FetchFilesByFileListLink( string fileListLink, string targetPath,
											OverwritePolicy policy, CancellationTokenSource cancelToken,
											IProgress<Minax.ProgressInfo> progress,
											Page mainPage )
		{
			if( policy != OverwritePolicy.AlwaysAsking ) {
				return await Minax.Web.Utils.FetchFilesByFileListLink( fileListLink, targetPath, policy,
										cancelToken, progress );
			}

			if( string.IsNullOrWhiteSpace( fileListLink ) ||
				string.IsNullOrWhiteSpace( targetPath ) )
				return false;

			try {
				Uri uri = new Uri( fileListLink );

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

					for( int i = 0; i < remoteRelFiles.Length; ++i ){
						try {
							var relFn = remoteRelFiles[i];
							var locFn = Path.GetFullPath( Path.Combine( targetPath, relFn ) );
							if( Directory.Exists( locFn ) ) {
								goto exit1;
							}
							if( Path.HasExtension( locFn ) == false ) {
								// this locFn is a directory
								Directory.CreateDirectory( locFn );
								goto exit1;
							}

							Uri relUri = new Uri( uri, relFn );

							response = await client.GetAsync( relUri );
							if( response == null || response.IsSuccessStatusCode == false ||
								response.Content.Headers.ContentLength <= 0 )
								goto exit1;

							if( File.Exists( locFn ) ) {
								var rst = await mainPage.DisplayAlert( "Overwrite Confirm",
									$"Glossary File \"{locFn}\" existed, do you want to overwrite it?", "Yes", "No" );

								if( rst != true )
									goto exit1;
							}

							using( var stream = new FileStream( locFn, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) ) {
								await response.Content.CopyToAsync( stream );
							}

						exit1:
							progress?.Report( new Minax.ProgressInfo {
								PercentOrErrorCode = i,
								Message = $"Glossary File \"{locFn}\" created.",
							} );
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

		/// <summary>
		/// Fetch remote files by remote file list link by FTP
		/// </summary>
		/// <param name="ftpFileListLink">Source remote file list link via FTP protocol</param>
		/// <param name="targetPath">Target path to place fetched files</param>
		/// <param name="policy">Overwrite policy for same file name</param>
		/// <param name="cancelToken">Canllation token</param>
		/// <param name="progress">Progress instance</param>
		/// <param name="mainWindow">MainWindow instance</param>
		/// <returns>true for success</returns>
		public static async Task<bool> FetchFilesByFileListLink( Uri ftpFileListLink, string targetPath,
											OverwritePolicy policy, CancellationTokenSource cancelToken,
											IProgress<Minax.ProgressInfo> progress,
											Page mainPage )
		{
			if( policy != OverwritePolicy.AlwaysAsking ) {
				return await Minax.Web.Utils.FetchFilesByFileListLink( ftpFileListLink, targetPath, policy,
										cancelToken, progress );
			}

			if( ftpFileListLink == null || ftpFileListLink.Scheme != Uri.UriSchemeFtp ||
				string.IsNullOrWhiteSpace( targetPath ) || mainPage == null )
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
							var rst = await mainPage.DisplayAlert( "Overwrite Confirm",
									$"Glossary File \"{locFn}\" existed, do you want to overwrite it?", "Yes", "No" );

							if( rst != true )
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
