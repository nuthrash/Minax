using System;

namespace Minax.Web.Translation
{
	/// <summary>
	/// Remote translator/translation service type
	/// </summary>
	[Serializable]
	public enum RemoteType
	{
		None = 0,

		#region "Web Page types"

		/// <summary>
		/// Excite Transltor (エキサイト翻訳). Powered by KODENSHA
		/// </summary>
		/// <remarks>https://www.excite.co.jp/world/</remarks>
		Excite = 1,
		/// <summary>
		/// Weblio Transltor (Weblio 翻訳)
		/// </summary>
		/// <remarks>https://translate.weblio.jp</remarks>
		Weblio = 10,
		/// <summary>
		/// CrossLanguage Translation (CROSS-Transer)
		/// </summary>
		/// <remarks>https://www.crosslanguage.co.jp/ http://cross.transer.com/ </remarks>
		CrossLanguage = 20,
		/// <summary>
		/// Baidu Transltor (百度翻译)
		/// </summary>
		/// <remarks>https://fanyi.baidu.com/</remarks>
		Baidu = 30,
		/// <summary>
		/// Youdao Transltor (有道翻译)
		/// </summary>
		/// <remarks>http://fanyi.youdao.com</remarks>
		Youdao = 40,
		/// <summary>
		/// Google Transltor
		/// </summary>
		Google = 50,
		/// <summary>
		/// Microsoft/Bing Transltor
		/// </summary>
		Microsoft = 60,

		#endregion

		#region "Web API types"

		/// <summary>
		/// Excite translation solution source. Kodensha(高電社) Co. Ltd.
		/// </summary>
		/// <remarks>https://www.kodensha.jp https://ja.wikipedia.org/wiki/%E9%AB%98%E9%9B%BB%E7%A4%BE ??</remarks>
		KodenshaFree = 100,
		/// <summary>
		/// CrossLanguage translation solution.
		/// </summary>
		CrossLanguageFree = 120,
		/// <summary>
		/// Baidu translation API (Free)
		/// </summary>
		BaiduFree = 130,
		/// <summary>
		/// Youdao translation API (Free)
		/// </summary>
		YoudaoFree = 140,
		/// <summary>
		/// Google translation API (Free)
		/// </summary>
		GoogleFree = 150,
		/// <summary>
		/// Microsoft translation API (Free)
		/// </summary>
		MicrosoftFree = 160, // found some information, but seems not free

		/// <summary>
		/// Excite translation solution source. Kodensha(高電社) Co. Ltd.
		/// </summary>
		/// <remarks>https://www.kodensha.jp https://ja.wikipedia.org/wiki/%E9%AB%98%E9%9B%BB%E7%A4%BE </remarks>
		KodenshaCharged = 200,
		/// <summary>
		/// CrossLanguage Corp.(株式會社クロスランゲージ)
		/// </summary>
		/// <remarks>https://www.crosslanguage.co.jp/auto-translation/sdk/api/  ??</remarks>
		CrossLanguageCharged = 220,
		/// <summary>
		/// Baidu Translation API (Charged)
		/// </summary>
		/// <remarks>http://api.fanyi.baidu.com/api/trans/product/apidoc </remarks>
		BaiduCharged = 230,
		/// <summary>
		/// Youdao Translation API (Charged)
		/// </summary>
		/// <remarks>https://ai.youdao.com/docs/doc-trans-api.s https://openapi.youdao.com/api </remarks>
		YoudaoCharged = 240,
		/// <summary>
		/// Google Translation API (Charged) V2
		/// </summary>
		/// <remarks>https://cloud.google.com/translate/docs/ https://translation.googleapis.com/language/translate/v2</remarks>
		GoogleCharged = 250,
		//GoogleChargedV3 = 251,
		/// <summary>
		/// Microsoft Translation API (Charged) V3
		/// </summary>
		MicrosoftCharged = 260,

		#endregion
	}
}
