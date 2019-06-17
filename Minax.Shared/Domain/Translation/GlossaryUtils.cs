using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Minax.Domain.Translation.SupportedLanguagesExtensions;

namespace Minax.Domain.Translation
{
	public static class GlossaryUtils
	{


		#region "Project Directory and other folders/MappingTable files"

		/// <summary>
		/// Open and start monitoring glossary files
		/// </summary>
		/// <param name="projFullPathFileName">Project full-path file name</param>
		/// <param name="projEntries">Project Mapping entries list</param>
		/// <param name="monitor">MappingMonitor for this project's base path</param>
		/// <param name="engineFolderName">Corresponding translator/translation engine folder name</param>
		/// <param name="srcLang">Supported source language</param>
		/// <param name="tgtLang">Supported target language</param>
		/// <returns>true for success</returns>
		public static async Task<bool> OpenAndMonitorGlossaryFiles( string projFullPathFileName, IList<MappingEntry> projEntries,
											MappingMonitor monitor,
											string engineFolderName = "Excite",
											SupportedSourceLanguage srcLang = SupportedSourceLanguage.Japanese,
											SupportedTargetLanguage tgtLang = SupportedTargetLanguage.ChineseTraditional )
		{
			if( string.IsNullOrWhiteSpace( projFullPathFileName ) || monitor == null )
				return false;

			var baseProjectPath = Path.GetDirectoryName( projFullPathFileName );
			if( string.IsNullOrWhiteSpace( baseProjectPath ) || 
				Directory.Exists( baseProjectPath ) == false ||
				monitor.BaseProjectPath != baseProjectPath )
				return false;

			var glossaryPath = monitor.GlossaryPath;
			if( Directory.Exists( glossaryPath ) == false ||
				string.IsNullOrWhiteSpace( engineFolderName ) ||
				srcLang == SupportedSourceLanguage.AutoDetect )
				return false;

			monitor.Stop();
			foreach( var fn in monitor.MonitoringFileList.ToList() ) {
				if( fn == projFullPathFileName )
					continue;

				monitor.RemoveMonitoring( fn );
			}
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

			var engine = new FileHelpers.DelimitedFileEngine<MappingEntry>( System.Text.Encoding.UTF8 );
			foreach( var field in engine.Options.Fields )
				field.IsOptional = true;
			engine.Options.Fields[0].IsOptional = false; // OriginalText
			// Switch error mode off
			engine.ErrorManager.ErrorMode = FileHelpers.ErrorMode.IgnoreAndContinue;

			List<(string, List<MappingEntry>)> glossaries = new List<(string, List<MappingEntry>)>();
			foreach( var fileName in listFiles ) {
				try {
					var fileExt = Path.GetExtension( fileName );

					switch( fileExt ) {
						case MappingEntryCsv.FileExtension:
							engine.Options.Delimiter = ",";
							break;

						case MappingEntryTsv.FileExtension:
							engine.Options.Delimiter = "\t";
							break;
					}

					var list = engine.ReadFileAsList( fileName );
					if( list == null ) {
						// try another delmiter
						engine.Options.Delimiter = engine.Options.Delimiter == "," ? "\t" : ",";
						list = engine.ReadFileAsList( fileName );
					}

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
			var projMappingColl = monitor.GetMappingCollection( projFullPathFileName );
			monitor.RemoveMonitoring( projFullPathFileName );

			foreach( var glossary in glossaries ) {
				monitor.AddMonitoring( glossary.Item1, glossary.Item2 );
			}

			// finally, add back project.conf's mapping for highest priority
			if( projMappingColl == null )
				monitor.AddMonitoring( projFullPathFileName, projEntries );
			else
				monitor.AddMonitoring( projFullPathFileName, projMappingColl );

			monitor.Start();
			return true;
		}

		/// <summary>
		/// Try parse and extrace glossary MappingEntry
		/// </summary>
		/// <param name="glossaryFullPathFileName">Full-path glossary file name</param>
		/// <returns>Extracted MappingEntry list</returns>
		public static List<MappingEntry> TryParseAndExtractMappingEntries( string glossaryFullPathFileName )
		{
			if( File.Exists( glossaryFullPathFileName ) == false )
				return null;

			var engine = new FileHelpers.DelimitedFileEngine<MappingEntry>( System.Text.Encoding.UTF8 );
			foreach( var field in engine.Options.Fields )
				field.IsOptional = true;
			engine.Options.Fields[0].IsOptional = false; // OriginalText
			// Switch error mode off
			engine.ErrorManager.ErrorMode = FileHelpers.ErrorMode.IgnoreAndContinue;

			try {
				var fileExt = Path.GetExtension( glossaryFullPathFileName );

				switch( fileExt ) {
					case MappingEntryCsv.FileExtension:
						engine.Options.Delimiter = ",";
						break;

					case MappingEntryTsv.FileExtension:
						engine.Options.Delimiter = "\t";
						break;
				}

				var list = engine.ReadFileAsList( glossaryFullPathFileName );
				if( list == null ) {
					// try another delmiter
					engine.Options.Delimiter = engine.Options.Delimiter == "," ? "\t" : ",";
					list = engine.ReadFileAsList( glossaryFullPathFileName );
				}


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
		}

		public static bool IsFileConcernedByProject( string projFullPathFileName, string glossaryFullPathFileName,
									string engineFolderName = "Excite",
									SupportedSourceLanguage srcLang = SupportedSourceLanguage.Japanese,
									SupportedTargetLanguage tgtLang = SupportedTargetLanguage.ChineseTraditional )
		{
			if( string.IsNullOrWhiteSpace( projFullPathFileName ) ||
				string.IsNullOrWhiteSpace( glossaryFullPathFileName ) ||
				srcLang == SupportedSourceLanguage.AutoDetect )
				return false;

			if( glossaryFullPathFileName == projFullPathFileName )
				return true;

			var baseProjPath = Path.GetDirectoryName( projFullPathFileName );
			if( glossaryFullPathFileName.StartsWith( baseProjPath ) == false )
				return false;

			var fileName = Path.GetFileName( glossaryFullPathFileName );

			var glossaryPath = Path.Combine( baseProjPath, MappingMonitor.GlossaryFolderName );
			if( Path.Combine( glossaryPath, fileName ) == glossaryFullPathFileName )
				return true;

			var enginePath = Path.Combine( glossaryPath, engineFolderName );
			if( Path.Combine( enginePath, fileName ) == glossaryFullPathFileName )
				return true;

			var src2tgt = $"{srcLang.ToString()}2{tgtLang.ToString()}";
			var langPath = Path.Combine( enginePath, src2tgt );
			if( Path.Combine( langPath, fileName ) == glossaryFullPathFileName )
				return true;

			return false;
		}

		/// <summary>
		/// Extrac sample empty glossary sub-folders
		/// </summary>
		/// <param name="targetPath">Target full-path</param>
		/// <returns>true for sucess</returns>
		public static bool ExtractSampleFolders( string targetPath )
		{
			if( string.IsNullOrWhiteSpace( targetPath ) )
				return false;

			try {
				string zipFileName = Path.GetTempFileName();

				var sampleZip = Properties.Resources.WebNovelsEmpty;
				File.WriteAllBytes( zipFileName, sampleZip );

				// NOTE: DO NOT clear old targetPath!! Just overwrite it!
				ZipFile.ExtractToDirectory( zipFileName, targetPath );

				File.Delete( zipFileName );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.WriteLine(
					$"[Minax.Web.Translation.GlossaryUtils.ExtractSampleDirectory()] cannot extract sample directory to {targetPath}! Exception: {ex.InnerException}" );
				return false;
			}
			return true;
		}

		/// <summary>
		/// Get glossary file name list under Glossary full-path folder
		/// </summary>
		/// <param name="glossaryPath">Full-path glossary sub-folder</param>
		/// <param name="engineFolderName">Translator engine sub-folder name</param>
		/// <param name="srcLang">Supported source language</param>
		/// <param name="tgtLang">Supported target language</param>
		/// <returns>Full-path file name list of found glossary files</returns>
		public static IList<string> GetGlossaryFiles( string glossaryPath, string engineFolderName = "Excite",
										SupportedSourceLanguage srcLang = SupportedSourceLanguage.Japanese,
										SupportedTargetLanguage tgtLang = SupportedTargetLanguage.ChineseTraditional )
		{
			if( Directory.Exists( glossaryPath ) == false ||
				string.IsNullOrWhiteSpace( engineFolderName ) ||
				srcLang == SupportedSourceLanguage.AutoDetect )
				return null;

			// the files in <glossaryPath>/<engineFolderName>/<langFolder>/*.* are specific srcLang to tgtLang glossary files
			// the files in <glossaryPath>/<engineFolderName>/*.* are language-ignored, so put them be careful!
			// the files in <glossaryPath>/*.* are engine-ignored, so put them be careful!!!

			// current supproted MappingEntry file extensions(.csv, .tsv, etc.)
			var supExtensions = new List<string> {
				MappingEntryCsv.FileExtension,
				MappingEntryTsv.FileExtension,
			};

			var listAllEngine = new List<string>();
			foreach( var fileName in Directory.EnumerateFiles( glossaryPath, "*.*", SearchOption.TopDirectoryOnly ) ) {
				if( supExtensions.Contains( Path.GetExtension( fileName ) ) )
					listAllEngine.Add( fileName );
			}

			var enginePath = Path.Combine( glossaryPath, engineFolderName );
			if( Directory.Exists( enginePath ) == false ) {
				return Minax.Utils.SortWithNumericOrdering( listAllEngine ).ToList();
			}

			// get files in enginePath
			var listAllLang = new List<string>();
			foreach( var fileName in Directory.EnumerateFiles( enginePath, "*.*", SearchOption.TopDirectoryOnly ) ) {
				if( supExtensions.Contains( Path.GetExtension( fileName ) ) )
					listAllLang.Add( fileName );
			}

			// concate listAllLang + listAllEngine back to listAllLang
			listAllLang = Minax.Utils.SortWithNumericOrdering( listAllLang ).Concat(
						Minax.Utils.SortWithNumericOrdering( listAllEngine ) ).ToList();

			var langFolder = $"{srcLang.ToString()}2{tgtLang.ToString()}"; // like "Japanese2ChineseTraditional"
			var langPath = Path.Combine( enginePath, langFolder );
			if( Directory.Exists( langPath ) == false )
				return listAllLang;

			// get files in langPath
			var list = new List<string>();
			foreach( var fileName in Directory.EnumerateFiles( langPath, "*.*", SearchOption.TopDirectoryOnly ) ) {
				if( supExtensions.Contains( Path.GetExtension( fileName ) ) )
					list.Add( fileName );
			}

			// the returned list is sorted by the sequence:
			//     <glossaryPath>/<engineFolder>/<langFolder>/*.* -> <glossaryPath>/<engineFolder>/*.* -> <glossaryPath>/*.*,
			// the later is high priority for overwriting previous terms!!
			return Minax.Utils.SortWithNumericOrdering( list ).Concat( listAllLang ).ToList();
		}

		#endregion

		#region "Extension"

		public static IEnumerable<string> NumericSort( this IEnumerable<string> list )
		{
			return Utils.SortWithNumericOrdering( list );
		}

		#endregion

		}
}
