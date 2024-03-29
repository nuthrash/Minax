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
		/// Excite Translator (エキサイト翻訳). Powered by KODENSHA
		/// </summary>
		/// <remarks>https://www.excite.co.jp/world/</remarks>
		Excite = 1,
		/// <summary>
		/// Weblio Translator (Weblio 翻訳)
		/// </summary>
		/// <remarks>https://translate.weblio.jp</remarks>
		Weblio = 10,
		/// <summary>
		/// CrossLanguage Translation (CROSS-Transer)
		/// </summary>
		/// <remarks>https://www.crosslanguage.co.jp/ http://cross.transer.com/ </remarks>
		CrossLanguage = 20,
		/// <summary>
		/// Baidu Translator (百度翻译)
		/// </summary>
		/// <remarks>https://fanyi.baidu.com/</remarks>
		Baidu = 30,
		/// <summary>
		/// Youdao Translator (有道翻译)
		/// </summary>
		/// <remarks>https://fanyi.youdao.com</remarks>
		Youdao = 40,
		/// <summary>
		/// Google Translator
		/// </summary>
		/// <remarks>https://translate.google.com/</remarks>
		Google = 50,
		/// <summary>
		/// Microsoft/Bing Translator
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
		/// MiraiTranslate translation solution.
		/// </summary>
		MiraiTranslateFree = 105,
		/// <summary>
		/// CrossLanguage translation solution.
		/// </summary>
		CrossLanguageFree = 110,
		/// <summary>
		/// Baidu translation API (Free)
		/// </summary>
		BaiduFree = 120,
		/// <summary>
		/// iCIBA translation API (Free)
		/// </summary>
		/// <remarks>http://www.iciba.com/fy https://ifanyi.iciba.com/index.php</remarks>
		IcibaFree = 133,
		/// <summary>
		/// LingoCloud/CaiyunXiaoYi translation API (Free)
		/// </summary>
		/// <remarks>https://fanyi.caiyunapp.com/ http://api.interpreter.caiyunai.com/v1/translator</remarks>
		LingoCloudFree = 135,
		/// <summary>
		/// Tencent translation API (Free)
		/// </summary>
		/// <remarks>https://fanyi.qq.com/</remarks>
		TencentFree = 137,
		/// <summary>
		/// Youdao translation API (Free)
		/// </summary>
		/// <remarks>https://ai.youdao.com/product-fanyi-text.s</remarks>
		YoudaoFree = 140,
		/// <summary>
		/// Naver Papago NMT translation solution.
		/// </summary>
		/// <remarks>https://papago.naver.com</remarks>
		PapagoFree = 145,
		/// <summary>
		/// Google translation API (Free)
		/// </summary>
		GoogleFree = 150,
		/// <summary>
		/// Microsoft translation API (Free)
		/// </summary>
		MicrosoftFree = 160, // found some information, but seems not free

		/// <summary>
		/// Excite translation solution upstream source. Kodensha(高電社) Co. Ltd.
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
		/// Naver Papago Translation API (Charged)
		/// </summary>
		/// <remarks>https://www.ncloud.com/product/aiService/papagoTranslation https://guide.ncloud-docs.com/docs/en/papagotranslation-api https://api.ncloud-docs.com/docs/en/ai-naver-papagonmt-translation</remarks>
		PapagoCharged = 245,
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
