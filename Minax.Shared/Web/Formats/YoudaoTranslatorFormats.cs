using Newtonsoft.Json;
using System.Collections.Generic;

namespace Minax.Web.Formats
{
	public class YoudaoTranslatorFormat1
	{
		[JsonProperty( PropertyName = "type" )]
		public string Type { get; set; }

		[JsonProperty( PropertyName = "errorCode" )]
		public int ErrorCode { get; set; }

		[JsonProperty( PropertyName = "elapsedTime" )]
		public double ElapsedTime { get; set; }

		[JsonProperty( PropertyName = "translateResult" )]
		public List<List<SrcTargetPair>> TranslateResult { get; set; } = new List<List<SrcTargetPair>>();

		public class SrcTargetPair
		{
			[JsonProperty( PropertyName = "src" )]
			public string Source { get; set; }
			[JsonProperty( PropertyName = "tgt" )]
			public string Target { get; set; }
		}

	}

	// http://ai.youdao.com/docs/doc-trans-api.s#p05
	public class YoudaoTranslationResponseV1
	{

		/// <summary>
		/// 錯誤返回碼。一定存在
		/// </summary>
		[JsonProperty( PropertyName = "errorCode" )]
		public string ErrorCode { get; set; }

		/// <summary>
		/// 源語言。查詢正確時，一定存在
		/// </summary>
		[JsonProperty( PropertyName = "query" )]
		public string Query { get; set; }

		/// <summary>
		/// 翻譯結果。查詢正確時，一定存在
		/// </summary>
		[JsonProperty( PropertyName = "translation" )]
		public List<string> Translation { get; set; }

		/// <summary>
		/// 詞義。基本詞典，查詞時才有
		/// </summary>
		[JsonProperty( PropertyName = "basic" )]
		public string Basic { get; set; }

		/// <summary>
		/// 詞義。網絡釋義，該結果不一定存在
		/// </summary>
		[JsonProperty( PropertyName = "web" )]
		public string Web { get; set; }

		/// <summary>
		/// 源語言和目標語言。一定存在
		/// </summary>
		[JsonProperty( PropertyName = "l" )]
		public string L { get; set; }

		/// <summary>
		/// 詞典deeplink。查詢語種為支持語言時，存在
		/// </summary>
		[JsonProperty( PropertyName = "dict" )]
		public DictData Dict { get; set; }

		/// <summary>
		/// webdeeplink。查詢語種為支持語言時，存在
		/// </summary>
		[JsonProperty( PropertyName = "webdict" )]
		public WebDictData WebDict { get; set; }

		/// <summary>
		/// 翻譯結果發音地址。翻譯成功一定存在，需要應用綁定語音合成實例才能正常播放，否則返回110錯誤碼
		/// </summary>
		[JsonProperty( PropertyName = "tSpeakUrl" )]
		public string TSpeakUrl { get; set; }


		/// <summary>
		/// 源語言發音地址。翻譯成功一定存在，需要應用綁定語音合成實例才能正常播放，否則返回110錯誤碼
		/// </summary>
		[JsonProperty( PropertyName = "speakUrl" )]
		public string SpeakUrl { get; set; }

		/// <summary>
		/// 單詞校驗後的結果。	主要校驗字母大小寫、單詞前含符號、中文簡繁體
		/// </summary>
		[JsonProperty( PropertyName = "returnPhrase" )]
		public string ReturnPhrase { get; set; }


		public class DictData
		{
			[JsonProperty( PropertyName = "url" )]
			public string Url { get; set; }
		}

		public class WebDictData
		{
			[JsonProperty( PropertyName = "url" )]
			public string Url { get; set; }
		}
	}
}
