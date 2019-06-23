using Minax.Web.Translation;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace MinaxWebTranslator.Properties
{
	/// <summary>
	/// This is the Settings static class that can be used in your Core solution or in any
	/// of your client applications. All settings are laid out the same exact way with getters
	/// and setters. 
	/// </summary>
	internal partial class Settings
	{
		public static Settings Default => defaultInstance;
		private static readonly Settings defaultInstance = new Settings();
		private const string StringSeparator1 = "Ͼ‰";
		private const string StringSeparator2 = "𝄆ϗ";

		public List<string> RecentProjects {
			get {
				if( RecentProjectsSave != null )
					return RecentProjectsSave;

				RecentProjectsSave = new List<string>();
				if( Preferences.ContainsKey( RecentProjectsKey ) == false ) {
					//Preferences.Set( RecentProjectsKey, string.Empty );
					return RecentProjectsSave;
				}

				var tmpStr = Preferences.Get( RecentProjectsKey, string.Empty );
				if( string.IsNullOrWhiteSpace( tmpStr ) )
					return RecentProjectsSave;

				var tmpLists = tmpStr.Split( new[] { StringSeparator1 }, StringSplitOptions.RemoveEmptyEntries );
				if( tmpLists == null || tmpLists.Length <= 0 )
					return RecentProjectsSave;

				RecentProjectsSave.Clear();
				RecentProjectsSave.AddRange( tmpLists );
				return RecentProjectsSave;

			}
			set {
				if( value == null || value.Count <= 0 ) {
					Preferences.Set( RecentProjectsKey, string.Empty );
					if( value != null )
						RecentProjectsSave = value;
					RecentProjectsSave.Clear();
					return;
				}

				Preferences.Set( RecentProjectsKey, ConcatenateStringList2SingleString1( value ) );
				RecentProjectsSave = value;
			}
		}
		private const string RecentProjectsKey = "RecentProject";
		//private static List<string> RecentProjectsDefault = new List<string>();
		private static List<string> RecentProjectsSave = null;

		public List<string> CustomGlossaryFileListLocations {
			get {
				if( CustomGlossaryFileListLocationsSave != null )
					return CustomGlossaryFileListLocationsSave;

				CustomGlossaryFileListLocationsSave = new List<string>();
				if( Preferences.ContainsKey( CustomGlossaryFileListLocationsKey ) == false ) {
					return CustomGlossaryFileListLocationsSave;
				}

				var tmpStr = Preferences.Get( CustomGlossaryFileListLocationsKey, string.Empty );
				if( string.IsNullOrWhiteSpace( tmpStr ) )
					return CustomGlossaryFileListLocationsSave;

				var tmpLists = tmpStr.Split( new[] { StringSeparator1 }, StringSplitOptions.RemoveEmptyEntries );
				if( tmpLists == null || tmpLists.Length <= 0 )
					return CustomGlossaryFileListLocationsSave;

				CustomGlossaryFileListLocationsSave.Clear();
				CustomGlossaryFileListLocationsSave.AddRange( tmpLists );
				return CustomGlossaryFileListLocationsSave;

			}
			set {
				if( value == null || value.Count <= 0 ) {
					Preferences.Set( CustomGlossaryFileListLocationsKey, string.Empty );
					if( value != null )
						CustomGlossaryFileListLocationsSave = value;
					CustomGlossaryFileListLocationsSave.Clear();
					return;
				}

				Preferences.Set( CustomGlossaryFileListLocationsKey, ConcatenateStringList2SingleString1( value ) );
				CustomGlossaryFileListLocationsSave = value;
			}
		}

		private const string CustomGlossaryFileListLocationsKey = "CustomGlossaryFileListLocations";
		private static List<string> CustomGlossaryFileListLocationsSave = null;

		public string DefaultGlossaryFileListLocation => "https://raw.githubusercontent.com/nuthrash/Minax/master/MinaxWebTranslator/TranslationProjects/GlossaryFileList.txt";

		public int QuickTranslationWordMax => 500;

		public RemoteType RemoteXlatorType {
			get {
				if( RemoteXlatorTypeSave != null ) {
					return RemoteXlatorTypeSave.GetValueOrDefault();
				}

				if( Preferences.ContainsKey( RemoteXlatorTypeKey ) == false ) {
					RemoteXlatorTypeSave = RemoteType.Excite;
					Preferences.Set( RemoteXlatorTypeKey, RemoteXlatorTypeSave.ToString() );
					return RemoteXlatorTypeSave.GetValueOrDefault();
				}

				RemoteType type;
				Enum.TryParse( Preferences.Get( RemoteXlatorTypeKey, RemoteXlatorTypeSave.ToString() ), out type );
				RemoteXlatorTypeSave = type;
				return RemoteXlatorTypeSave.GetValueOrDefault();
			}
			set {
				Preferences.Set( RemoteXlatorTypeKey, value.ToString() );
				//Preferences.Set( RemoteXlatorTypeKey, RemoteXlatorTypeSave.ToString() );
				RemoteXlatorTypeSave = value;
			}
		}
		private const string RemoteXlatorTypeKey = "RemoteXlatorType";
		private static RemoteType? RemoteXlatorTypeSave = null;


		public bool RemeberRecentProjects {
			get => Preferences.Get( RemeberRecentProjectsKey, RemeberRecentProjectsDef );
			set => Preferences.Set( RemeberRecentProjectsKey, value );
		}
		private const string RemeberRecentProjectsKey = "RemeberRecentProjects";
		private static readonly bool RemeberRecentProjectsDef = true;

		public int RemeberRecentProjectMax {
			get => Preferences.Get( RemeberRecentProjectMaxKey, RemeberRecentProjectMaxDef );
			set => Preferences.Set( RemeberRecentProjectMaxKey, value );
		}
		private const string RemeberRecentProjectMaxKey = "RemeberRecentProjectMax";
		private static readonly int RemeberRecentProjectMaxDef = 20;

		//[global::System.Configuration.DefaultSettingValueAttribute( "" )]
		public string XlatorBaiduAppId {
			get => Preferences.Get( XlatorBaiduAppIdKey, XlatorBaiduAppIdDef );
			set => Preferences.Set( XlatorBaiduAppIdKey, value );
		}
		private const string XlatorBaiduAppIdKey = "XlatorBaiduAppId";
		private static readonly string XlatorBaiduAppIdDef = string.Empty;

		public string XlatorBaiduSecretKey {
			get => Preferences.Get( XlatorBaiduSecretKeyKey, XlatorBaiduSecretKeyDef );
			set => Preferences.Set( XlatorBaiduSecretKeyKey, value );
		}
		private const string XlatorBaiduSecretKeyKey = "XlatorBaiduSecretKey";
		private static readonly string XlatorBaiduSecretKeyDef = string.Empty;

		public string XlatorYoudaoAppKey {
			get => Preferences.Get( XlatorYoudaoAppKeyKey, XlatorYoudaoAppKeyDef );
			set => Preferences.Set( XlatorYoudaoAppKeyKey, value );
		}
		private const string XlatorYoudaoAppKeyKey = "XlatorYoudaoAppKey";
		private static readonly string XlatorYoudaoAppKeyDef = string.Empty;


		public string XlatorYoudaoAppSecret {
			get => Preferences.Get( XlatorYoudaoAppSecretKey, XlatorYoudaoAppSecretDef );
			set => Preferences.Set( XlatorYoudaoAppSecretKey, value );
		}
		private const string XlatorYoudaoAppSecretKey = "XlatorYoudaoAppSecret";
		private static readonly string XlatorYoudaoAppSecretDef = string.Empty;


		public string XlatorGoogleApiKey {
			get => Preferences.Get( XlatorGoogleApiKeyKey, XlatorGoogleApiKeyDef );
			set => Preferences.Set( XlatorGoogleApiKeyKey, value );
		}
		private const string XlatorGoogleApiKeyKey = "XlatorGoogleApiKey";
		private static readonly string XlatorGoogleApiKeyDef = string.Empty;

		public string XlatorMicrosoftServer {
			get => Preferences.Get( XlatorMicrosoftServerKey, XlatorMicrosoftServerDef );
			set => Preferences.Set( XlatorMicrosoftServerKey, value );
		}
		private const string XlatorMicrosoftServerKey = "XlatorMicrosoftServer";
		private static readonly string XlatorMicrosoftServerDef = "api.cognitive.microsofttranslator.com";

		public string XlatorMicrosoftSubKey {
			get => Preferences.Get( XlatorMicrosoftSubKeyKey, XlatorMicrosoftSubKeyDef );
			set => Preferences.Set( XlatorMicrosoftSubKeyKey, value );
		}
		private const string XlatorMicrosoftSubKeyKey = "XlatorMicrosoftSubKey";
		private static readonly string XlatorMicrosoftSubKeyDef = string.Empty;


		public string XlatorMicrosoftSubRegion {
			get => Preferences.Get( XlatorMicrosoftSubRegionKey, XlatorMicrosoftSubRegionDef );
			set => Preferences.Set( XlatorMicrosoftSubRegionKey, value );
		}
		private const string XlatorMicrosoftSubRegionKey = "XlatorMicrosoftSubRegion";
		private static readonly string XlatorMicrosoftSubRegionDef = string.Empty;

		public string XlatorCrypto {
			get => Preferences.Get( XlatorCryptoKey, XlatorCryptoDef );
			set => Preferences.Set( XlatorCryptoKey, value );
		}
		private const string XlatorCryptoKey = "XlatorCrypto";
		private static readonly string XlatorCryptoDef = string.Empty;


		private static string ConcatenateStringList2SingleString1( IList<string> list )
		{
			StringBuilder sb = new StringBuilder();
			foreach( var str in list ) {
				sb.Append( $"{str}{StringSeparator1}" );
			}
			return sb.ToString();
		}

		public void Save()
		{
			Preferences.Set( RecentProjectsKey, ConcatenateStringList2SingleString1( RecentProjectsSave ) );
			Preferences.Set( CustomGlossaryFileListLocationsKey, ConcatenateStringList2SingleString1( CustomGlossaryFileListLocationsSave ) );
			Preferences.Set( RemoteXlatorTypeKey, RemoteXlatorTypeSave.ToString() );
		}

	}
}
