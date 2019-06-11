using System;
using System.Collections.Generic;
using System.Text;

namespace Minax.Domain.Translation
{
	/// <summary>
	/// Current supported languages
	/// </summary>
	public static class SupportedLanguages
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

		public static string ToIso639( this SupportedSourceLanguage srcLang )
		{
			switch( srcLang ) {
				case SupportedSourceLanguage.English:
					return "en";
				case SupportedSourceLanguage.Japanese:
					return "ja";

				case SupportedSourceLanguage.ChineseSimplified:
				case SupportedSourceLanguage.ChineseTraditional:
					return "zh";

				default:
					return "";
			}
		}

		public static string ToIso639( this SupportedTargetLanguage tgtLang )
		{
			switch( tgtLang ) {
				case SupportedTargetLanguage.ChineseTraditional:
					return "zh";

				case SupportedTargetLanguage.English:
					return "en";

				default:
					return "";
			}
		}
	}
}
