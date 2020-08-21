using Minax.Domain.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
		/// Field separator string for storing some fields to single string with it
		/// </summary>
		public static string FieldSeparator => "⒇₢";

		/// <summary>
		/// Project Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Supported source language for text Mapping
		/// </summary>
		public SupportedSourceLanguage SourceLanguage { get; set; } = SupportedSourceLanguage.Japanese;
		/// <summary>
		/// Supported target language for text Mapping
		/// </summary>
		public SupportedTargetLanguage TargetLanguage { get; set; } = SupportedTargetLanguage.ChineseTraditional;

		/// <summary>
		/// Cover image relative path to this TranslationProject file path
		/// </summary>
		public string CoverImageRelativePath { get; set; }

		/// <summary>
		/// Description text about this project
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Remote site URL for this project such as Web Novel site, Article site, etc.
		/// </summary>
		public string RemoteSite { get; set; }

		/// <summary>
		/// Last modified date
		/// </summary>
		public DateTime? LastModifiedDate { get; set; }

		/// <summary>
		/// Mapping table for replacing OriginalText (source text) to MappingText (target text)
		/// </summary>
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
