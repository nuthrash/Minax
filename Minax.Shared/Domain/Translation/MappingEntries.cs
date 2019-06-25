using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Minax.Domain.Translation
{
	/// <summary>
	/// Mapping Entry base type
	/// </summary>
	/// <remarks>FileHelpers library can only handle fields or simple get/set properties, not the properties
	/// with many checking procedures, so the sequence of private field declaration for each
	/// INotifyPropertyChanged property is very important!
	/// DO NOT place field declaration together!</remarks>
	[Serializable]
	[XmlInclude( typeof( MappingEntryCsv ) )]
	[XmlInclude( typeof( MappingEntryTsv ) )]
	[XmlInclude( typeof( MappingMonitor.MappingModel ) )]
	[IgnoreEmptyLines( true ), IgnoreFirst( 0 ), DelimitedRecord( "\t" )]
	public class MappingEntry
	{
		[FieldValueDiscarded]
		[XmlIgnore]
		public const string FileExtension = ".tsv";

		/// <summary>
		/// Original string/text, cannot be empty but may full of whitespace characters
		/// </summary>
		public string OriginalText { get; set; }

		/// <summary>
		/// Mapping string/text
		/// </summary>
		public string MappingText { get; set; }

		/// <summary>
		/// This MappingEntry text category
		/// </summary>
		public TextCategory? Category { get; set; }

		/// <summary>
		/// Basic description for this MappingEntry
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Extra description for this MappingEntry
		/// </summary>
		public string Comment { get; set; }

	}

	// Tab-separated values (.tsv) 
	[IgnoreEmptyLines( true ), IgnoreFirst( 0 ), DelimitedRecord( "\t" )]
	public class MappingEntryTsv : MappingEntry
	{
		[FieldValueDiscarded]
		[XmlIgnore]
		public new const string FileExtension = ".tsv";

		public static IList<MappingEntryTsv> ReadFile( FileHelperEngine<MappingEntryTsv> engine, string fullPathFileName )
		{
			if( engine == null || File.Exists( fullPathFileName ) == false ||
				Path.GetExtension( fullPathFileName ) != FileExtension )
				return null;

			return engine.ReadFileAsList( fullPathFileName );
		}
	}


	// Comma-separated values (.csv) 
	[IgnoreEmptyLines( true ), IgnoreFirst( 0 ), DelimitedRecord( "," )]
	public class MappingEntryCsv : MappingEntry
	{
		[FieldValueDiscarded]
		[XmlIgnore]
		public new const string FileExtension = ".csv";


		public static IList<MappingEntryCsv> ReadFile( FileHelperEngine<MappingEntryCsv> engine, string fullPathFileName )
		{
			if( engine == null || File.Exists( fullPathFileName ) == false ||
				Path.GetExtension( fullPathFileName ) != FileExtension )
				return null;

			return engine.ReadFileAsList( fullPathFileName );
		}
	}
}
