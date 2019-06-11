using Minax.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Minax.Domain.Translation
{
	/// <summary>
	/// Mapping entries and files monitor for Translation project and glossary files.
	/// </summary>
	public class MappingMonitor : IDisposable
	{
		/// <summary>
		/// Constructor with necessary parameter
		/// </summary>
		/// <param name="baseProjectPath">The root project path</param>
		public MappingMonitor( string baseProjectPath )
		{
			if( string.IsNullOrWhiteSpace( baseProjectPath ) )
				throw new ArgumentException(
					string.Format( Languages.Global.Str1ArgumentIsInvalid, nameof( baseProjectPath ) ),
					nameof( baseProjectPath ) );

			if( Directory.Exists( baseProjectPath ) == false ) {
				try {
					Directory.CreateDirectory( baseProjectPath );
					//System.Threading.Thread.Sleep( 10 );
					Directory.Delete( baseProjectPath );
				}
				catch {
					throw new ArgumentException(
						string.Format( Languages.Global.Str1ArgumentIsInvalid, nameof( baseProjectPath ) ),
						nameof( baseProjectPath ) );
				}
			}

			BaseProjectPath = baseProjectPath;
			if( BaseProjectPath.EndsWith( $"{Path.DirectorySeparatorChar}" ) == false )
				BaseProjectPath = BaseProjectPath + Path.DirectorySeparatorChar;
			GlossaryPath = Path.Combine( BaseProjectPath, GlossaryFolderName );

			mFileList = new ObservableList<string>( Directory.GetFiles( BaseProjectPath, "*.*", SearchOption.AllDirectories ) );
			mFileList.AddRange( Directory.GetDirectories( BaseProjectPath, "*", SearchOption.AllDirectories ) );

			// finally, prepare FileSystemWatcher instance
			mWatcher = new FileSystemWatcher();
			mWatcher.Path = BaseProjectPath;
			mWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName |
									NotifyFilters.LastWrite | NotifyFilters.Size;
			mWatcher.Filter = "*.*";
			mWatcher.IncludeSubdirectories = true;
			mWatcher.Changed += OnFileChanged;
			mWatcher.Created += OnFileChanged;
			mWatcher.Deleted += OnFileChanged;
			mWatcher.Renamed += OnFileRenamed;
			mWatcher.EnableRaisingEvents = false;
		}
		~MappingMonitor()
		{
			this.Dispose();
		}

		#region IDisposable
		public void Dispose()
		{
			// avoid resource leak
			if( mDisposed )
				return;

			if( mDisposing == false ) {
				mDisposing = true;
				ReleaseResources();
				mDisposed = true;
			}
		}
		#endregion


		#region "Public properties/const"

		/// <summary>
		/// Default glossary folder name where stored mapping files.
		/// </summary>
		public const string GlossaryFolderName = "Glossary";

		/// <summary>
		/// Base project path for monitoring all files and directies changed events.
		/// </summary>
		public string BaseProjectPath { get; private set; }
		/// <summary>
		/// Glossary path under BaseProjectPath
		/// </summary>
		public string GlossaryPath { get; private set; }

		/// <summary>
		/// The descended sequence of OriginalText property/field by string.Length for substituting longer text first.
		/// </summary>
		/// <remarks>This list would be updated frequently when added new text or deleted old text..</remarks>
		public ReadOnlyObservableList<string> DescendedOriginalTextByTextLength => mDesOriginalTextColl;

		/// <summary>
		/// The MappingModel list ordered with descended sequence by DescendedOriginalTextByTextLength.
		/// </summary>
		public ReadOnlyObservableList<MappingModel> DescendedModels => mDesModelsColl;

		/// <summary>
		/// File list of existing files and directories in BaseProjectPath folder on this time
		/// </summary>
		/// <remarks>This list only show current watching files and directories.</remarks>
		//public IReadOnlyObservableList<string> FileList => mFileList as IReadOnlyObservableList<string>;
		public ReadOnlyObservableList<string> FileList => mFileList;

		/// <summary>
		/// Registered file list for monitoring Changed/Renamed events in BaseProjectPath folder and sub-directories.
		/// The sequence of this list reflect the priority of each file from low to high, which means index 0 has lowest priority.
		/// This property would changed by AddMonitoring(), RemoveMonitoring(), etc..
		/// </summary>
		public ReadOnlyObservableList<string> MonitoringFileList => mMonFileList;

		/// <summary>
		/// Auto remove Monitoring when file changed (and when Mapping collection changed still remove them!)
		/// </summary>
		public bool AutoRemoveMonitoringWhenFileChanged { get; set; } = false;

		/// <summary>
		/// Auto change ProjectBaseFileName property when detected file name changed
		/// </summary>
		public bool AutoModifyMappingContentWhenFileRenamed { get; set; } = true;

		/// <summary>
		/// The deferred monitoring changed event
		/// </summary>
		public event MonitorEventHandler MonitorDeferredChanged;

		#endregion


		#region "Public classes/enum"

		/// <summary>
		/// MappingModel(inherited from MappingEntry) with some extra runtime properties
		/// </summary>
		public class MappingModel : MappingEntry, INotifyPropertyChanged, IDataErrorInfo
		{
			/// <summary>
			/// Original string/text
			/// </summary>
			public new string OriginalText {
				get => base.OriginalText;
				set {
					if( base.OriginalText != value ) {
						base.OriginalText = value;
						OnNotify();
					}
				}
			}

			/// <summary>
			/// Mapping string/text
			/// </summary>
			public new string MappingText {
				get => base.MappingText;
				set {
					if( base.MappingText != value ) {
						base.MappingText = value;
						OnNotify();
					}
				}
			}

			/// <summary>
			/// Mapping text category
			/// </summary>
			public new TextCategory? Category {
				get => base.Category;
				set {
					if( base.Category != value ) {
						base.Category = value;
						OnNotify();
					}
				}
			}

			/// <summary>
			/// Basic description for this Mapping
			/// </summary>
			public new string Description {
				get => base.Description;
				set {
					if( base.Description != value ) {
						base.Description = value;
						OnNotify();
					}
				}
			}

			/// <summary>
			/// Extra description for this Mapping
			/// </summary>
			public new string Comment {
				get => base.Comment;
				set {
					if( base.Comment != value ) {
						base.Comment = value;
						OnNotify();
					}
				}
			}

			/// <summary>
			/// Relative ProjectBasedFileName to MappingMonitor's BaseProjectPath
			/// </summary>
			public string ProjectBasedFileName {
				// like "Glossary/Excite/Japanese2ChineseTraditional/Monster1.tsv"
				get => projBasedFileName;
				set => SetProperty( ref projBasedFileName, value );
			}

			#region IDataErrorInfo

			public string Error => $"{this[OriginalText]} {this[MappingText]}";

			public string this[string columnName] {
				get {
					string errorMessage = null;
					switch( columnName ) {
						case nameof( OriginalText ):
							if( String.IsNullOrWhiteSpace( OriginalText ) ) {
								errorMessage = Languages.Global.Str0OriginalTextCantEmpty;
							}
							break;
					}
					return errorMessage;
				}
			}

			#endregion

			public event PropertyChangedEventHandler PropertyChanged;

			#region "private/protected data/methods"

			private string projBasedFileName;

			protected bool SetProperty<T>( ref T backingStore, T value = default(T),
											[CallerMemberName]string propertyName = "" )
			{
				if( EqualityComparer<T>.Default.Equals( backingStore, value ) )
					return false;

				backingStore = value;
				OnNotify( propertyName );
				return true;
			}

			private void OnNotify( [CallerMemberName]string propertyName = "" )
			{
				PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
			}

			#endregion

			/// <summary>
			/// Create a new MappingEntry with same values of this MappingModel
			/// </summary>
			/// <returns></returns>
			public MappingEntry ToMappingEntry()
			{
				return new MappingEntry {
					OriginalText = this.OriginalText,
					MappingText = this.MappingText,
					Category = this.Category,
					Description = this.Description,
					Comment = this.Comment,
				};
			}

			/// <summary>
			/// Create MappingModel from MappingEntry
			/// </summary>
			/// <param name="entry">Source MappingEntry</param>
			/// <param name="srcFileName">Source ProjectBasedFileName</param>
			/// <param name="handler">PropertyChanged event handler to subscribe</param>
			/// <returns>MappingModel object</returns>
			public static MappingModel FromMappingEntry( MappingEntry entry,
							string srcFileName, PropertyChangedEventHandler handler = null )
			{
				if( entry == null || string.IsNullOrWhiteSpace( srcFileName ) )
					return null;

				var model = new MappingModel {
					ProjectBasedFileName = srcFileName,
					OriginalText = entry.OriginalText,
					MappingText = entry.MappingText,
					Category = entry.Category,
					Description = entry.Description,
					Comment = entry.Comment,
				};

				if( handler != null ) {
					model.PropertyChanged -= handler;
					model.PropertyChanged += handler;
				}

				return model;
			}
		}


		[Flags]
		public enum MonitorEvents
		{
			NoSpecific,

			FileCreated = 1, // == WatcherChangeTypes.Created
			FileDeleted = 2, // == WatcherChangeTypes.Deleted
			FileChanged = 4, // == WatcherChangeTypes.Changed
			FileRenamed = 8, // == WatcherChangeTypes.Renamed
			FileAll = 15, // == WatcherChangeTypes.All

			MappingCreated = 32,
			MappingDeleted = 64,
			MappingSquenceChanged = 128,
			MappingPropertyChanged = 256,
		}

		public class MappingEventArgs : EventArgs
		{
			public MonitorEvents Event { get; set; }
			public string FullPath { get; set; }
			public string OldFullPath { get; set; }
			public object MappingData { get; set; }
			public object OriginalMappingData { get; set; }

			public MappingEventArgs( MonitorEvents @event, string oldFullPath = null, string fullPath = null,
				object origMappingData = null, object mappingData = null )
			{
				Event = @event;
				OldFullPath = oldFullPath;
				FullPath = fullPath;
				OriginalMappingData = origMappingData;
				MappingData = mappingData;
			}
		}

		public delegate void MonitorEventHandler( object sender, MonitorEvents @event, MappingEventArgs args );

		#endregion // "Public classes/enum"


		#region "private/internal data members, helpers, event handlers"

		private FileSystemWatcher mWatcher = null;
		private volatile bool mDisposed = false, mDisposing = false;
		private ObservableList<string> mFileList = null;
		private ReadOnlyObservableList<string> mFileColl;
		private readonly ObservableList<string> mMonFileList = new ObservableList<string>();
		private ReadOnlyObservableList<string> mMonitoringFileList;
		private readonly Dictionary<string, ObservableList<MappingModel>> mMonFileDict = new Dictionary<string, ObservableList<MappingModel>>();

		private ReadOnlyObservableList<string> mDesOriginalText = null;
		private readonly ObservableList<string> mDesOriginalTextColl = new ObservableList<string>();
		private readonly ObservableList<MappingModel> mDesModelsColl = new ObservableList<MappingModel>();
		private ReadOnlyObservableList<MappingModel> mDesModels = null;
		private readonly Dictionary<string, List<MappingModel>> mMappingDict = new Dictionary<string, List<MappingModel>>();

		private readonly object mSyncObj = new object(), mSyncEvent = new object();
		private readonly List<(string FullPath, MappingEventArgs Args)> mFileEventQueue = new List<(string, MappingEventArgs)>();
		private System.Threading.Timer mDeferredTask = null;

		private void ReleaseResources()
		{
			if( mWatcher != null ) {
				mWatcher.Changed -= OnFileChanged;
				mWatcher.Created -= OnFileChanged;
				mWatcher.Deleted -= OnFileChanged;
				mWatcher.Renamed -= OnFileRenamed;
				try {
					// seems Xamarin.Forms in Android not supported this method...
					mWatcher.Dispose();
				}
				catch { }
			}
			mWatcher = null;

			ClearAllMonitoring();
			mFileList.Clear();
		}

		private void CleanAllMapping()
		{
			lock( mDesOriginalTextColl )
				mDesOriginalTextColl.Clear();
			lock( mDesModelsColl )
				mDesModelsColl.Clear();

			foreach( var listByPriority in mMappingDict.Values ) {
				listByPriority.Clear();
			}
			mMappingDict.Clear();
		}

		private void MergeMapping( string fullPathFileName, IList<MappingModel> models )
		{
			var idx = mMonFileList.IndexOf( fullPathFileName );
			if( idx < 0 )
				return;

			bool changed = false;
			if( models == null || models.Count <= 0 ) {
				// clear mapping
				var projBasedFileName = fullPathFileName.Replace( BaseProjectPath, "" );
				foreach( var list in mMappingDict.Values ) {
					foreach( var model in list ) {
						if( model.ProjectBasedFileName == projBasedFileName ) {
							list.Remove( model );
							changed = true;
							break;
						}
					}
				}
			}
			else {
				// merge > 0 count models...
				changed = true;
				foreach( var model in models ) {
					if( mMappingDict.ContainsKey( model.OriginalText ) == false ) {
						mMappingDict[model.OriginalText] = new List<MappingModel> { model };
					}
					else {
						if( mMappingDict[model.OriginalText] == null ) {
							mMappingDict[model.OriginalText] = new List<MappingModel> { model };
						}
						else {
							var listByPriority = mMappingDict[model.OriginalText];
							if( listByPriority.Count <= 0 ) {
								listByPriority.Add( model );
							}
							else {
								// check current models in listByPriority which priority is higher...
								var firstIdx = mMonFileList.IndexOf( Path.Combine( BaseProjectPath, listByPriority[0].ProjectBasedFileName ) );
								bool added = false;
								if( firstIdx < idx ) {
									listByPriority.Insert( 0, model );
									added = true;
								}
								else if( firstIdx > idx ) {
									for( int i = 1; i < listByPriority.Count; ++i ) {
										var iIdx = mMonFileList.IndexOf( Path.Combine( BaseProjectPath, listByPriority[i].ProjectBasedFileName ) );
										if( iIdx == idx ) {
											// same OriginalText in same file...so, ignore it
											added = true;
											break;
										}
										else if( iIdx < idx ) {
											// current i model's priority is lower than idx, so insert it forward
											listByPriority.Insert( i, model );
											added = true;
											break;
										}
									}
								}
								// when firstIdx == idx, which means same OriginalText in same file...so, ignore it

								if( added != true ) {
									// add to last as lowest priority
									listByPriority.Add( model );
								}
							}
						}
					}
				}
			}

			if( changed ) {
				// clear empty list and kvp
				ClearEmptyMappingDictEntry();

				// update final sorted OriginalText keys and models by descended string.Length...
				UpdateDescendedData();
			}
		}

		private void RemoveMapping( string fullPathFileName, IList<MappingModel> models )
		{
			var idx = mMonFileList.IndexOf( fullPathFileName );
			if( idx < 0 || models == null || models.Count <= 0 )
				return;

			bool changed = false;
			foreach( var model in models ) {
				if( mMappingDict.ContainsKey( model.OriginalText ) == false ) {
					continue;
				}

				var listByPriority = mMappingDict[model.OriginalText];
				foreach( var mm in listByPriority ) {
					if( model.ProjectBasedFileName == mm.ProjectBasedFileName ) {
						listByPriority.Remove( mm );
						changed = true;
						break;
					}
				}
			}

			// update DescendedOriginalTextByTextLength amd DescendedModels
			if( changed ) {
				ClearEmptyMappingDictEntry();
				UpdateDescendedData();
			}
		}

		private void UpdateDescendedData()
		{
			var desList = mMappingDict.Keys.OrderByDescending( key => key.Length ).ToList();
			var desModels = new List<MappingModel>();
			foreach( var key in desList ) {
				var listByPriority = mMappingDict[key];
				if( listByPriority != null && listByPriority.Count > 0 )
					desModels.Add( listByPriority[0] );
			}

			// update DescendedOriginalTextByTextLength amd DescendedModels
			if( mDesModelsColl.Count == desModels.Count ) {
				for( int i = 0; i < mDesModelsColl.Count; ++i ) {
					mDesModelsColl[i] = desModels[i];
					mDesOriginalTextColl[i] = desModels[i].OriginalText;
				}
			}
			else if( mDesModelsColl.Count > desModels.Count ) {
				// >, larger than
				if( desModels.Count <= 0 ) {
					mDesModelsColl.Clear();
					mDesOriginalTextColl.Clear();
				}
				else {
					int diff = mDesModelsColl.Count - desModels.Count;
					for( int i = 0; i < desModels.Count; ++i ) {
						mDesModelsColl[i] = desModels[i];
						mDesOriginalTextColl[i] = desModels[i].OriginalText;
					}
					for( int i = desModels.Count + diff - 1; i >= desModels.Count; --i ) {
						mDesModelsColl.RemoveAt( i );
						mDesOriginalTextColl.RemoveAt( i );
					}
				}
			}
			else {
				// <, less than
				if( mDesModelsColl.Count <= 0 ) {
					foreach( var entry in desModels ) {
						mDesModelsColl.Add( entry );
						mDesOriginalTextColl.Add( entry.OriginalText );
					}
				}
				else {
					int i = 0, diff = desModels.Count - mDesModelsColl.Count;
					for( i = 0; i < mDesModelsColl.Count; ++i ) {
						mDesModelsColl[i] = desModels[i];
						mDesOriginalTextColl[i] = desModels[i].OriginalText;
					}
					for( ; i < desModels.Count; ++i ) {
						mDesModelsColl.Add( desModels[i] );
						mDesOriginalTextColl.Add( desModels[i].OriginalText );
					}
				}
			}
		}

		private void ClearEmptyMappingDictEntry()
		{
			var keysToBeRemoved = new List<string>();
			foreach( var kvp in mMappingDict ) {
				if( kvp.Value == null || kvp.Value.Count <= 0 )
					keysToBeRemoved.Add( kvp.Key );
			}
			foreach( var key in keysToBeRemoved ) {
				mMappingDict.Remove( key );
			}
		}

		/// <summary>
		/// Deferred worker for checking sequential FileChanged events
		/// </summary>
		/// <param name="subject"></param>
		private void DeferredFileChecker( object subject )
		{
			// comsume mFileEventQueue
			lock( mSyncEvent ) {
				lock( mFileEventQueue ) {
					foreach( var tuple2 in mFileEventQueue ) {
						bool contains = mFileList.Contains( tuple2.FullPath );
						FileAttributes attr;

						switch( tuple2.Args.Event ) {
							case MonitorEvents.FileCreated:
								// file was created, in fact, system would fire Changed events immediatelly!!!
								if( contains == false ) {
									// notify subscriber(s) for file created by CollectionChanged event
									mFileList.Add( tuple2.FullPath );
									MonitorDeferredChanged?.Invoke( this, tuple2.Args.Event, tuple2.Args );
								}
								break;
							case MonitorEvents.FileDeleted:
								// file was deleted
								if( contains ) {
									// notify subscriber(s) for file deleted by CollectionChanged event
									mFileList.Remove( tuple2.FullPath );
									MonitorDeferredChanged?.Invoke( this, tuple2.Args.Event, tuple2.Args );
								}

								// notify subscriber(s) for monitoring file deleted by CollectionChanged event
								if( AutoRemoveMonitoringWhenFileChanged )
									RemoveMonitoring( tuple2.FullPath );
								break;
							case MonitorEvents.FileChanged:
								if( contains == false ) {
									// file was created, then system fire Changed events immediatelly!!!
									// notify subscriber(s) for file created by CollectionChanged event
									mFileList.Add( tuple2.FullPath );
								}
								else {
									// ignore Directory
									attr = File.GetAttributes( tuple2.FullPath );
									if( attr.HasFlag( FileAttributes.Directory ) ) {
										// TODO: 
										goto exit1;
									}

									// file is existed, or deleted -> created -> changed...
									// notify subscriber to reload/read file with changed content to this monitor!!
								}
								MonitorDeferredChanged?.Invoke( this, tuple2.Args.Event, tuple2.Args );

								// file was changed, so monitoring file might changed...
								// notify subscriber(s) for monitoring file changed by CollectionChanged event
								if( AutoRemoveMonitoringWhenFileChanged )
									RemoveMonitoring( tuple2.FullPath );
								break;

							case MonitorEvents.FileRenamed:
								attr = File.GetAttributes( tuple2.Args.FullPath );

								if( attr.HasFlag( FileAttributes.Directory ) ) {
									// TODO:
									goto exit1;
								}
								else {
									if( mFileList.Contains( tuple2.Args.OldFullPath ) )
										mFileList.Remove( tuple2.Args.OldFullPath );
									mFileList.Add( tuple2.FullPath );

									// notify subscriber(s) for monitoring file renamed by CollectionChanged event
									if( AutoModifyMappingContentWhenFileRenamed ) {
										ChangeMonitoringFilePath( tuple2.Args.OldFullPath, tuple2.Args.FullPath );
									}
									else if( AutoRemoveMonitoringWhenFileChanged ) {
										RemoveMonitoring( tuple2.Args.OldFullPath );
									}
								}
								MonitorDeferredChanged?.Invoke( this, tuple2.Args.Event, tuple2.Args );
								break;

						}
					}
				}
			exit1:
				mFileEventQueue.Clear();

				mDeferredTask?.Dispose();
				mDeferredTask = null;
			}
		}

		private void OnFileChanged( object source, FileSystemEventArgs e )
		{

			/*

			Some situations:
			1. Copy a new file to a folder
				Created -> Changed -> Changed
			2. Overwrite a file with same name
				Changed -> Changed -> Changed
			3. Copy a file to another file with different name in same folder
				Created -> Changed -> Changed
			4. Change a file's name
				Renamed
			5. Delete a file
				Deleted
			6. Change a file's content then save
				Changed
			7. Delete a file first, then copy other file to same folder with same file name
				Deleted -> Created -> Changed -> Changed

			*/
			var type = (MonitorEvents)e.ChangeType;

			lock( mSyncEvent ) {
				foreach( var tuple2 in mFileEventQueue ) {
					if( tuple2.FullPath == e.FullPath ) {
						tuple2.Args.Event = type;
						return;
					}
				}

				// not found
				mFileEventQueue.Add( (e.FullPath, new MappingEventArgs( type, e.FullPath, e.FullPath )) );

				// trigger deferred task to detect real file status!!
				if( mDeferredTask == null ) {
					mDeferredTask = new System.Threading.Timer( DeferredFileChecker, null, 1000,
										System.Threading.Timeout.Infinite );
				}
			}
		}

		private void OnFileRenamed( object sender, RenamedEventArgs e )
		{
			var type = (MonitorEvents)e.ChangeType;

			lock( mSyncEvent ) {
				foreach( var tuple2 in mFileEventQueue ) {
					if( tuple2.FullPath == e.FullPath ) {
						tuple2.Args.Event = type;
						tuple2.Args.OldFullPath = e.OldFullPath;
						tuple2.Args.FullPath = e.FullPath;
						return;
					}
				}

				// not found
				mFileEventQueue.Add( (e.OldFullPath, new MappingEventArgs( type, e.OldFullPath, e.FullPath, e ) ) );

				// trigger deferred task to detect real file status!!
				if( mDeferredTask == null ) {
					mDeferredTask = new System.Threading.Timer( DeferredFileChecker, null, 1000,
										System.Threading.Timeout.Infinite );
				}
			}
		}

		private void Item_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			var entry = sender as MappingModel;
			if( entry == null || string.IsNullOrEmpty( entry.OriginalText ) )
				return;

			// TODO: Merge ???
			
		}

		private void Collection_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			// Merge ...
			switch( e.Action ) {
				case NotifyCollectionChangedAction.Add:
					foreach( MappingModel model in e.NewItems ) {
						model.PropertyChanged -= Item_PropertyChanged;
						model.PropertyChanged += Item_PropertyChanged;
					}
					MergeMapping( Path.Combine( BaseProjectPath, (e.NewItems[0] as MappingModel).ProjectBasedFileName ),
									e.NewItems.Cast<MappingModel>().ToList() );

					break;

				case NotifyCollectionChangedAction.Remove:
					foreach( MappingModel model in e.OldItems ) {
						model.PropertyChanged -= Item_PropertyChanged;
						if( string.IsNullOrEmpty( model.OriginalText ) )
							continue;
					}
					//if( AutoRemoveMonitoringWhenFileChanged )
					RemoveMapping( Path.Combine( BaseProjectPath, (e.OldItems[0] as MappingModel).ProjectBasedFileName ),
									e.OldItems.Cast<MappingModel>().ToList() );
					break;

				case NotifyCollectionChangedAction.Replace:
					if( e.NewItems != null && e.OldItems != null &&
						e.NewItems.Count > 0 && e.OldItems.Count > 0 ) {
						MonitorDeferredChanged?.Invoke( this, MonitorEvents.MappingSquenceChanged,
											new MappingEventArgs(
												MonitorEvents.MappingDeleted, null, null,
												e.OldItems, e.NewItems
											) );
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					RemoveMapping( Path.Combine( BaseProjectPath, (e.OldItems[0] as MappingModel).ProjectBasedFileName ),
									e.OldItems.Cast<MappingModel>().ToList() );
					MonitorDeferredChanged?.Invoke( this, MonitorEvents.MappingDeleted,
											new MappingEventArgs (
												MonitorEvents.MappingDeleted, null, null,
												e.OldItems, e.NewItems
											) );
					break;
			}
		}

		private void AddMonFileListWithPriority( string fullPathFileName )
		{
			if( mMonFileList.Contains( fullPathFileName ) )
				return;

			var path = Path.GetDirectoryName( fullPathFileName );

			// try to insert existed path files
			var existed = mMonFileList.Where( x => Path.GetDirectoryName( x ) == path ).ToList();
			if( existed != null && existed.Count > 0 ) {
				existed.Add( fullPathFileName );
				existed = Utils.SortWithNumericOrdering( existed ).ToList();
				int idx = existed.IndexOf( fullPathFileName );

				// insert to first
				if( idx <= 0 ) {
					var next = existed[1];
					var nextIdx = mMonFileList.IndexOf( next );
					mMonFileList.Insert( nextIdx, fullPathFileName );
					return;
				}

				// insert to middle/last
				var prev = existed[idx - 1];
				var prevIdx = mMonFileList.IndexOf( prev );
				mMonFileList.Insert( prevIdx + 1, fullPathFileName );
				return;
			}

			var glossaryPath = Path.Combine( BaseProjectPath, GlossaryFolderName );
			for( int i = 0; i < mMonFileList.Count; ++i ) {
				var fileName = mMonFileList[i];

				if( fileName.StartsWith( glossaryPath ) == false ) {
					// this fileName is highest priority for project file
					mMonFileList.Insert( i, fullPathFileName );
					return;
				}

				// glossary files
				var p1 = Path.GetDirectoryName( fileName );
				if( p1.Length <= path.Length ) {
					mMonFileList.Insert( i, fullPathFileName );
					return;
				}
			}

			// insert to emprty list
			mMonFileList.Add( fullPathFileName );
		}

		#endregion // "private/protected data/methods"


		/// <summary>
		/// Add project/glossary file with its deserialized MappingEntry items to monitoring list
		/// </summary>
		/// <param name="fullPathFileName">Full-path file name to identify</param>
		/// <param name="data">Deserialized MappingEntry items</param>
		/// <returns>true for success</returns>
		public bool AddMonitoring( string fullPathFileName, IList<MappingEntry> data )
		{
			if( File.Exists( fullPathFileName ) == false || data == null ||
				mFileList.Contains( fullPathFileName ) == false )
				return false;

			// when create a new project, its coresponding .conf may be un-created yet...
			//if( string.IsNullOrWhiteSpace( fullPathFileName ) || data == null )
			//	return false;

			// not in same project folder...
			if( fullPathFileName != Path.Combine( BaseProjectPath, fullPathFileName ) )
				return false;

			AddMonFileListWithPriority( fullPathFileName );

			ObservableList<MappingModel> models = null;
			// clear old collection
			if( mMonFileDict.ContainsKey( fullPathFileName ) ) {
				models = mMonFileDict[fullPathFileName];
				foreach( var model in models ) {
					model.PropertyChanged -= Item_PropertyChanged;
				}
				models.CollectionChanged -= Collection_CollectionChanged;
				//models.Clear(); // DO NOT clear models, because someone would still occupy this collection.
				MergeMapping( fullPathFileName, null );
			}

			// create new collection and each item
			var projBaseFileName = fullPathFileName.Replace( BaseProjectPath, "" );
			models = new ObservableList<MappingModel>();
			foreach( var entry in data ) {
				models.Add( MappingModel.FromMappingEntry( entry, projBaseFileName, Item_PropertyChanged ) );
			}
			models.CollectionChanged += Collection_CollectionChanged;
			mMonFileDict[fullPathFileName] = models;
			MergeMapping( fullPathFileName, models );

			return true;
		}

		/// <summary>
		/// Add project/glossary file with its deserialized MappingEntry items to monitoring list
		/// </summary>
		/// <param name="fullPathFileName">Full-path file name to identify</param>
		/// <param name="models">Deserialized MappingModel items</param>
		/// <returns>true for success</returns>
		public bool AddMonitoring( string fullPathFileName, ObservableList<MappingModel> models )
		{
			if( File.Exists( fullPathFileName ) == false || models == null ||
				mFileList.Contains( fullPathFileName ) == false )
				return false;

			// when create a new project, its coresponding .conf may be un-created yet...
			//if( string.IsNullOrWhiteSpace( fullPathFileName ) || data == null )
			//	return false;

			// not in same project folder...
			if( fullPathFileName != Path.Combine( BaseProjectPath, fullPathFileName ) )
				return false;

			AddMonFileListWithPriority( fullPathFileName );

			// clear old mapping models
			if( mMonFileDict.ContainsKey( fullPathFileName ) ) {
				var coll = mMonFileDict[fullPathFileName];
				foreach( var model in coll ) {
					model.PropertyChanged -= Item_PropertyChanged;
				}
				coll.CollectionChanged -= Collection_CollectionChanged;
				MergeMapping( fullPathFileName, null );
			}

			mMonFileDict[fullPathFileName] = models;
			MergeMapping( fullPathFileName, models );
			foreach( var model in models ) {
				model.PropertyChanged -= Item_PropertyChanged;
				model.PropertyChanged += Item_PropertyChanged;
			}
			models.CollectionChanged -= Collection_CollectionChanged;
			models.CollectionChanged += Collection_CollectionChanged;

			return true;
		}

		/// <summary>
		/// Remove monitoring file
		/// </summary>
		/// <param name="fullPathFileName">Monitoring full-path file name</param>
		/// <returns>true for success</returns>
		public bool RemoveMonitoring( string fullPathFileName )
		{
			bool found = false;
			if( mMonFileDict.ContainsKey( fullPathFileName ) ) {
				var models = mMonFileDict[fullPathFileName];
				foreach( var entry in models )
					entry.PropertyChanged -= Item_PropertyChanged;
				models.CollectionChanged -= Collection_CollectionChanged;
				//models.Clear(); // DO NOT clear models, because someone would still occupy this collection.
				mMonFileDict.Remove( fullPathFileName );
				MergeMapping( fullPathFileName, null );
				found = true;
			}

			if( mMonFileList.Contains( fullPathFileName ) ) {
				mMonFileList.Remove( fullPathFileName );
				found = true;
			}

			return found;
		}

		/// <summary>
		/// Change monitoring file name
		/// </summary>
		/// <param name="oldFullPath">Old full-path file name</param>
		/// <param name="newFullPath">New full-path file name</param>
		/// <returns>true for success</returns>
		public bool ChangeMonitoringFilePath( string oldFullPath, string newFullPath )
		{
			// TODO: Check newFullPath is existed in mMonFileXXX ??
			if( newFullPath.StartsWith( this.BaseProjectPath ) == false )
				return false;

			var projBasedFileName = newFullPath.Replace( this.BaseProjectPath, "" );
			bool found = false;

			if( mMonFileDict.ContainsKey( oldFullPath ) ) {
				var models = mMonFileDict[oldFullPath];
				foreach( var entry in models ) {
					entry.PropertyChanged -= Item_PropertyChanged;
					entry.ProjectBasedFileName = projBasedFileName;
					entry.PropertyChanged += Item_PropertyChanged;
				}
				mMonFileDict.Remove( oldFullPath );
				mMonFileDict[newFullPath] = models;
				found = true;
			}

			if( mMonFileList.Contains( oldFullPath ) ) {
				mMonFileList.Remove( oldFullPath );
				AddMonFileListWithPriority( newFullPath );
				found = true;
			}

			return found;
		}

		/// <summary>
		/// Clear all monitoring files and MappingModel collections
		/// </summary>
		public void ClearAllMonitoring()
		{
			foreach( var kvp in mMonFileDict ) {
				var models = kvp.Value;
				foreach( var entry in models ) {
					entry.PropertyChanged -= Item_PropertyChanged;
				}
				models.CollectionChanged -= Collection_CollectionChanged;
				//models.Clear(); // DO NOT clear models, because someone would still occupy this collection.
			}

			// clear mapping models
			CleanAllMapping();

			// clear monitoring file data
			mMonFileDict.Clear();
			mMonFileList.Clear();
		}

		/// <summary>
		/// Reload all file list of BaseProjectPath
		/// </summary>
		public void ReloadFileList()
		{
			if( Directory.Exists( BaseProjectPath ) == false )
				return;

			mFileList = new ObservableList<string>( Directory.GetFiles( BaseProjectPath, "*.*", SearchOption.AllDirectories ) );
			mFileList.AddRange( Directory.GetDirectories( BaseProjectPath, "*", SearchOption.AllDirectories ) );
		}

		/// <summary>
		/// Get Mapping collection by file name
		/// </summary>
		/// <param name="fullPathFileName">Full-path file name</param>
		/// <returns>null for failed</returns>
		public ObservableList<MappingModel> GetMappingCollection( string fullPathFileName )
		{
			if( mMonFileDict.ContainsKey( fullPathFileName ) == false )
				return null;
			return mMonFileDict[fullPathFileName];
		}

		/// <summary>
		/// Start monitoring and raising events
		/// </summary>
		/// <returns>true for suceess</returns>
		public bool Start()
		{
			try {
				lock( mWatcher )
					mWatcher.EnableRaisingEvents = true;
			}
			catch {
				return false;
			}
			return true;
		}

		/// <summary>
		/// Stop monitoring
		/// </summary>
		public void Stop()
		{
			try {
				lock( mWatcher )
					mWatcher.EnableRaisingEvents = false;
			}
			catch { }
		}

	}
}
