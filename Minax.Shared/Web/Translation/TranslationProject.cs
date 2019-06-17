using Minax.Domain.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using static Minax.Domain.Translation.SupportedLanguagesExtensions;

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

		public string CoverImageSource { get; set; }

		public string Description { get; set; }

		public DateTime? LastModifiedDate { get; set; }

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
	}
}
