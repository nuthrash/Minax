using System;
using System.Collections.Generic;
using System.Text;

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
		/// Excite Translation (エキサイト翻訳). Powered by KODENSHA
		/// </summary>
		/// <remarks>https://www.excite.co.jp/world/</remarks>
		Excite = 1,
		/// <summary>
		/// Weblio Translation (Weblio 翻訳)
		/// </summary>
		/// <remarks>https://translate.weblio.jp</remarks>
		Weblio = 10,
		/// <summary>
		/// CrossLanguage Translation (CROSS-Transer)
		/// </summary>
		/// <remarks>https://www.crosslanguage.co.jp/ http://cross.transer.com/ </remarks>
		CrossLanguage = 20,
		/// <summary>
		/// Baidu Translation (百度翻译)
		/// </summary>
		/// <remarks>https://fanyi.baidu.com/</remarks>
		Baidu = 30,
		/// <summary>
		/// Youdao Translation (有道翻译)
		/// </summary>
		/// <remarks>http://fanyi.youdao.com</remarks>
		Youdao = 40,
		/// <summary>
		/// Google Translation
		/// </summary>
		Google = 50,
		/// <summary>
		/// Bing Translation
		/// </summary>
		Bing = 60,
		#endregion

		#region "Web API types"

		/// <summary>
		/// Excite translation solution source. Kodensha(高電社) Co. Ltd.
		/// </summary>
		/// <remarks>https://www.kodensha.jp https://ja.wikipedia.org/wiki/%E9%AB%98%E9%9B%BB%E7%A4%BE ??</remarks>
		KodenshaFree = 100,
		CrossLanguageFree = 120,
		BaiduFree = 130,
		YoudaoFree = 140,
		GoogleFree = 150,
		BingFree = 160, // ???

		/// <summary>
		/// Excite translation solution source. Kodensha(高電社) Co. Ltd.
		/// </summary>
		/// <remarks>https://www.kodensha.jp https://ja.wikipedia.org/wiki/%E9%AB%98%E9%9B%BB%E7%A4%BE </remarks>
		KodenshaCharged = 200,
		/// <summary>
		/// CrossLanguage Corp.(株式會社クロスランゲージ)
		/// </summary>
		/// <remarks>https://www.crosslanguage.co.jp/auto-translation/sdk/api/ ??</remarks>
		CrossLanguageCharged = 220,
		/// <summary>
		/// Baidu translation
		/// </summary>
		/// <remarks>http://api.fanyi.baidu.com/api/trans/product/apidoc </remarks>
		BaiduCharged = 230,
		/// <summary>
		/// Youdao translation
		/// </summary>
		/// <remarks>https://ai.youdao.com/docs/doc-trans-api.s https://openapi.youdao.com/api </remarks>
		YoudaoCharged = 240,
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>https://cloud.google.com/translate/docs/ https://translation.googleapis.com/language/translate/v2</remarks>
		GoogleCharged = 250,
		BingCharged = 260,

		#endregion
	}
}
