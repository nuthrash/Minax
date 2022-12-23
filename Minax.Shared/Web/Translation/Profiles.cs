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
			[RemoteType.Microsoft] = "Microsoft",

			[RemoteType.KodenshaFree] = "Excite",
			[RemoteType.MiraiTranslateFree] = "MiraiTranslate",
			[RemoteType.CrossLanguageFree] = "CrossLanguage",

			[RemoteType.BaiduFree] = "Baidu",
			[RemoteType.IcibaFree] = "Iciba",
			[RemoteType.LingoCloudFree] = "LingoCloud",
			[RemoteType.TencentFree] = "Tencent",
			[RemoteType.YoudaoFree] = "Youdao",
			[RemoteType.PapagoFree] = "Papago",
			[RemoteType.GoogleFree] = "Google",
			[RemoteType.MicrosoftFree] = "Microsoft",

			[RemoteType.KodenshaCharged] = "Excite",
			[RemoteType.CrossLanguageCharged] = "CrossLanguage",
			[RemoteType.BaiduCharged] = "Baidu",
			[RemoteType.YoudaoCharged] = "Youdao",
			[RemoteType.GoogleCharged] = "Google",
			[RemoteType.MicrosoftCharged] = "Microsoft",

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
			[RemoteType.MiraiTranslateFree] = "miraitranslate.com", // https://miraitranslate.com/trial/
			[RemoteType.CrossLanguageFree] = "cross.transer.com", // http://cross.transer.com/text/exec_tran
			[RemoteType.BaiduFree] = "fanyi.baidu.com", // https://fanyi.baidu.com/transapi
			[RemoteType.IcibaFree] = "ifanyi.iciba.com", // https://www.iciba.com/fy
			[RemoteType.LingoCloudFree] = "fanyi.caiyunapp.com", // https://fanyi.caiyunapp.com
			[RemoteType.TencentFree] = "fanyi.qq.com", // https://fanyi.qq.com/api/translate
			[RemoteType.YoudaoFree] = "aidemo.youdao.com", // https://aidemo.youdao.com/trans http://fanyi.youdao.com/translate
			[RemoteType.PapagoFree] = "papago.naver.com", // https://papago.naver.com/apis/n2mt/translate
			[RemoteType.GoogleFree] = "translate.google.com", // https://translate.google.com/translate_a/single
			[RemoteType.MicrosoftFree] = "www.bing.com", // "https://www.bing.com/translator"

			[RemoteType.KodenshaCharged] = "about:blank", // unknown
			[RemoteType.BaiduCharged] = "fanyi-api.baidu.com", // https://fanyi-api.baidu.com/api/trans/vip/translate
			[RemoteType.YoudaoCharged] = "fanyi.youdao.com", // http://fanyi.youdao.com/translate
			[RemoteType.GoogleCharged] = "translation.googleapis.com", // https://translation.googleapis.com/language/translate/v2/
			[RemoteType.MicrosoftCharged] = "api.cognitive.microsofttranslator.com", // https://api.cognitive.microsofttranslator.com/languages?api-version=3.0

		} );

		public static class MaxWords
		{
			public const int Excite = 1000; // 2000 words in Excite web page...
			public const int Weblio = 1500; // 4000 words in Weblio web page...
			public const int CrossLanguage = 1500; // 2000 words in CROSS-Transer web page...
			public const int MiraiTranslate = 1500; // 2000 words in MiraiTranslate web page...

			public const int Youdao = 800; // 1000
			public const int YoudaoMobile = 200; // ??
			public const int Baidu = 1600; //5000
			public const int Iciba = 1000; //3000
			public const int LingoCloud = 2000; //?? 
			public const int Tencent = 800; // 翻译上限5000字符，请将原文分开翻译  翻译上限1000字符,请将原文分开翻译

			public const int Papago = 2500; // 5000 words in MiraiTranslate web page...

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
			public const int Bing = 1700; //5000;
		}


		// Japanese HTML replace characters
		public static readonly IReadOnlyList<(string From, string To)> JapaneseEscapeHtmlText = new List<(string From, string To)> {
			//( "＜", "「" ), ( "＞", "」" ), ("〝", "「"), ("〟", "」"),
			("〝", "「"), ("〟", "」"),
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

		public static readonly IReadOnlyList<(string From, string To)> MiraiXlationAfter2Cht = new List<(string From, string To)> {
			//( "“", "「"), ( "”", "」"), ( "‘", "『"), ( "’", "』"),
			//( ",", "，"), ( "!", "！"), ( "?", "？"),
			( "尸體", "屍體"), ( "僵尸", "殭屍"), ( "幹擾", "干擾"), ( "納米", "奈米" ), ( "仿佛", "彷彿" ), ( "輕松", "輕鬆" ),
			( "重復", "重複" ), ( "誌願", "志願" ), ( "復蘇", "復甦" ), ( "幹杯", "乾杯"), ( "係統", "系統"),
			( "傳感器", "感測器" ),
		};


		public static readonly IReadOnlyList<(string From, string To)> BaiduXlationAfter2Cht = new List<(string From, string To)> {
			( "“", "「" ), ( "”", "」" ), ( "‘", "《" ), ( "’", "》" ),
			( "异", "異" ), ( "潜", "潛" ), ( "欧", "歐" ), ( "畧", "略" ),
			( "猪", "豬" ), ( "弃", "棄" ), ( "囙", "因" ), ( "猫", "貓" ),
			( "荧", "螢" ), ( "锁", "鎖" ), ( "灾", "災" ), ( "亯", "享" ),
			( "寘", "置" ), ( "攷", "考" ), ( "胷", "胸" ), ( "鹅", "鵝" ),
			( "氷", "冰" ),
			( "好象", "好像" ), ( "溪穀", "溪谷" ),  ( "貭素", "素質" ),  ( "仿佛", "彷彿" ),
			( "晋昇", "晉升" ), ("納米", "奈米"),
		};

		public static readonly IReadOnlyList<(string From, string To)> YoudaoXlationAfter2Cht = new List<(string From, string To)> {
			//( "“", "「" ), ( "”", "」" ), ( "‘", "『" ), ( "’", "』" ),
			//( ",", "，" ), ( "!", "！" ), ( "?", "？" ), 
			( "纔", "才" ), ( "墻", "牆" ), ( "着", "著" ), ( "爲", "為" ), ( "麪", "麵" ),
			( "干脆", "乾脆" ), ( "制造", "製造" ), ( "想象", "想像" ), ( "涌入", "湧入" ),
			( "建筑", "建築" ), ( "干勁", "幹勁" ), ( "輕松", "輕鬆" ), ( "頭發", "頭髮" ), 
			( "柜臺", "櫃台" ), ( "示范", "示範" ), ( "女仆", "女僕" ), ( "仆人", "僕人" ),
			( "納米", "奈米" ), ( "軟件", "軟體" ), ( "芝士", "起司" ), ( "報道", "報導" ),
			( "導火索", "導火線" ),
		};

		public static readonly IReadOnlyList<(string From, string To)> PapagoXlationAfter2Cht = new List<(string From, string To)> {
			( "干脆", "乾脆" ), ( "制造", "製造" ),
			( "建筑", "建築" ), ( "干勁", "幹勁" ), ( "輕松", "輕鬆" ), ( "頭發", "頭髮" ),
			( "柜臺", "櫃台" ), ( "示范", "示範" ), ( "女仆", "女僕" ), ( "仆人", "僕人" ),
		};

		public static readonly IReadOnlyList<(string From, string To)> GoogleXlationAfter2Cht = new List<(string From, string To)> {
			( "“", "「"), ( "”", "」"), ( "‘", "『"), ( "’", "』"),
			//( "\"", "「"),
		};

		public static readonly IReadOnlyList<(string From, string To)> MicrosoftXlationAfter2Cht = new List<(string From, string To)> {
			//( ",", "，"), ( "!", "！"), ( "?", "？"), ( "。。。", "…"),
			( "納米", "奈米" ),
		};

		public static readonly IReadOnlyList<(string From, string To)> XlationAfterMsConvert2Cht = new List<(string, string)> {
			( "体", "體"), ( "声", "聲"), ( "么", "麼"), ( "尸", "屍"), ( "并", "並"),
			( "涌", "湧"), ( "后", "後"), ( "里", "裡"), ( "于", "於"), ( "采", "採"),
			( "贊", "讚"), ( "愿", "願"), ( "馀", "餘"), ( "圣", "聖"), ( "閑", "閒"),
			( "公裡", "公里"), ( "制作", "製作"), ( "王後", "王后"), ( "皇後", "皇后"), ( "關系", "關係"),
			( "范圍", "範圍"), ( "戰斗", "戰鬥"), ( "游戲", "遊戲"), ( "仿佛", "彷彿"), ( "家伙", "傢伙"),
			( "丑陋", "醜陋"), ( "燒堿", "燒鹼"),
		};

		#endregion
	}
}
