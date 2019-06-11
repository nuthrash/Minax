using Minax.Domain.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Minax
{
	/// <summary>
	/// General utility methods
	/// </summary>
	public static class Utils
	{

		#region "Serialize/Deserialize"

		/// <summary>
		/// Serialize a .Net instance/object to a XML file
		/// </summary>
		/// <typeparam name="T">Source data type</typeparam>
		/// <param name="obj">Source object</param>
		/// <param name="filename">Target full path filename</param>
		/// <returns>true for success</returns>
		public static bool SerializeToXml<T>( T obj, string filename )
		{
			if( obj == null || string.IsNullOrWhiteSpace( filename ) )
				return false;

			try {
				string tmpFile = Path.GetTempFileName();

				// save to tmpFile first
				using( Stream fs = File.Open( tmpFile, FileMode.OpenOrCreate, FileAccess.ReadWrite ) )
				using( StreamWriter sw = new StreamWriter( fs, Encoding.UTF8 ) ) {
					System.Xml.Serialization.XmlSerializer xs =
									new System.Xml.Serialization.XmlSerializer( typeof( T ) );

					// serialize to xml
					MemoryStream ms = new MemoryStream();
					xs.Serialize( XmlWriter.Create( ms ), obj );
					ms.Flush();
					ms.Seek( 0, SeekOrigin.Begin );

					XmlDocument xd = new XmlDocument();
					ms.Flush();
					ms.Seek( 0, SeekOrigin.Begin );
					xd.Load( ms );

					xd.Save( sw );
				}

				// move tmpFile to filename
				if( File.Exists( filename ) )
					File.Delete( filename );

				File.Move( tmpFile, filename );
			}
			catch {
				return false;
			}
			return true;
		}

		/// <summary>
		/// Deserialize a .Net object from a XML file
		/// </summary>
		/// <param name="filename">Source file name</param>
		/// <returns>null or Deserialized object</returns>
		public static T DeserializeFromXml<T>( string filename )
		{
			if( string.IsNullOrEmpty( filename ) || File.Exists( filename ) == false )
				return default( T );

			T conf = default( T );
			try {
				using( Stream fs = File.Open( filename, FileMode.Open, FileAccess.Read ) ) {
					System.Xml.Serialization.XmlSerializer xs =
									new System.Xml.Serialization.XmlSerializer( typeof( T ) );
					conf = (T)xs.Deserialize( fs );
				}
			}
			catch {
				return default( T );
			}
			return conf;
		}

		/// <summary>
		///  Deserialize a .Net object from a XML stream
		/// </summary>
		/// <param name="inputStream">Source XML input stream</param>
		/// <returns>null or Deserialized object</returns>
		public static T DeserializeFromXml<T>( Stream inputStream )
		{
			T conf = default( T );
			try {
				System.Xml.Serialization.XmlSerializer xs =
									new System.Xml.Serialization.XmlSerializer( typeof( T ) );
				conf = (T)xs.Deserialize( inputStream );
			}
			catch {
				return default( T );
			}
			return conf;
		}

		// https://stackoverflow.com/questions/11883913/convert-deserialization-method-to-asyncs
		public static Task<T> DeserializeObjectAsync<T>( string xml )
		{
			using( StringReader reader = new StringReader( xml ) ) {
				using( XmlReader xmlReader = XmlReader.Create( reader ) ) {
					DataContractSerializer serializer =
						new DataContractSerializer( typeof( T ) );
					T theObject = (T)serializer.ReadObject( xmlReader );
					return Task.FromResult( theObject );
				}
			}
		}


		#endregion // "Serialize/Deserialize"


		#region "Sorting"

		/// <summary>
		/// Sort a string list with natural number ordering
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static IEnumerable<string> SortWithNumericOrdering( IEnumerable<string> list )
		{
			if( list.Count() <= 0 )
				return new string[] { };

			int maxLen = list.Select( s => s.Length ).Max();

			return list.Select( s => new {
				OrgStr = s,
				SortStr = Regex.Replace( s, @"(\d+)|(\D+)", m => m.Value.PadLeft( maxLen, char.IsDigit( m.Value[0] ) ? ' ' : '\xffff' ) )
			} )
			.OrderBy( x => x.SortStr )
			.Select( x => x.OrgStr );
		}
		#endregion

		#region "File Formats"

		// https://stackoverflow.com/a/9466515, https://stackoverflow.com/a/27976558
		public static bool IsFileUtf8Text( string fullPathFileName )
		{
			if( File.Exists( fullPathFileName ) == false )
				return false;

			var utf8NoBom = new UTF8Encoding( false );
			try {
				using( var reader = new StreamReader( fullPathFileName, utf8NoBom ) ) {
					reader.Read();
					if( Equals( reader.CurrentEncoding, utf8NoBom ) == false )
						return true;
				}
			}
			catch {
				return false;
			}
			return false;
		}

		// 0xEF 0xBB 0xBF
		private static byte[] sUtf8Bom = null;

		// http://www.unicode.org/faq/utf_bom.html
		public static bool IsByteArrayStartsWithUtf8Bom( byte[] first3Bytes )
		{
			if( first3Bytes == null || first3Bytes.Length <= 2 )
				return false;

			if( sUtf8Bom == null ) {
				var utf8WithBom = new UTF8Encoding( true );
				sUtf8Bom = utf8WithBom.GetPreamble();
			}

			// NO need to chech which is shorter...
			// var shorter = sUtf8Bom.Length <= first3Bytes.Length ? sUtf8Bom : first3Bytes;
			for( int i = 0; i < sUtf8Bom.Length; ++i ) {
				if( sUtf8Bom[i] != first3Bytes[i] )
					return false;
			}

			// return preamble.Where( ( p, i ) => p != bytes[i] ).Any();
			return true;
		}

		#endregion

		#region "Enum, Converters"

		private static readonly SortedDictionary<TextCategory, string> sTextCategoryDescriptions = new SortedDictionary<TextCategory, string>();
		public static IReadOnlyList<string> GetAllTextCategoryL10nStrings()
		{
			if( sTextCategoryDescriptions.Count <= 0 ) {
				var dict = sTextCategoryDescriptions;
				dict[TextCategory.Undefined] = Languages.TextCategory.Str0Undefined;

				dict[TextCategory.Character] = Languages.TextCategory.Str0Character;
				dict[TextCategory.FamilyName] = Languages.TextCategory.Str0FamilyName;
				dict[TextCategory.MiddleName] = Languages.TextCategory.Str0MiddleName;
				dict[TextCategory.NickName] = Languages.TextCategory.Str0NickName;
				dict[TextCategory.AliasName] = Languages.TextCategory.Str0AliasName;
				dict[TextCategory.ReincarnatedName] = Languages.TextCategory.Str0ReincarnatedName;
				dict[TextCategory.OtherName] = Languages.TextCategory.Str0OtherName;

				dict[TextCategory.Organization] = Languages.TextCategory.Str0Organization;
				dict[TextCategory.JobPosition] = Languages.TextCategory.Str0JobPosition;
				dict[TextCategory.Relation] = Languages.TextCategory.Str0Relation;

				dict[TextCategory.Species] = Languages.TextCategory.Str0Species;
				dict[TextCategory.Animal] = Languages.TextCategory.Str0Animal;
				dict[TextCategory.Monster] = Languages.TextCategory.Str0Monster;

				dict[TextCategory.Material] = Languages.TextCategory.Str0Material;
				dict[TextCategory.Inventory] = Languages.TextCategory.Str0Inventory;

				dict[TextCategory.Food] = Languages.TextCategory.Str0Food;
				dict[TextCategory.Clothing] = Languages.TextCategory.Str0Clothing;
				dict[TextCategory.Housing] = Languages.TextCategory.Str0Housing;
				dict[TextCategory.Transportation] = Languages.TextCategory.Str0Transportation;
				dict[TextCategory.Education] = Languages.TextCategory.Str0Education;
				dict[TextCategory.Entertainment] = Languages.TextCategory.Str0Entertainment;
				dict[TextCategory.Music] = Languages.TextCategory.Str0Music;

				dict[TextCategory.HairStyle] = Languages.TextCategory.Str0HairStyle;
				dict[TextCategory.Shoe] = Languages.TextCategory.Str0Shoe;

				dict[TextCategory.Facility] = Languages.TextCategory.Str0Facility;
				dict[TextCategory.Building] = Languages.TextCategory.Str0Building;
				dict[TextCategory.Location] = Languages.TextCategory.Str0Location;
				dict[TextCategory.City] = Languages.TextCategory.Str0City;

				dict[TextCategory.Action] = Languages.TextCategory.Str0Action;
				dict[TextCategory.Phenomenon] = Languages.TextCategory.Str0Phenomenon;
				dict[TextCategory.Property] = Languages.TextCategory.Str0Property;
				dict[TextCategory.Social] = Languages.TextCategory.Str0Social;

				dict[TextCategory.BodyOrgan] = Languages.TextCategory.Str0BodyOrgan;

				dict[TextCategory.Color] = Languages.TextCategory.Str0Color;

				dict[TextCategory.Weapon] = Languages.TextCategory.Str0Weapon;
				dict[TextCategory.Maneuver] = Languages.TextCategory.Str0Maneuver;
				dict[TextCategory.Magic] = Languages.TextCategory.Str0Magic;
				dict[TextCategory.Alchemy] = Languages.TextCategory.Str0Alchemy;

				dict[TextCategory.Science] = Languages.TextCategory.Str0Science;
				dict[TextCategory.Book] = Languages.TextCategory.Str0Book;
				dict[TextCategory.Game] = Languages.TextCategory.Str0Game;
				dict[TextCategory.Language] = Languages.TextCategory.Str0Language;
				dict[TextCategory.Translator] = Languages.TextCategory.Str0Translator;
				dict[TextCategory.MiscTerms] = Languages.TextCategory.Str0MiscTerms;

				dict[TextCategory.Verbs] = Languages.TextCategory.Str0Verbs;
				dict[TextCategory.Noun] = Languages.TextCategory.Str0Noun;
				dict[TextCategory.Adjective] = Languages.TextCategory.Str0Adjective;
				dict[TextCategory.Adverb] = Languages.TextCategory.Str0Adverb;
				dict[TextCategory.Conjunction] = Languages.TextCategory.Str0Conjunction;
				dict[TextCategory.Determiners] = Languages.TextCategory.Str0Determiners;
				dict[TextCategory.Preposition] = Languages.TextCategory.Str0Preposition;
				dict[TextCategory.Pronouns] = Languages.TextCategory.Str0Pronouns;

			}

			return sTextCategoryDescriptions.Values.ToList();
		}

		public static string GetTextCategoryL10nString( TextCategory value )
		{
			if( sTextCategoryDescriptions.Count <= 0 ) {
				_ = GetAllTextCategoryL10nStrings();
			}

			if( sTextCategoryDescriptions.ContainsKey( value ) == false )
				return Languages.TextCategory.Str0Undefined;

			return sTextCategoryDescriptions[value];
		}

		public static TextCategory GetTextCategoryL10nValue( string description )
		{
			if( string.IsNullOrWhiteSpace( description ) )
				return TextCategory.Undefined;

			foreach( var kvp in sTextCategoryDescriptions ) {
				if( description == kvp.Value )
					return kvp.Key;
			}

			var result = TextCategory.Undefined;
			// maybe description is TextCategory enum value string
			Enum.TryParse( description, out result );

			return result;
		}

		#endregion


		#region "Security, Encrypt, Decrypt"

		public static string EncryptToBase64( string sourceText, string cryptoKey )
		{
			if( string.IsNullOrEmpty( sourceText ) || string.IsNullOrWhiteSpace( cryptoKey ) )
				return string.Empty;

			var srcBytes = Encoding.UTF8.GetBytes( sourceText );
			using( var aes = System.Security.Cryptography.Aes.Create() )
			using( var sha256 = System.Security.Cryptography.SHA256.Create() )
			using( var md5 = System.Security.Cryptography.MD5.Create() ) {
				aes.Mode = System.Security.Cryptography.CipherMode.CBC;
				aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				aes.Key = sha256.ComputeHash( Encoding.UTF8.GetBytes( cryptoKey ) );
				aes.IV = md5.ComputeHash( Encoding.UTF8.GetBytes( cryptoKey ) );

				var xform = aes.CreateEncryptor();
				return Convert.ToBase64String( xform.TransformFinalBlock( srcBytes, 0, srcBytes.Length ) );
			}
		}

		public static string DecryptFromBas64( string base64, string cryptoKey )
		{
			if( string.IsNullOrEmpty( base64 ) || string.IsNullOrWhiteSpace( cryptoKey ) )
				return string.Empty;

			var encBytes = Convert.FromBase64String( base64 );
			using( var aes = System.Security.Cryptography.Aes.Create() )
			using( var sha256 = System.Security.Cryptography.SHA256.Create() )
			using( var md5 = System.Security.Cryptography.MD5.Create() ) {
				aes.Mode = System.Security.Cryptography.CipherMode.CBC;
				aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				aes.Key = sha256.ComputeHash( Encoding.UTF8.GetBytes( cryptoKey ) );
				aes.IV = md5.ComputeHash( Encoding.UTF8.GetBytes( cryptoKey ) );

				var xform = aes.CreateDecryptor();
				return Encoding.UTF8.GetString( xform.TransformFinalBlock( encBytes, 0, encBytes.Length ) );
			}
		}

		#endregion
	}
}
