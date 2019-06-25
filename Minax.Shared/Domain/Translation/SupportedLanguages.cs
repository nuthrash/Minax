using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Domain.Translation
{

	public enum SupportedSourceLanguage
	{
		AutoDetect = 0,

		English,
		Japanese,
		ChineseSimplified,
		ChineseTraditional,
	}

	public enum SupportedTargetLanguage
	{
		ChineseTraditional,
		English,
	}

	public static class SupportedLanguagesExtensions
	{
		// ISO 639 (primary language code, two-letter) + ISO 3166-1 (sub language code, region) <= old
		// RFC 4646 http://www.ietf.org/rfc/rfc4646.txt <= old
		// ISO 639 (primary language code, two-letter) + ISO 15924 (sub language code, region) http://unicode.org/iso15924/iso15924-codes.html

		/// <summary>
		/// Convert Minax language enum to ISO code
		/// </summary>
		/// <param name="srcLang">Supported source language</param>
		/// <returns>ISO code string</returns>
		public static string ToIsoCode( this SupportedSourceLanguage srcLang )
		{
			switch( srcLang ) {
				case SupportedSourceLanguage.English:
					return "en";
				case SupportedSourceLanguage.Japanese:
					return "ja";

				case SupportedSourceLanguage.ChineseSimplified:
					return "zh-Hans";
				case SupportedSourceLanguage.ChineseTraditional:
					return "zh-Hant";

				default:
					return "";
			}
		}

		/// <summary>
		/// Convert Minax language enum to ISO code
		/// </summary>
		/// <param name="tgtLang">Supported target language</param>
		/// <returns>ISO code string</returns>
		public static string ToIsoCode( this SupportedTargetLanguage tgtLang )
		{
			switch( tgtLang ) {
				case SupportedTargetLanguage.ChineseTraditional:
					return "zh-Hant";

				case SupportedTargetLanguage.English:
					return "en";

				default:
					return "";
			}
		}

		public static string ToL10nString( this SupportedSourceLanguage srcLang )
		{
			switch( srcLang ) {
				case SupportedSourceLanguage.ChineseSimplified:
					return Languages.SupportedLanguages.Str0ChineseSimplified;
				case SupportedSourceLanguage.ChineseTraditional:
					return Languages.SupportedLanguages.Str0ChineseTraditional;

				case SupportedSourceLanguage.English:
					return Languages.SupportedLanguages.Str0English;

				case SupportedSourceLanguage.Japanese:
					return Languages.SupportedLanguages.Str0Japanese;
			}
			return "";
		}

		public static string ToL10nString( this SupportedTargetLanguage tgtLang )
		{
			switch( tgtLang ) {
				//case SupportedTargetLanguage.ChineseSimplified:
				//	return Languages.SupportedLanguages.Str0ChineseSimplified;
				case SupportedTargetLanguage.ChineseTraditional:
					return Languages.SupportedLanguages.Str0ChineseTraditional;

				case SupportedTargetLanguage.English:
					return Languages.SupportedLanguages.Str0English;

				//case SupportedTargetLanguage.Japanese:
				//	return Languages.SupportedLanguages.Str0Japanese;
			}
			return "";
		}
	}
}
