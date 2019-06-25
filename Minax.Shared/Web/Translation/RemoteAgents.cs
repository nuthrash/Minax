using Jurassic;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Formats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minax.Web.Translation
{
	public static class RemoteAgents
	{
		public static ReadOnlyObservableList<MappingMonitor.MappingModel> DescendedModels { get; set; } = new ObservableList<MappingMonitor.MappingModel>();

		public static ObservableList<MappingMonitor.MappingModel> CurrentUsedModels { get; } = new ObservableList<MappingMonitor.MappingModel>();

		public static bool MarkMappedTextWithHtmlBoldTag { get; set; } = false;


		#region "Translation API free"

		public static IEnumerable<YieldResult> TranslateByCrossLanguageFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress )
		{
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocCrossLangFree, srcLang2TgtLang = "CR-JCT-N";
			switch( sourceLanguage ) {
				//case SupportedSourceLanguage.ChineseSimplified:
				//switch( targetLanguage ) {
				//	//case SupportedTargetLanguage.Japanese:
				//	//	srcLang2TgtLang = "CR-CJ-N";
				//	case SupportedTargetLanguage.ChineseTraditional:
				//	default:
				//		yield break;
				//}
				//break;
				//case SupportedSourceLanguage.ChineseTraditional:
				//switch( targetLanguage ) {
				//	//case SupportedTargetLanguage.Japanese:
				//	//	srcLang2TgtLang = "CR-CJT-N";
				//	case SupportedTargetLanguage.ChineseTraditional:
				//	default:
				//		yield break;
				//}
				//break;
				//case SupportedSourceLanguage.English:
				//switch( targetLanguage ) {
				//	//case SupportedTargetLanguage.Japanese:
				//	//	srcLang2TgtLang = "CR-EJ";
				//	case SupportedTargetLanguage.ChineseTraditional:
				//	default:
				//		yield break;
				//}
				//break;
				case SupportedSourceLanguage.Japanese:
					switch( targetLanguage ) {
						case SupportedTargetLanguage.English:
							srcLang2TgtLang = "CR-JE";
							break;
						case SupportedTargetLanguage.ChineseTraditional:
							srcLang2TgtLang = "CR-JCT-N"; // CHS is "CR-JC-N"
							break;
						default:
							yield break;
					}
					break;
				default:
					yield break;
			}


			var text = sourceText;
			CurrentUsedModels.Clear();
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", 5.61359, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.CrossLanguage, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientXLang.DefaultRequestHeaders.Accept.Count <= 0 )
				clientXLang.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientXLang.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientXLang.DefaultRequestHeaders.Add( "User-Agent", "curl/7.47.0" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "e", srcLang2TgtLang }, // from Source Language to Target Lanaguage
							{ "r", "0" }, // ??
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["t"] = section; // words to be translated
				//values["t"] = Uri.EscapeDataString(section); // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientXLang.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<CrossLanguageFormat1>( responseString );

				if( translatedData != null && translatedData.Results != null &&
					translatedData.Results.Count > 0 ) {
					sb.Clear();

					foreach( var result in translatedData.Results ) {
						if( string.IsNullOrEmpty( result.OriginalText ) )
							continue;
						sb.Append( result.OriginalText );
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
						afterTgtLangs.Add( Profiles.CrossLanguageXlationAfter2Cht );

					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						PercentOrErrorCode = percent,
						OriginalSection = section,
						TranslatedSection = sb.ToString()
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", response );
					yield break;
				}
			}
		}


		public static IEnumerable<YieldResult> TranslateByBaiduFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress )
		{
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocBaiduFree, srcLang, tgtLang = "cht";
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "zh";
					break;
				case SupportedSourceLanguage.ChineseTraditional:
					srcLang = "cht";
					break;
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "jp";
					break;
				default:
					yield break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					defLoc = sLocExciteEn;
					tgtLang = "en";
					break;
			}

			// same language is non-sense...
			if( srcLang == tgtLang )
				yield break;

			var text = sourceText;
			var sb = new StringBuilder( sourceText );

			// replace some text to avoid collision
			if( sourceLanguage == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					sb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();
			if( DescendedModels == null )
				DescendedModels = new ObservableList<MappingMonitor.MappingModel>();

			// Tulpe3 => <Original text, tmp. text, Translated text>
			var tmpList = new List<(string OrigText, string TmpText, string XlatedText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = string.Format( "θabcdeλ{0}◎", num++ );
				var k2 = $"{{{k1}}}";
				tmpList.Add( (mm.OriginalText, k2, mm.MappingText) );
				sb.Replace( mm.OriginalText, k2 );

				var tmpstr = sb.ToString();
				if( tmpstr.Contains( $"…{k2}" ) || tmpstr.Contains( $"【{k2}】" ) ||
					tmpstr.Contains( $"＋{k2}" ) ) {
					tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				}

				CurrentUsedModels.Add( mm );
			}

			if( cancelToken.IsCancellationRequested )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Baidu, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( baiduToken == null || baiduGtk == null ) {
				RefreshBaiduData( cancelToken );
			}


			Report( progress, 1, "Preparing Translation", null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "from", srcLang }, // from Source Language
							{ "to", tgtLang }, // to Target Language
							{ "transtype", "translang" },
							{ "simple_means_flag", "3" },
							//{ "token", "c22a137642db9f8d1aed41f5b8cb0b64" },
							{ "token", baiduToken },
						};

			int xlatedSectionCnt = 0, retry = 10;
			foreach( var section in sections ) {

			reloaded:
				jsEngine.Evaluate( jsBaiduToken );
				var sign = jsEngine.CallGlobalFunction<string>( "token", section, baiduGtk );

				values["query"] = section; // words to be translated
				//values["sign"] = token( text, "320305.131321201" ); // window.gtk == "320....."
				//values["sign"] = "453684.134917";
				values["sign"] = sign;

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 1500, 2500 ) );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientBaidu.PostAsync( defLoc, content, cancelToken ).Result;

				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<BaiduTranslatorFormat2>( responseString );

				if( translatedData != null && translatedData.TransResult != null &&
					translatedData.TransResult.Status == 0 &&
					translatedData.TransResult.Data != null &&
					translatedData.TransResult.Data.Count > 0 ) {
					sb.Clear();

					// prepare translated strings by mapping to original lines
					var origLines = section.Split( sNewLineSeparator, StringSplitOptions.None );
					int origIndex = 0;
					foreach( var data in translatedData.TransResult.Data ) {
						if( string.IsNullOrWhiteSpace( data.Destination ) )
							continue;

						for( int i = origIndex; i < origLines.Length; ++i ) {
							if( string.IsNullOrEmpty( origLines[i] ) ) {
								sb.Append( Environment.NewLine );
								continue;
							}

							// check original string start with whitespace or Fullwidth form space
							foreach( var ch in origLines[i] ) {
								if( char.IsWhiteSpace( ch ) || '　' == ch )
									sb.Append( ch );
								else
									break;
							}

							sb.Append( data.Destination + Environment.NewLine );

							origIndex = i + 1;
							break;
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
						afterTgtLangs.Add( Profiles.BaiduXlationAfter2Cht );

					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[{]?θabcdeλ(?<SeqNum>[0-9]+)◎[}]?", "{θabcdeλ${SeqNum}◎}" ) )
						continue; // {θabcdeλ<SeqNum>◎}


					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					// the token might expired, so try to reload it
					if( translatedData != null && (translatedData.Error == 997 ||
						translatedData.Error == 998) && --retry >= 0 ) {
						// 998 means need to reload home page to get new token
						RefreshBaiduData( cancelToken );
						goto reloaded;
					}

					if( translatedData != null && string.IsNullOrWhiteSpace( translatedData.Message ) == false ) {
						Report( progress, -1, $"Something wrong when translating, ErrorCode: {translatedData.Error}, Message: {translatedData.Message}",
							translatedData );
					}
					else {
						Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					}
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByYoudaoFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress )
		{
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocYoudaoFree, srcLang = "ja", tgtLang = "zh-CHS"; // from Japanese to ChineseSimplified
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}


			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "(abc0{0})", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Youdao, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientYoudao.DefaultRequestHeaders.Accept.Count <= 0 )
				clientYoudao.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			if( clientYoudao.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientYoudao.DefaultRequestHeaders.Add( "User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare form values, https://ai.youdao.com/docs/doc-trans-api.s#p04
			var values = new Dictionary<string, string> {
							{ "type", "AUTO" },
							{ "doctype", "json" },
							// https://ai.youdao.com/docs/doc-trans-api.s#p07
							{ "from", "AUTO" },
							//{ "from", srcLang}, // from Source Language
							{ "to", tgtLang}, // to Target Language, useless, most are xlate to ChineseSimplified
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["i"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientYoudao.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<YoudaoTranslatorFormat1>( responseString );

				if( translatedData != null && translatedData.ErrorCode == 0 &&
						translatedData.TranslateResult.Count > 0 ) {
					sb.Clear();
					foreach( var paragraphs in translatedData.TranslateResult ) {
						foreach( var dataSection in paragraphs ) {
							var pair = dataSection;
							sb.Append( ChineseConverter.Convert( pair.Target, ChineseConversionDirection.SimplifiedToTraditional ) );
						}
						sb.AppendLine();
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.YoudaoXlationAfter2Cht );
						afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[(]?abc0(?<SeqNum>[0-9]+)[)]?", "(abc0${SeqNum})" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", response );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByGoogleFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress )
		{
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocGoogleFree, srcLang, tgtLang = "zh_Hant";
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "zh_Hans";
					break;
				case SupportedSourceLanguage.ChineseTraditional:
					srcLang = "zh_Hant";
					break;
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "ja";
					break;
				default:
					yield break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}

			// same language is non-sense...
			if( srcLang == tgtLang )
				yield break;

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "[{0}]", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Google + 1000, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientGoogle.DefaultRequestHeaders.Accept.Count <= 0 )
				clientGoogle.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			if( clientGoogle.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientGoogle.DefaultRequestHeaders.Add( "User-Agent",
						 "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "client", "gtx" },
							{ "dt", "t" },
							{ "dj", "1" }, // with JSON name
							{ "ie", "UTF-8" },
							{ "oe", "UTF-8" },
							{ "sl", srcLang}, // from Source Language, or AUTO
							{ "tl", tgtLang}, // to Target Language
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["q"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientGoogle.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<GoogleTranslatorFormat1>( responseString );

				if( translatedData != null && translatedData.Sentences != null &&
					translatedData.Sentences.Count > 0 ) {
					sb.Clear();

					foreach( var data in translatedData.Sentences ) {
						if( string.IsNullOrWhiteSpace( data.Translated ) )
							continue;
						sb.Append( data.Translated );
					}

					// replace mis-translated text
					sb.Replace( " ]", "]" );
					sb.Replace( "[ ", "[" );

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.GoogleXlationAfter2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}


		private static Jurassic.ScriptEngine jsEngine = new ScriptEngine();
		private static HttpClientHandler handlerBaidu = new HttpClientHandler();
		private static string baiduToken = null, baiduGtk = "320305.131321201", baiduCookie;
		private const string baiduCookieName = "BAIDUID";
		private static void RefreshBaiduData( CancellationToken cancelToken )
		{

			if( clientBaidu.DefaultRequestHeaders.Accept.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientBaidu.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Add( "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" );

			try {
				var uri = new Uri( sLocBaidu );

				var rsp = clientBaidu.GetAsync( uri, cancelToken ).Result;
				Task.Delay( 500 );
				rsp = clientBaidu.GetAsync( uri, cancelToken ).Result;

				var content = rsp.Content.ReadAsStringAsync().Result;

				var tokenMatch = System.Text.RegularExpressions.Regex.Match( content, "token: '(.*?)'," );
				var gtkMatch = System.Text.RegularExpressions.Regex.Match( content, "window.gtk = '(.*?)';" );
				if( gtkMatch.Success && gtkMatch.Groups.Count > 1 )
					baiduGtk = gtkMatch.Groups[1].Value;
				if( tokenMatch.Success && tokenMatch.Groups.Count > 1 )
					baiduToken = tokenMatch.Groups[1].Value;

				IEnumerable<Cookie> rspCookies = handlerBaidu.CookieContainer.GetCookies( uri ).Cast<Cookie>();
				var rspCookie = rspCookies.FirstOrDefault( x => x.Name == baiduCookieName );
				if( rspCookie != null ) {
					baiduCookie = rspCookie.Value.ToString();

					// BAIDUID=99C3CE30DA0FE8FD336F4428DA0DFDFC:FG=1; BIDUPSID=99C3CE30DA0FE8FD336F4428DA0DFDFC; PSTM=1515055428; REALTIME_TRANS_SWITCH=1; FANYI_WORD_SWITCH=1; HISTORY_SWITCH=1; SOUND_SPD_SWITCH=1; SOUND_PREFER_SWITCH=1; locale=zh; PSINO=7; H_PS_PSSID=1443_25549_21127_22157; Hm_lvt_64ecd82404c51e03dc91cb9e8c025574=1514859052,1515028770,1515029153,1515114213; Hm_lpvt_64ecd82404c51e03dc91cb9e8c025574=1515134327; from_lang_often=%5B%7B%22value%22%3A%22zh%22%2C%22text%22%3A%22%u4E2D%u6587%22%7D%2C%7B%22value%22%3A%22en%22%2C%22text%22%3A%22%u82F1%u8BED%22%7D%5D; to_lang_often=%5B%7B%22value%22%3A%22en%22%2C%22text%22%3A%22%u82F1%u8BED%22%7D%2C%7B%22value%22%3A%22zh%22%2C%22text%22%3A%22%u4E2D%u6587%22%7D%5D
					if( baiduCookie != null ) {
						IEnumerable<string> tmpValues;
						if( clientBaidu.DefaultRequestHeaders.TryGetValues( baiduCookieName, out tmpValues ) )
							clientBaidu.DefaultRequestHeaders.Remove( baiduCookieName );

						clientBaidu.DefaultRequestHeaders.Add( baiduCookieName, baiduCookie );
					}
				}
			}
			catch { }
		}


		#endregion

		#region "Translation APIs with subscription charging"

		// http://api.fanyi.baidu.com/api/trans/product/apidoc
		// last testing: 2019/05/25
		public static IEnumerable<YieldResult>
						TranslateByBaiduCharged( string sourceText,
												SupportedSourceLanguage sourceLanguage,
												SupportedTargetLanguage targetLanguage,
												CancellationToken cancelToken,
												IProgress<ProgressInfo> progress,
												string appId, string secretKey )
		{
			if( string.IsNullOrWhiteSpace( sourceText ) || string.IsNullOrWhiteSpace( appId ) ||
				string.IsNullOrWhiteSpace( secretKey ) )
				yield break;

			// Supported Languages: http://api.fanyi.baidu.com/api/trans/product/apidoc#languageList

			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocBaiduChargedCommon, srcLang, tgtLang = "cht";
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "zh";
					break;
				case SupportedSourceLanguage.ChineseTraditional:
					srcLang = "cht";
					break;
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "jp";
					break;
				default:
					// NOTE: cannot let translator auto detect !!
					yield break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}

			// same language is non-sense...
			if( srcLang == tgtLang )
				yield break;

			if( DescendedModels == null )
				DescendedModels = new ObservableList<MappingMonitor.MappingModel>();

			var text = sourceText;
			var sb = new StringBuilder( sourceText );
			var tmpList = new List<(string OrigText, string TmpText, string XlatedText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				var k1 = string.Format( "θabcdeλ{0}◎", num++ );
				var k2 = $"{{{k1}}}";
				tmpList.Add( (mm.OriginalText, k2, mm.MappingText) );
				sb.Replace( mm.OriginalText, k2 );

				var tmpstr = sb.ToString();
				if( tmpstr.Contains( $"…{k2}" ) || tmpstr.Contains( $"【{k2}】" ) ||
					tmpstr.Contains( $"＋{k2}" ) ) {
					tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				}

				CurrentUsedModels.Add( mm );
			}

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Baidu, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientBaidu.DefaultRequestHeaders.Accept.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientBaidu.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Add( "User-Agent", "curl/7.47.7" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare Query and location values
			var salt = "1435660288";
			MD5 md5Hash = MD5.Create();

			// Sample request: http://api.fanyi.baidu.com/api/trans/vip/translate?q=apple&from=en&to=zh&appid=2015063000000001&salt=1435660288&sign=f89f9594663708c1605f3d736d01d2d4
			var values = new Dictionary<string, string> {
							{ "from", srcLang }, // from Source Language
							{ "to", tgtLang }, // to Target Language
							{ "appid", appId },
							{ "salt", salt },
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				// words to be translated
				values["q"] = section; //Uri.EscapeDataString( section ); // encoding in FormUrlEncodedContent
				values["sign"] = CalculteMd5( md5Hash, appId + section + salt + secretKey );


				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				HttpResponseMessage response = null;
				try {
					response = clientBaidu.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) || response.StatusCode != System.Net.HttpStatusCode.OK )
					continue;

				var translatedData = JsonConvert.DeserializeObject<BaiduTranslationResponseV1>( responseString );

				if( translatedData != null && translatedData.TranslatedResult != null &&
					translatedData.TranslatedResult.Count > 0 ) {
					sb.Clear();

					// prepare translated strings by mapping to original lines
					var origLines = section.Split( sNewLineSeparator, StringSplitOptions.None );
					int origIndex = 0;
					foreach( var data in translatedData.TranslatedResult ) {
						if( string.IsNullOrWhiteSpace( data.Destination ) )
							continue;

						for( int i = origIndex; i < origLines.Length; ++i ) {
							if( string.IsNullOrEmpty( origLines[i] ) ) {
								sb.AppendLine();
								continue;
							}

							// check original string start with whitespace or Fullwidth form space
							foreach( var ch in origLines[i] ) {
								if( char.IsWhiteSpace( ch ) || '　' == ch )
									sb.Append( ch );
								else
									break;
							}

							sb.AppendLine( data.Destination );

							origIndex = i + 1;
							break;
						}
					}


					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.BaiduXlationAfter2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[{]?θabcdeλ(?<SeqNum>[0-9]+)◎[}]?", "{θabcdeλ${SeqNum}◎}" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult>
						TranslateByYoudaoCharged( string sourceText,
													SupportedSourceLanguage sourceLanguage,
													SupportedTargetLanguage targetLanguage,
													CancellationToken cancelToken,
													IProgress<ProgressInfo> progress,
													string appKey, string appSecret )
		{
			if( string.IsNullOrWhiteSpace( sourceText ) || string.IsNullOrWhiteSpace( appKey ) ||
				string.IsNullOrWhiteSpace( appSecret ) )
				yield break;

			// https://ai.youdao.com/doc.s#guide
			// Supported Languages: http://ai.youdao.com/docs/doc-trans-api.s#p07

			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocYoudaoChargedCommon, srcLang, tgtLang = "zh-CHS";
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "zh-CHS";
					break;
				//case SupportedSourceLanguage.ChineseTraditional:
				//	srcLang = "zh-CHT"; // NOT SUPPORTED
				//	break;
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "ja";
					break;
				default:
					// NOTE: cannot let translator auto detect !!
					yield break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}

			// same language is non-sense...
			if( srcLang == tgtLang )
				yield break;

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "(abc0{0})", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Youdao, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientYoudao.DefaultRequestHeaders.Accept.Count <= 0 )
				clientYoudao.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientYoudao.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Add( "User-Agent", "curl/7.47.7" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare Query and location values
			var salt = "1526368137702";
			var sha256Hash = SHA256.Create();

			// http://ai.youdao.com/docs/doc-trans-api.s#p04
			var values = new Dictionary<string, string> {
							{ "from", srcLang }, // from Source Language
							{ "to", tgtLang }, // to Target Language
							{ "appKey", appKey },
							{ "salt", salt },
							{ "signType", "v3" },
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				// words to be translated
				string curtime = Convert.ToString( (long)((DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc )).TotalMilliseconds / 1000) );
				string input;
				if( section.Length > 20 ) {
					input = section.Substring( 0, 10 ) + section.Length.ToString() +
							section.Substring( section.Length - 10 );
					input = appKey + input + salt + curtime + appSecret;
				}
				else {
					input = appKey + section + salt + curtime + appSecret;
				}

				values["q"] = section;
				values["sign"] = CalculteSha256( sha256Hash, input );
				values["curtime"] = curtime;

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				HttpResponseMessage response = null;
				try {
					response = clientYoudao.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) || response.StatusCode != System.Net.HttpStatusCode.OK )
					continue;

				var translatedData = JsonConvert.DeserializeObject<YoudaoTranslationResponseV1>( responseString );

				if( translatedData != null && translatedData.Translation != null &&
					translatedData.Translation.Count > 0 ) {
					sb.Clear();

					// prepare translated strings by mapping to original lines
					var origLines = section.Split( sNewLineSeparator, StringSplitOptions.None );
					int origIndex = 0;
					foreach( var data in translatedData.Translation ) {
						if( string.IsNullOrWhiteSpace( data ) )
							continue;

						var tgtLines = data.Split( sNewLineSeparator, StringSplitOptions.RemoveEmptyEntries );
						var tgtIndex = 0;
						for( int i = origIndex; i < origLines.Length; ++i ) {
							if( string.IsNullOrEmpty( origLines[i] ) ) {
								sb.AppendLine();
								continue;
							}

							// check original string start with whitespace or Fullwidth form space
							foreach( var ch in origLines[i] ) {
								if( char.IsWhiteSpace( ch ) || '　' == ch )
									sb.Append( ch );
								else
									break;
							}

							if( tgtIndex < tgtLines.Length ) {
								if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
									sb.AppendLine( ChineseConverter.Convert( tgtLines[tgtIndex++], ChineseConversionDirection.SimplifiedToTraditional ) );
								}
								else {
									sb.AppendLine( tgtLines[tgtIndex++] );
								}
							}
							else {
								origIndex = i + 1;
								break;
							}
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.YoudaoXlationAfter2Cht );
						afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[(]?abc0(?<SeqNum>[0-9]+)[)]?", "(abc0${SeqNum})" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult>
						TranslateByGoogleCharged( string sourceText,
												SupportedSourceLanguage sourceLanguage,
												SupportedTargetLanguage targetLanguage,
												CancellationToken cancelToken,
												IProgress<ProgressInfo> progress,
												string apiKey, string model = "nmt" )
		{
			if( string.IsNullOrWhiteSpace( sourceText ) || string.IsNullOrWhiteSpace( apiKey ) )
				yield break;

			// API Key console https://console.cloud.google.com/apis/credentials
			// EVAL:  https://developers.google.com/apis-explorer/?hl=zh_TW#p/translate/v2/language.translations.translate?_h=8&resource=%257B%250A++%2522format%2522%253A+%2522text%2522%252C%250A++%2522model%2522%253A+%2522nmt%2522%252C%250A++%2522target%2522%253A+%2522zh-TW%2522%252C%250A++%2522q%2522%253A+%250A++%255B%2522%255C%2522%25E3%2580%2580%25E4%25BB%258A%25E3%2581%25A7%25E3%2581%25AF%25E3%2581%259D%25E3%2581%2593%25E3%2581%258B%25E3%2581%2597%25E3%2581%2593%25E3%2581%25A7%25E7%25A6%2581%25E7%2585%2599%25E9%2581%258B%25E5%258B%2595%25E3%2581%258C%25E5%25A7%258B%25E3%2581%25BE%25E3%2581%25A3%25E3%2581%25A6%25E3%2581%2584%25E3%2582%258B%25E3%2580%2582%25E6%2590%25BA%25E5%25B8%25AF%25E7%2581%25B0%25E7%259A%25BF%25E3%2582%2592%25E6%258C%2581%25E3%2581%25A3%25E3%2581%25A6%25E3%2581%2584%25E3%2582%2588%25E3%2581%2586%25E3%2581%25A8%25E3%2582%2582%25E3%2580%2581%25E7%25A6%2581%25E7%2585%2599%25E3%2582%25A8%25E3%2583%25AA%25E3%2582%25A2%25E3%2581%25A8%25E3%2581%2597%25E3%2581%25A6%25E6%258C%2587%25E5%25AE%259A%25E3%2581%2595%25E3%2582%258C%25E3%2581%25A6%25E3%2581%2584%25E3%2582%258B%25E8%2588%25B9%25E7%259D%2580%25E3%2581%258D%25E5%25A0%25B4%25E3%2581%25A7%25E7%2585%2599%25E8%258D%2589%25E3%2582%2592%25E5%2590%25B8%25E3%2581%2586%25E3%2581%25AA%25E3%2581%25A9%25E3%2583%259E%25E3%2583%258A%25E3%2583%25BC%25E9%2581%2595%25E5%258F%258D%25E3%2581%25A0%25E3%2580%2582%255C%2522%2522%252C%2522%25E3%2580%258C%25E3%2581%258A%25E3%2581%2586%25E3%2582%2588%25E3%2581%2589%25EF%25BD%259E%25E3%2580%2581%25E3%2583%2592%25E3%2583%2598%25E3%2581%25B8%25E3%2581%25B8%25E2%2580%25A6%25E2%2580%25A6%25E6%259C%2580%25E8%25BF%2591%25E3%2581%25AF%25E5%25BF%2599%25E3%2581%2597%25E3%2581%258F%25E3%2581%25A6%25E3%2581%2584%25E3%2581%2584%25E3%2581%25AD%25E3%2581%2587%25EF%25BC%2581%25E3%2580%2580%25E8%2582%25A1%25E9%2596%2593%25E3%2581%258C%25E4%25B9%25BE%25E3%2581%258F%25E6%259A%2587%25E3%2582%2582%25E3%2581%25AD%25E3%2581%2587%25E3%2580%2581%25E3%2582%25B5%25E3%2582%25A4%25E3%2583%2583%25E3%2582%25B3%25E3%2582%25A9%25E2%2580%25A6%25E2%2580%25A6%25E3%2582%25A6%25E3%2581%25AB%25E3%2583%258F%25E3%2582%25A4%25E3%2583%2586%25E3%2583%25B3%25E3%2582%25B7%25E3%2583%25A7%25E3%2583%25B3%25E3%2581%25A0%25E3%2581%2581%25EF%25BC%2581%25EF%25BC%2581%25E3%2580%2580%25E7%2588%25BA%25E5%2585%25B1%25E3%2581%2589%25EF%25BD%259E%25E9%2599%258D%25E3%2582%258A%25E8%2590%25BD%25E3%2581%25A8%25E3%2581%2595%25E3%2582%258C%25E3%2582%2593%25E3%2581%2598%25E3%2582%2583%25E3%2581%25AD%25E3%2581%2587%25E3%2581%259C%25E3%2581%2587%25EF%25BC%259F%25E3%2580%2580%25E3%2581%2594%25E6%259C%259F%25E5%25BE%2585%25E9%2580%259A%25E3%2582%258A%25E3%2581%25AB%25E8%25B6%2585%25E9%2580%259F%25E3%2581%25A7%25E3%2582%25A4%25E3%2582%25AB%25E3%2581%259B%25E3%2581%25A6%25E3%2582%2584%25E3%2582%2593%25E3%2582%2588%25E3%2581%258A%25E3%2581%2589%25EF%25BC%2581%25EF%25BC%2581%25E3%2580%258D%2522%250A++%255D%250A%257D&
			// Supported Languages: https://cloud.google.com/translate/docs/languages
			//	ISO-639-1, ISO-639-2 and BCP-47

			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocGoogleCharged, srcLang, tgtLang = "zh-TW";
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "zh";
					break;
				case SupportedSourceLanguage.ChineseTraditional:
					srcLang = "zh-TW";
					break;
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "ja";
					break;
				default:
					// let translator auto detect
					//break;
					yield break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}

			// same language is non-sense...
			if( srcLang == tgtLang )
				yield break;

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "[{0}]", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Google + 1400, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientGoogle.DefaultRequestHeaders.Accept.Count <= 0 )
				clientGoogle.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientGoogle.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientGoogle.DefaultRequestHeaders.Add( "User-Agent", "curl/7.47.7" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare Query and location values, with HTML text and escape translation class
			// https://translation.googleapis.com/language/translate/v2?key={YOUR_API_KEY}
			sb.Append( defLoc + $"&key={apiKey}" );


			string loc = sb.ToString();
			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				// words to be translated and must be wrapped as JSON array!! 
				var postData = new GoogleCloudTranslationRequestV2 {
					//Format = "text",
					Model = model,
					TargetLanguage = tgtLang,
					Query = new List<string> { section }
				};
				string json = JsonConvert.SerializeObject( postData );
				HttpContent contentPost = new StringContent( json, Encoding.UTF8, "application/json" );

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				HttpResponseMessage response = null;
				try {
					response = clientGoogle.PostAsync( loc, contentPost, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) || response.StatusCode != System.Net.HttpStatusCode.OK )
					continue;

				var translatedData = JsonConvert.DeserializeObject<GoogleCloudTranslationResponseV2>( responseString );

				if( translatedData != null && translatedData.Data != null && translatedData.Data.Translations != null &&
					translatedData.Data.Translations.Count > 0 ) {
					sb.Clear();

					foreach( var data in translatedData.Data.Translations ) {
						if( string.IsNullOrWhiteSpace( data.TranslatedText ) )
							continue;
						sb.Append( data.TranslatedText );
					}

					sb.Replace( " ]", "]" );
					sb.Replace( "[ ", "[" );

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.GoogleXlationAfter2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult>
						TranslateByGoogleChargedV3( string sourceText,
													SupportedSourceLanguage sourceLanguage,
													SupportedTargetLanguage targetLanguage,
													CancellationToken cancelToken,
													IProgress<ProgressInfo> progress,
													string authorizationBearerToken )
		{
			// https://cloud.google.com/translate/docs/migrate-to-v3
			// https://translation.googleapis.com/v3beta1/projects/my-project/locations/us-central1/supportedLanguages


			yield break;
		}

		public static IEnumerable<YieldResult>
						TranslateByMicrosoftCharged( string sourceText,
														SupportedSourceLanguage sourceLanguage,
														SupportedTargetLanguage targetLanguage,
														CancellationToken cancelToken,
														IProgress<ProgressInfo> progress,
														string subscriptionKey, string subscriptionRegion,
														string preferRegionHost = "api.cognitive.microsofttranslator.com" )
		{
			if( string.IsNullOrWhiteSpace( sourceText ) || string.IsNullOrWhiteSpace( subscriptionKey ) )
				yield break;

			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocBingChargedV3, srcLang, tgtLang = "zh-Hant";
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.ChineseSimplified:
					srcLang = "zh-Hans";
					break;
				case SupportedSourceLanguage.ChineseTraditional:
					srcLang = "zh-Hant";
					break;
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "ja";
					break;
				default:
					// let translator auto detect
					//break;
					yield break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}

			// same language is non-sense...
			if( srcLang == tgtLang )
				yield break;

			if( string.IsNullOrWhiteSpace( preferRegionHost ) == false ) {
				defLoc = $"https://{preferRegionHost}/translate?api-version=3.0";
			}

			var text = sourceText;
			var sb = new StringBuilder();
			//var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "WWW.ZKS{0}.ORG", null, ref sb );
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "<p class=\"notranslate\">ZKS{0}</p>", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			//if( false == SliceSections( text, Translator.MaxWords.Bing, out sections ) ||
			if( false == SliceSectionsToHtml( text, 4800, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientBing.DefaultRequestHeaders.Accept.Count <= 0 )
				clientBing.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientBing.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBing.DefaultRequestHeaders.Add( "User-Agent", "curl/7.47.7" );

			Report( progress, 1, "Preparing Translation", null );

			// prepare Query and location values, with HTML text and escape translation class
			sb.Append( defLoc + $"&profanityAction=noAction&includeAlignment=true&includeSentenceLength=true&textType=html&to={tgtLang}" );

			string loc = sb.ToString();
			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				// words to be translated and must be wrapped as JSON array!! 
				var postData = new[] { new MicrosoftTranslatorFormatV3.TextData { Text = section } };
				string json = JsonConvert.SerializeObject( postData );
				HttpContent contentPost = new StringContent( json, Encoding.UTF8, "application/json" );
				contentPost.Headers.Add( "Ocp-Apim-Subscription-Key", subscriptionKey );
				if( string.IsNullOrWhiteSpace( subscriptionRegion ) == false ) {
					// refer: https://docs.microsoft.com/en-us/azure/cognitive-services/authentication
					contentPost.Headers.Add( "Ocp-Apim-Subscription-Region", subscriptionRegion );
				}

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				HttpResponseMessage response = null;
				try {
					response = clientBing.PostAsync( loc, contentPost, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) || response.StatusCode != System.Net.HttpStatusCode.OK )
					continue;

				var translatedData = JsonConvert.DeserializeObject<List<MicrosoftTranslatorFormatV3>>( responseString );

				if( translatedData != null && translatedData.Count > 0 && translatedData[0].Translations != null &&
					translatedData[0].Translations.Count > 0 ) {
					sb.Clear();

					// prepare translated strings by mapping to original lines
					var origLines = section.Split( sNewLineSeparatorHtml, StringSplitOptions.None );
					int origIndex = 0;
					foreach( var data in translatedData[0].Translations ) {
						if( data.To != tgtLang || string.IsNullOrWhiteSpace( data.Text ) )
							continue;

						var tgtLines = data.Text.Split( sNewLineSeparatorHtml, StringSplitOptions.RemoveEmptyEntries );
						int tgtIndex = 0;
						for( int i = origIndex; i < origLines.Length; ++i ) {
							if( string.IsNullOrEmpty( origLines[i] ) ) {
								sb.AppendLine();
								continue;
							}

							// check original string start with whitespace or Fullwidth form space
							foreach( var ch in origLines[i] ) {
								if( char.IsWhiteSpace( ch ) || '　' == ch )
									sb.Append( ch );
								else
									break;
							}

							if( tgtIndex < tgtLines.Length ) {
								sb.AppendLine( tgtLines[tgtIndex++] );
							}
							else {
								origIndex = i + 1;
								break;
							}
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.BingXlationAfter2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}


		private static string CalculteMd5( MD5 md5Hash, string src )
		{
			byte[] data = md5Hash.ComputeHash( Encoding.UTF8.GetBytes( src ) );

			// Convert the input string to a byte array and compute the hash.
			StringBuilder sb = new StringBuilder();

			// Loop through each byte of the hashed data 
			// and format each one as a lower hexadecimal string.
			for( int i = 0; i < data.Length; i++ ) {
				sb.Append( data[i].ToString( "x2" ) );
			}

			// Return the hexadecimal string.
			return sb.ToString();
		}

		private static string CalculteSha256( SHA256 sha256Hash, string src )
		{
			byte[] data = sha256Hash.ComputeHash( Encoding.UTF8.GetBytes( src ) );

			// Return the hexadecimal string.
			return BitConverter.ToString( data ).Replace( "-", "" );
		}


		#endregion // Translation APIs with charging


		#region "Web Page Style Translator without AJAX"

		// https://agirlamonggeeks.com/2018/11/25/c-8-features-part-2-async-method-with-yield-return/
		public static IEnumerable<YieldResult> TranslateByExcite( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress )
		{
			// prepare default location
			string defLoc = sLocExciteCht, srcLang2TgtLang = "JACH"; // from Japanese to Chinese
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					defLoc = sLocExciteEn;
					srcLang2TgtLang = "JAEN";
					break;
			}

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", 5.61359, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Excite, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			Report( progress, 1, "Preparing Translation", null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "wb_lp", srcLang2TgtLang}, // from Source Language to Target Language
						};

			if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
				values["big5"] = "yes"; // translated to Chinese Traditional

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["before"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				HttpResponseMessage response = null;
				try {
					response = client.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}
				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				mHtmlDoc.LoadHtml( responseString );
				var afterTextArea = mHtmlDoc.GetElementbyId( "after" );
				if( afterTextArea != null && string.IsNullOrWhiteSpace( afterTextArea.InnerText ) == false ) {
					sb.Clear();
					// filter out usless html code
					sb.Append( afterTextArea.InnerText.Replace( "&#010;", Environment.NewLine ) );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.From, tuple2.To );

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.XlatedText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.XlatedText );
					}


					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByWeblio( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress )
		{
			// prepare default location
			string defLoc = sLocWeblioCht, srcLang2TgtLang = "JC"; // from Japanese to Chinese...CJ

			// Only support Japanese <-> English, Japanese <-> ChineseSimp. and Japanese <-> Korea
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.Japanese:
					// default is Japanese -> ChineseSimp.
					if( targetLanguage == SupportedTargetLanguage.English ) {
						defLoc = sLocWeblioEn;
						srcLang2TgtLang = "JE"; // EJ
					}
					break;

				//case SupportedSourceLanguage.English:
				//if( TargetLanguage == SupportedTargetLanguage.Japanese)
				//srcLang2TgtLang = "EJ";
				//break;
				//case SupportedSourceLanguage.ChineseSimplified:
				//case SupportedSourceLanguage.ChineseTraditional:
				//if( TargetLanguage == SupportedTargetLanguage.Japanese )
				//srcLang2TgtLang = "CJ";
				//break;

				default:
					yield break;
			}

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", 5.61359, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Weblio, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			Report( progress, 1, "Preparing Translation", null );

			// prepare form values
			// https://translate.weblio.jp/chinese/?lp=JC&originalText=%E3%81%8A%E3%81%A3%E3%81%95%E3%82%93
			var values = new Dictionary<string, string> {
							{ "lp", srcLang2TgtLang}, // from Source Language to Target Language
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["originalText"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Thread.Sleep( rnd.Next( 500, 1000 ) );
				}

				sb.Clear();
				HttpResponseMessage response = null;
				try {
					response = client.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, "Got Exception: " + ex.Message, ex );
					yield break;
				}
				string responseString = response.Content.ReadAsStringAsync().Result;

				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				mHtmlDoc.LoadHtml( responseString );
				var inputXlatedText = mHtmlDoc.GetElementbyId( "translatedText" );
				var translatedText = inputXlatedText.GetAttributeValue( "value", string.Empty );
				if( translatedText != null && string.IsNullOrWhiteSpace( translatedText ) == false ) {
					// filter out usless html code
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
						sb.AppendLine( ChineseConverter.Convert( translatedText,
										ChineseConversionDirection.SimplifiedToTraditional ) );
					else
						sb.AppendLine( translatedText );

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.WeblioXlationAfter2Cht );
						afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, $"{xlatedSectionCnt}/{sections.Count} Translated", section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};

				}

				if( sb.Length <= 0 ) {
					Report( progress, -1, $"Something wrong when translating, StatusCode: {response.StatusCode}", section );
					yield break;
				}
			}
		}

		#endregion

		public static bool SliceSections( string text, int maxWords, out List<string> sections )
		{
			if( text == null ) {
				sections = null;
				return false;
			}

			sections = new List<string>();

			int nlcnt = Environment.NewLine.Length;
			StringBuilder sb = new StringBuilder();
			using( System.IO.StringReader reader = new System.IO.StringReader( text ) ) {
				try {
					string line = null;
					while( (line = reader.ReadLine()) != null ) {

						if( line.Length + nlcnt > maxWords ) {
							// a single line is large than maxWords
							if( sb.Length > 0 ) {
								sections.Add( sb.ToString() );
								sb.Clear();
							}

							int idx = 0;
							while( idx < line.Length ) {
								int leftLen = line.Length - idx + 1;
								if( leftLen < maxWords ) {
									sb.AppendLine( line.Substring( idx ) );
									break;
								}

								sections.Add( line.Substring( idx, maxWords ) );
								idx += maxWords;
							}
						}
						else if( sb.Length + line.Length + nlcnt > maxWords ||
								  sb.Length + nlcnt > maxWords ) {
							sections.Add( sb.ToString() );
							sb.Clear();
							sb.AppendLine( line );
						}
						else {
							sb.AppendLine( line );
						}

						line = null;
					}

					// add lines in sb
					if( sb.Length > 0 )
						sections.Add( sb.ToString() );

					// add last line
					if( line != null && string.IsNullOrWhiteSpace( line ) == false )
						sections.Add( line );

				}
				catch( OutOfMemoryException ) {
					return false;
				}
			}

			return true;
		}

		public static bool SliceSectionsToHtml( string text, int maxWords, out List<string> sections )
		{
			if( text == null ) {
				sections = null;
				return false;
			}

			sections = new List<string>();

			string htmlNewLine = "<br>";
			int nlcnt = htmlNewLine.Length;
			StringBuilder sb = new StringBuilder();
			using( System.IO.StringReader reader = new System.IO.StringReader( text ) ) {
				try {
					string line = null;
					while( (line = reader.ReadLine()) != null ) {

						if( line.Length + nlcnt > maxWords ) {
							// a single line is large than maxWords
							if( sb.Length > 0 ) {
								sections.Add( sb.ToString() );
								sb.Clear();
							}

							int idx = 0;
							while( idx < line.Length ) {
								int leftLen = line.Length - idx + 1;
								if( leftLen < maxWords ) {
									//sb.AppendLine( line.Substring( idx ) );
									sb.Append( line.Substring( idx ) + htmlNewLine );
									break;
								}

								sections.Add( line.Substring( idx, maxWords ) );
								idx += maxWords;
							}
						}
						else if( sb.Length + line.Length + nlcnt > maxWords ||
								  sb.Length + nlcnt > maxWords ) {
							sections.Add( sb.ToString() );
							sb.Clear();
							sb.Append( line + htmlNewLine );
						}
						else {
							sb.Append( line + htmlNewLine );
						}

						line = null;
					}

					// add lines in sb
					if( sb.Length > 0 )
						sections.Add( sb.ToString() );

					// add last line
					if( line != null && string.IsNullOrWhiteSpace( line ) == false )
						sections.Add( line );

				}
				catch( OutOfMemoryException ) {
					return false;
				}
			}

			return true;
		}

		private static readonly string[] sNewLineSeparator = new string[] { "\r\n", "\r", "\n" };
		private static readonly string[] sNewLineSeparatorHtml = new[] { "<br>" };

		private static readonly Random rnd = new Random();

		private static readonly string sLocExciteCht = "https://www.excite.co.jp/world/fantizi/";
		private static readonly string sLocExciteEn = "https://www.excite.co.jp/world/english/";
		private static readonly string sLocWeblioCht = "https://translate.weblio.jp/chinese/";
		private static readonly string sLocWeblioEn = "https://translate.weblio.jp/";
		private static readonly string sLocCrossLang = "http://cross.transer.com/";
		private static readonly HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();

		private static readonly HttpClient client = new HttpClient(), clientXLang = new HttpClient(), clientBaidu = new HttpClient( handlerBaidu ), clientYoudao = new HttpClient();
		private static readonly HttpClient clientGoogle = new HttpClient(), clientBing = new HttpClient();

		private static readonly string sLocCrossLangFree = "http://cross.transer.com/text/exec_tran";
		private static readonly string sLocBaiduFree = "https://fanyi.baidu.com/v2transapi";
		private static readonly string sLocBaiduFreeDetectLang = "https://fanyi.baidu.com/langdetect"; // [Form Data] query: xxxxx
		private static readonly string sLocYoudaoFree = "https://fanyi.youdao.com/translate";
		private static readonly string sLocGoogleFree = "https://translate.google.com/translate_a/single";

		// useless... often encounter   Message: AppId is over the quota
		//private static readonly string sLocBingFree = "https://api.microsofttranslator.com/v2/Http.svc/Translate?appId=AFC76A66CF4F434ED080D245C30CF1E71C22959C&from=&to=zh-TW&text=レシピ";


		// https://api.fanyi.baidu.com/api/trans/product/index   AppId, Secret   // appid + salt + sign
		private static readonly string sLocBaiduChargedCommon = "https://fanyi-api.baidu.com/api/trans/vip/translate";
		private static readonly string sLocBaiduChargedField = "https://fanyi-api.baidu.com/api/trans/vip/fieldtranslate";
		private static readonly string sLocBaiduChargedCustom = "https://fanyi-api.baidu.com/api/trans/private/translate";
		private static readonly string sLocBaiduChargedDetectLang = "https://fanyi-api.baidu.com/api/trans/vip/language";

		private static readonly string sLocYoudaoChargedCommon = "https://openapi.youdao.com/api";

		// https://cloud.google.com/translate/
		//    https://cloud.google.com/translate/docs/reference/rest/v2/translate
		private static readonly string sLocGoogleCharged = "https://translation.googleapis.com/language/translate/v2?";
		private static readonly string sLocGoogleChargedDetectLang = "https://translation.googleapis.com/language/translate/v2/detect/?q=Change&key=XXXXXX";

		// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v2-0-reference
		// Microsoft Translator Hub will be retired on May 17, 2019. And V2 APIs have also been deprecated on March 30, 2018.
		//private static readonly string sLocBingCharged = "https://api.microsofttranslator.com/V2/Http.svc/Translate";
		// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-reference
		private static readonly string sLocBingChargedV3 = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";


		private static readonly string sLocBaidu = "https://fanyi.baidu.com/#jp/cht/まさか";
		private static readonly string jsBaiduToken = @"
function a(r, o) {
    for (var t = 0; t < o.length - 2; t += 3) {
        var a = o.charAt(t + 2);
        a = a >= 'a' ? a.charCodeAt(0) - 87 : Number(a),
        a = '+' === o.charAt(t + 1) ? r >>> a : r << a,
        r = '+' === o.charAt(t) ? r + a & 4294967295 : r ^ a
	}
    return r
}
var C = null;
var token = function( r, _gtk ) {
    var o = r.length;
	o > 30 && (r = '' + r.substr(0, 10) + r.substr(Math.floor(o / 2) - 5, 10) + r.substring(r.length, r.length - 10));
    var t = void 0,
    t = null !== C ? C: (C = _gtk || '') || '';
    for (var e = t.split('.'), h = Number( e[0]) || 0, i = Number( e[1]) || 0, d = [], f = 0, g = 0; g<r.length; g++) {
        var m = r.charCodeAt( g );
        128 > m ? d[f++] = m : (2048 > m ? d[f++] = m >> 6 | 192 : (55296 === (64512 & m) && g + 1 < r.length && 56320 === (64512 & r.charCodeAt(g + 1)) ? (m = 65536 + ((1023 & m) << 10) + (1023 & r.charCodeAt(++g)), d[f++] = m >> 18 | 240, d[f++] = m >> 12 & 63 | 128) : d[f++] = m >> 12 | 224, d[f++] = m >> 6 & 63 | 128), d[f++] = 63 & m | 128)
    }
    for (var S = h, u = '+-a^+6', l = '+-3^+b+-f', s = 0; s<d.length; s++)
		S += d[s], S = a( S, u);
    return S = a( S, l),
		S ^= i,
		0 > S && (S = (2147483647 & S) + 2147483648),
		S %= 1e6,
		S.toString() + '.' + (S ^ h)
}
";

		private static void Report( IProgress<ProgressInfo> progress,
						int percentOrCode, string message, object infoObject )
		{
			progress?.Report( new ProgressInfo {
				PercentOrErrorCode = percentOrCode,
				Message = message,
				InfoObject = infoObject
			} );

		}

		private static List<(string OrigText, string TmpText, string XlatedText)>
			ReplaceBeforeXlation( string text, SupportedSourceLanguage srcLang,
									string patternWithNum1, double? extraNum,
									ref StringBuilder replacedSb )
		{
			if( replacedSb == null )
				replacedSb = new StringBuilder();
			replacedSb.Clear();
			replacedSb.Append( text );

			// replace some text to avoid collision
			if( srcLang == SupportedSourceLanguage.Japanese ) {
				foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
					replacedSb.Replace( From, To );
				}
			}

			CurrentUsedModels.Clear();
			if( DescendedModels == null )
				DescendedModels = new ObservableList<MappingMonitor.MappingModel>();

			// Tulpe3 => <Original text, tmp. text, Translated text>
			var tmpList = new List<(string OrigText, string TmpText, string XlatedText)>();
			int num = rnd.Next( 10, 30 );
			foreach( var mm in DescendedModels ) {
				if( text.Contains( mm.OriginalText ) == false )
					continue;

				string k1 = null;
				if( extraNum == null ) {
					k1 = string.Format( patternWithNum1, num++ );
					while( text.Contains( k1 ) )
						k1 = string.Format( patternWithNum1, num++ );
				}
				else {
					k1 = string.Format( patternWithNum1, extraNum + num++ );
					while( text.Contains( k1 ) )
						k1 = string.Format( patternWithNum1, extraNum + num++ );
				}
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				replacedSb.Replace( mm.OriginalText, k1 );

				CurrentUsedModels.Add( mm );
			}

			return tmpList;
		}

		private static bool ReplaceAfterXlation( StringBuilder sb,
								List<(string OrigText, string TmpText, string XlatedText)> tmpList,
								List<IReadOnlyList<(string From, string To)>> afterTgtLangs,
								string regexPattern, string regexReplacement )
		{
			if( sb == null || sb.Length <= 0 || tmpList == null )
				return false;

			// filter out usless html code
			sb.Replace( "&#010;", Environment.NewLine );
			foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
				sb.Replace( tuple2.From, tuple2.To );

			// replace some CHS char. from Youdao/Weblio... translated string
			if( afterTgtLangs != null ) {
				foreach( var list in afterTgtLangs ) {
					foreach( var (From, To) in list ) {
						sb.Replace( From, To );
					}
				}
			}

			// Regex replace mis-translated temp. string
			if( regexPattern != null && regexReplacement != null ) {
				var tstr = System.Text.RegularExpressions.Regex.Replace( sb.ToString(), regexPattern, regexReplacement );
				if( string.IsNullOrWhiteSpace( tstr ) == false ) {
					sb.Clear();
					sb.Append( tstr );
				}
			}

			// final, replace back mapping translated text
			if( MarkMappedTextWithHtmlBoldTag ) {
				foreach( var tuple3 in tmpList )
					sb.Replace( tuple3.TmpText, $"<b>{tuple3.XlatedText}</b>" );
			}
			else {
				foreach( var tuple3 in tmpList )
					sb.Replace( tuple3.TmpText, tuple3.XlatedText );
			}
			return true;
		}

	}
}
