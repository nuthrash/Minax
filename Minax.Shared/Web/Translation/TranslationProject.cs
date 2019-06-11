using Minax.Domain.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using static Minax.Domain.Translation.SupportedLanguages;

namespace Minax.Web.Translation
{
	/// <summary>
	/// Translation Project facilities
	/// </summary>
	[Serializable]
	public class TranslationProject
	{
		/// <summary>
		/// Default project file extenstion
		/// </summary>
		public static string FileExtension => ".conf";

		/// <summary>
		/// Project Name
		/// </summary>
		public string Name { get; set; }

		public SupportedSourceLanguage SourceLanguage { get; set; } = SupportedSourceLanguage.Japanese;
		public SupportedTargetLanguage TargetLanguage { get; set; } = SupportedTargetLanguage.ChineseTraditional;

		public string ApiKeyGoogleTranslator { get; set; }
		public string ApiSnGoogleTranslator { get; set; }
		public string ApiKeyMicrosoftTranslator { get; set; }  // Bing/Azure
		public string ApiKeyBaiduTranslator { get; set; }
		public string ApiKeyYoudaoTranslator { get; set; }

		public List<MappingEntry> MappingTable { get; set; } = new List<MappingEntry>();


		#region "Serialize/Deserialize"

		/// <summary>
		/// Serialize a .Net instance/object to a XML file
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="conf"></param>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static bool SerializeToXml( TranslationProject conf, string filename )
		{
			if( conf == null || string.IsNullOrEmpty( filename ) )
				return false;

			return Minax.Utils.SerializeToXml<TranslationProject>( conf, filename );
		}

		/// <summary>
		/// Deserialize a .Net object from a XML file
		/// </summary>
		/// <param name="filename">Target file name</param>
		/// <returns>null or Deserialized object</returns>
		public static TranslationProject DeserializeFromXml( string filename )
		{
			if( string.IsNullOrEmpty( filename ) || File.Exists( filename ) == false )
				return null;

			return Minax.Utils.DeserializeFromXml<TranslationProject>( filename );
		}

		#endregion // Serialize

		#region "Project Directory and other folders/MappingTable files"

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
					$"[Minax.Web.TranslationProject.ExtractSampleDirectory()] cannot extract sample directory to {targetPath}! Exception: {ex.InnerException}" );
				return false;
			}
			return true;
		}

		public static IList<string> GetGlossaryFiles( string glossaryPath, string engineFolderName = "Excite",
										SupportedSourceLanguage srcLang = SupportedSourceLanguage.Japanese,
										SupportedTargetLanguage tgtLang = SupportedTargetLanguage.ChineseTraditional )
		{
			if( Directory.Exists( glossaryPath ) == false ||
				string.IsNullOrWhiteSpace( engineFolderName ) ||
				srcLang == SupportedSourceLanguage.AutoDetect )
				return null;

			// the files in <glossaryPath>/<engineFolderName>/langFolder are specific srcLang to tgtLang translating files
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
	}
}
