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

	public class YoudaoTranslatorFormat2
	{
		// 0 for success
		[JsonProperty( PropertyName = "code" )]
		public int Code {
			get; set;
		}

		[JsonProperty( PropertyName = "translateResult" )]
		public List<List<Result>> TranslateResult {
			get; set;
		}

		// like "ja2zh-CHS"
		[JsonProperty( PropertyName = "type" )]
		public string Type {
			get; set;
		}

		public class Result
		{
			[JsonProperty( PropertyName = "tgt" )]
			public string Target {
				get; set;
			}

			[JsonProperty( PropertyName = "src" )]
			public string Source {
				get; set;
			}

			[JsonProperty( PropertyName = "tgtPronounce" )]
			public string TargetPronounce {
				get; set;
			}

			[JsonProperty( PropertyName = "srcPronounce" )]
			public string SourcePronounce {
				get; set;
			}

			[JsonProperty( PropertyName = "jaTransPjm" )]
			public List<TransPjm> JaTransPjm {
				get; set;
			}
		}

		public class TransPjm
		{
			[JsonProperty( PropertyName = "pjm" )]
			public string Pjm {
				get; set;
			}

			[JsonProperty( PropertyName = "word" )]
			public string Word {
				get; set;
			}
		}
	}

	public class YoudaoTranslatorFormat3
	{
		// 錯誤傳回碼	。一定存在
		[JsonProperty( PropertyName = "errorCode" )]
		public string ErrorCode {
			get; set;
		}

		// 翻譯結果。查詢正確時，一定存在
		[JsonProperty( PropertyName = "translation" )]
		public List<string> Translation {
			get; set;
		}

		// 源語言。查詢正確時，一定存在
		[JsonProperty( PropertyName = "query" )]
		public string Query {
			get; set;
		}

		[JsonProperty( PropertyName = "requestId" )]
		public string RequestId {
			get; set;
		}

		// 詞典deeplink。查詢語種為支援語言時，存在
		[JsonProperty( PropertyName = "dict" )]
		public DictData Dict {
			get; set;
		}

		// webdeeplink。查詢語種為支援語言時，存在
		[JsonProperty( PropertyName = "webdict" )]
		public WebdictData Webdict {
			get; set;
		}

		// 源語言和目標語言。一定存在
		[JsonProperty( PropertyName = "l" )]
		public string L {
			get; set;
		}

		[JsonProperty( PropertyName = "isWord" )]
		public bool IsWord {
			get; set;
		}

		// 翻譯結果發音地址。翻譯成功一定存在，需要應用繫結語音合成服務（？）才能正常播放，否則返回110錯誤碼
		[JsonProperty( PropertyName = "tSpeakUrl" )]
		public string TSpeakUrl {
			get; set;
		}

		// 源語言發音地址。翻譯成功一定存在，需要應用繫結語音合成服務（？）才能正常播放，否則返回110錯誤碼
		[JsonProperty( PropertyName = "speakUrl" )]
		public string SpeakUrl {
			get; set;
		}


		public class DictData
		{
			[JsonProperty( PropertyName = "url" )]
			public string Url { get; set; }
		}

		public class WebdictData
		{
			[JsonProperty( PropertyName = "url" )]
			public string Url { get; set; }
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
		/// <remarks>
		/// a. 中文查詞的basic欄位只包含explains欄位。
		/// b. 英文查詞的basic欄位中又包含以下欄位。
		///   us-phonetic	美式音標，英文查詞成功，一定存在
		///   phonetic		默認音標，默認是英式音標，英文查詞成功，一定存在
		///   uk-phonetic	英式音標，英文查詞成功，一定存在
		///   uk-speech		英式發音，英文查詞成功，一定存在
		///   us-speech		美式發音，英文查詞成功，一定存在
		///   explains		基本釋義
		/// </remarks>
		[JsonProperty( PropertyName = "basic" )]
		public string Basic { get; set; }

		/// <summary>
		/// 詞義。網絡釋義，該結果不一定存在
		/// </summary>
		/// <remarks>有道詞典</remarks>
		[JsonProperty( PropertyName = "web" )]
		public List<string> Web { get; set; }

		/// <summary>
		/// 源語言和目標語言。一定存在
		/// </summary>
		/// <example>"EN2zh-CHS"</example>
		[JsonProperty( PropertyName = "l" )]
		public string L { get; set; }

		/// <summary>
		/// 詞典deeplink。查詢語種為支援語言時，存在
		/// </summary>
		[JsonProperty( PropertyName = "dict" )]
		public DictData Dict { get; set; }

		/// <summary>
		/// webdeeplink。查詢語種為支援語言時，存在
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

	// https://ai.youdao.com/DOCSIRMA/html/%E8%87%AA%E7%84%B6%E8%AF%AD%E8%A8%80%E7%BF%BB%E8%AF%91/API%E6%96%87%E6%A1%A3/%E6%89%B9%E9%87%8F%E7%BF%BB%E8%AF%91%E6%9C%8D%E5%8A%A1/%E6%89%B9%E9%87%8F%E7%BF%BB%E8%AF%91%E6%9C%8D%E5%8A%A1-API%E6%96%87%E6%A1%A3.html
	// for https://openapi.youdao.com/v2/api
	public class YoudaoTranslationResponseV2
	{

		/// <summary>
		/// 錯誤返回碼。一定存在
		/// </summary>
		[JsonProperty( PropertyName = "errorCode" )]
		public string ErrorCode {
			get; set;
		}

		/// <summary>
		/// 錯誤結果的序號。結果部分出錯時存在。序號與輸入的i欄位順序一一對應，序號從0開始。JSONArray中元素為int類型
		/// </summary>
		[JsonProperty( "errorIndex" )]
		public List<int> ErrorIndex {
			get; set;
		}

		/// <summary>
		/// 翻譯結果。批次請求中存在正確結果時，一定存在。
		/// JSONArray中元素為JSONObject類型，JSONObject中一定包含query、translation和type欄位（均為String類型），分別表示翻譯原句、翻譯結果和翻譯所用的語言方向。
		/// </summary>
		[JsonProperty( "translateResults" )]
		public List<TranslateResult> TranslateResults {
			get; set;
		}

		public class TranslateResult
		{
			// 翻譯原句
			[JsonProperty( "query" )]
			public string Query {
				get; set;
			}

			// 翻譯結果
			[JsonProperty( "translation" )]
			public string Translation {
				get; set;
			}

			// 翻譯結果
			[JsonProperty( "type" )]
			public string Type {
				get; set;
			}

			// 第一個q欄位語言方向核實結果
			[JsonProperty( "verifyResult" )]
			public string VerifyResult {
				get; set;
			}
		}
	}
}
