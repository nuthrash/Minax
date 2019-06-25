using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Minax.Web.Translation
{
	public static class Profiles
	{
		public static readonly ReadOnlyDictionary<RemoteType, string> DefaultEngineFolders = 
			new ReadOnlyDictionary<RemoteType, string>( new Dictionary<RemoteType, string> {
			[RemoteType.Excite] = "Excite",
			[RemoteType.Weblio] = "Weblio",
			[RemoteType.CrossLanguage] = "CrossLanguage",
			[RemoteType.Baidu] = "Baidu",
			[RemoteType.Youdao] = "Youdao",
			[RemoteType.Google] = "Google",
			[RemoteType.Microsoft] = "Bing",

			[RemoteType.KodenshaFree] = "Excite",
			[RemoteType.CrossLanguageFree] = "CrossLanguage",
			[RemoteType.BaiduFree] = "Baidu",
			[RemoteType.YoudaoFree] = "Youdao",
			[RemoteType.GoogleFree] = "Google",
			[RemoteType.MicrosoftFree] = "Bing",

			[RemoteType.KodenshaCharged] = "Excite",
			[RemoteType.CrossLanguageCharged] = "CrossLanguage",
			[RemoteType.BaiduCharged] = "Baidu",
			[RemoteType.YoudaoCharged] = "Youdao",
			[RemoteType.GoogleCharged] = "Google",
			[RemoteType.MicrosoftCharged] = "Bing",

		} );

		public static readonly ReadOnlyDictionary<RemoteType, string> DefaultHosts =
			new ReadOnlyDictionary<RemoteType, string> ( new Dictionary<RemoteType, string> {
			[RemoteType.Excite] = "www.excite.co.jp",
			[RemoteType.Weblio] = "translate.weblio.jp",
			[RemoteType.CrossLanguage] = "cross.transer.com",
			[RemoteType.Baidu] = "fanyi.baidu.com",
			[RemoteType.Youdao] = "fanyi.youdao.com",
			[RemoteType.Google] = "translate.google.com", // "https://translate.google.com/?op=translate&sl=en&tl=zh-TW&text=Translated"
			[RemoteType.Microsoft] = "www.bing.com", // "https://www.bing.com/translator"

			[RemoteType.KodenshaFree] = "about:blank", // unknown
			[RemoteType.CrossLanguageFree] = "cross.transer.com", // http://cross.transer.com/text/exec_tran
			[RemoteType.BaiduFree] = "fanyi.baidu.com", // https://fanyi.baidu.com/transapi
			[RemoteType.YoudaoFree] = "fanyi.youdao.com", // http://fanyi.youdao.com/translate
			[RemoteType.GoogleFree] = "translate.google.com", // https://translate.google.com/translate_a/single
			[RemoteType.MicrosoftFree] = "about:blank", // unknown

			[RemoteType.KodenshaCharged] = "about:blank", // unknown
			[RemoteType.BaiduCharged] = "fanyi-api.baidu.com", // https://fanyi-api.baidu.com/api/trans/vip/translate
			[RemoteType.YoudaoCharged] = "fanyi.youdao.com", // http://fanyi.youdao.com/translate
			[RemoteType.GoogleCharged] = "translation.googleapis.com", // https://translation.googleapis.com/language/translate/v2/
			[RemoteType.MicrosoftCharged] = "api.cognitive.microsofttranslator.com", // https://api.cognitive.microsofttranslator.com/languages?api-version=3.0

		} );

		public static class MaxWords
		{
			public const int Excite = 1500; // 2000 words in Excite web page...
			public const int Weblio = 1500; // 4000 words in Weblio web page...
			public const int CrossLanguage = 1500; // 2000 words in CROSS-Transer web page...

			/// <summary>
			/// Google Translator
			/// </summary>
			/// <remarks>Its translate_m.js seems occupy the URL field to place encoded original text,
			/// therefore some WebView Control(like WebBrowser of netfx) would sometimes encounter JavaScript
			/// invalid encoding error messages result by exceed the max. length of URL field. 
			/// In fact, each WebView Control has its own upper bound of URL field, and some modern browsers
			/// can see https://stackoverflow.com/a/417184.
			/// </remarks>
			public const int Google = 200; // 5000
			public const int Youdao = 2000; // 5000;
			public const int Baidu = 1600; //5000;
			public const int Bing = 1700; //5000;
		}


		// Japanese HTML replace characters
		public static readonly IReadOnlyList<(string From, string To)> JapaneseEscapeHtmlText = new List<(string From, string To)> {
			( "＜", "「" ), ( "＞", "」" ), ("〝", "「"), ("〟", "」"),
		};

		// convert HTML code to text
		public static readonly IReadOnlyList<(string From, string To)> HtmlCodeRecoveryText = new List<(string From, string To)> {
			( "&lt;", "<" ), ( "&gt;", ">" ), ( "&amp;", "&" ), ( "&quot;", "\"" ),
		};


		#region "After Translated, then replace some strings"

		// the sequence of following tuple (,) pairs in List are very important,
		//   text where was replaced by previous From string would be searched and replaced by next From string!! 

		public static readonly IReadOnlyList<(string From, string To)> WeblioXlationAfter2Cht = new List<(string From, string To)> {
			( "“", "「"), ( "”", "」"), ( "‘", "『"), ( "’", "』"),
			( ",", "，"), ( "!", "！"), ( "?", "？"),
			( "干脆", "乾脆"), ( "制造", "製造"),
		};

		public static readonly IReadOnlyList<(string From, string To)> CrossLanguageXlationAfter2Cht = new List<(string From, string To)> {
			//( "“", "「"), ( "”", "」"), ( "‘", "《"), ( "’", "》"),
			//( "异", "異"), ( "潜", "潛"), ( "欧", "歐"), ( "畧", "略"),
			//( "猪", "豬"), ( "弃", "棄"), ( "囙", "因"), ( "猫", "貓"),
		};

		public static readonly IReadOnlyList<(string From, string To)> BaiduXlationAfter2Cht = new List<(string From, string To)> {
			( "“", "「"), ( "”", "」"), ( "‘", "《"), ( "’", "》"),
			( "异", "異"), ( "潜", "潛"), ( "欧", "歐"), ( "畧", "略"),
			( "猪", "豬"), ( "弃", "棄"), ( "囙", "因"), ( "猫", "貓"),
			( "荧", "螢"),
		};

		public static readonly IReadOnlyList<(string From, string To)> YoudaoXlationAfter2Cht = new List<(string From, string To)> {
			( "“", "「"), ( "”", "」"), ( "‘", "『"), ( "’", "』"),
			( ",", "，"), ( "!", "！"), ( "?", "？"), ( "干脆", "乾脆"), ( "制造", "製造"),
		};

		public static readonly IReadOnlyList<(string From, string To)> GoogleXlationAfter2Cht = new List<(string From, string To)> {
			( "“", "「"), ( "”", "」"), ( "‘", "『"), ( "’", "』"),
			//( "\"", "「"),
		};

		public static readonly IReadOnlyList<(string From, string To)> MicrosoftXlationAfter2Cht = new List<(string From, string To)> {
			( ",", "，"), ( "!", "！"), ( "?", "？"), ( "。。。", "…"),
			//( "\"", "「"),
		};

		public static readonly IReadOnlyList<(string From, string To)> XlationAfterMsConvert2Cht = new List<(string, string)> {
			( "体", "體"), ( "声", "聲"), ( "么", "麼"), ( "尸", "屍"), ( "并", "並"),
			( "涌", "湧"), ( "后", "後"), ( "里", "裡"), ( "于", "於"), ( "采", "採"),
			( "贊", "讚"), ( "愿", "願"), ( "馀", "餘"), ( "圣", "聖"), ( "閑", "閒"),
			( "公裡", "公里"), ( "制作", "製作"), ( "王後", "王后"), ( "皇後", "皇后"), ( "關系", "關係"),
			( "范圍", "範圍"), ( "戰斗", "戰鬥"), ( "游戲", "遊戲"), ( "仿佛", "彷彿"), ( "家伙", "傢伙"),
		};

		#endregion
	}
}
