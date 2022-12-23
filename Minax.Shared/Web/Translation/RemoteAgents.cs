using HtmlAgilityPack;
using Jurassic;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using Minax.Collections;
using Minax.Domain.Translation;
using Minax.Web.Formats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
				clientXLang.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentCurl1 );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientXLang.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message), ex );
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
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						PercentOrErrorCode = percent,
						OriginalSection = section,
						TranslatedSection = sb.ToString()
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode), response );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByMiraiTranslateFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			const string zhtLang = "zt";
			string defLoc = sLocMiraiXlateFree, srcLang = "ja", tgtLang = zhtLang;
			bool isSrcCjk = false;
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
				case SupportedSourceLanguage.Japanese:
					srcLang = "ja";
					isSrcCjk = true;
					break;
				//case SupportedSourceLanguage.ChineseTraditional:
				//case SupportedSourceLanguage.ChineseSimplified:
				//	isSrcCjk = true;
				default:
					yield break;
			}


			var text = sourceText;
			CurrentUsedModels.Clear();
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.MiraiTranslate, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			if( miraiTran == null ) {
				RefreshMiraiData( cancelToken );
			}

			// prepare form values
			var values = new Dictionary<string, object> {
							{ "source", srcLang }, // from Source Language
							{ "target", tgtLang }, // to Target Lanaguage
							{ "filter_profile", "nmt" },
							{ "profile", "inmt" },
							{ "InmtTarget", "" },
							{ "InmtTranslateType", "gisting" },
							{ "usePrefix", false },
							{ "tran", miraiTran },
							//{ "tran", "EGQ6FXaPXpNg23e4SiJj95JkcKftUuiMAMx1pZqbcZPva3gB3wyShtplMYgcf0pf" },

							// Glossary, need setup first and mapping lang. by other procedure!!
							// "adaptPhrases":[{"source":"バジリスク","target":"Basilisk"}]
						};

			if( zhtLang == srcLang || zhtLang == tgtLang )
				values[zhtLang] = true;

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["input"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				var json = JsonConvert.SerializeObject( values );
				var content = new StringContent( json, Encoding.UTF8, "application/json" );
				HttpResponseMessage response = null;
				try {
					response = clientMiraiXlate.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<MiraiTranslateFormat1>( responseString );

				if( translatedData != null && translatedData.Status == MiraiTranslateFormat1.StatusSuccess &&
					translatedData.Outputs != null && translatedData.Outputs.Count > 0 ) {
					sb.Clear();

					foreach( var output in translatedData.Outputs ) {
						foreach( var result in output.Results ) {
							if( isSrcCjk == false ) {
								// directly append composited sentences
								sb.Append( result.Translation );
							}
							else {
								foreach( var sen in result.Sentences ) {
									// put origin whitespace char.
									foreach( char ch in sen.Original ) {
										if( char.IsWhiteSpace( ch ) || '　' == ch )
											sb.Append( ch );
										else
											break;
									}
									int xlationLen = sen.Translation.Length;
									for( int i = 0; i < xlationLen; ++i ) {
										if( char.IsWhiteSpace( sen.Translation[i] ) == false ) {
											sb.Append( sen.Translation.Substring(i, xlationLen - i) );
											if( sen.OriginalDelimiter != "" )
												sb.Append( sen.OriginalDelimiter );
											break;
										}
									}
								}
							}
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
						afterTgtLangs.Add( Profiles.MiraiXlationAfter2Cht );

					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						PercentOrErrorCode = percent,
						OriginalSection = section,
						TranslatedSection = sb.ToString()
					};
				}
				else {
					// maybe the tran expired
					miraiTran = null;
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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

				var k1 = string.Format( "http://tmp58.org/a{0}b", num );
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				num++;

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

			//clientBaidu.Timeout = TimeSpan.FromSeconds( 30 ); // set connecting try-timeout to 30 seconds
			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
				values["sign"] = sign;

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 1500, 2500 ), cancelToken ).Wait( cancelToken );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				Exception netEx = null;
				int r2 = 5;
				for( ; r2 > 0; r2-- ) {
					try {
						response = clientBaidu.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
					}
					catch( AggregateException ae ) {
						if( ae.InnerException is System.Net.Http.HttpRequestException hre ) {
							if( hre.InnerException is WebException we ) {
								netEx = we;
								Task.Delay( 200, cancelToken ).Wait( cancelToken );
							}
							else {
								netEx = hre;
								Task.Delay( 200, cancelToken ).Wait( cancelToken );
							}
						}
						else {
							netEx = ae;
							Task.Delay( 200, cancelToken ).Wait( cancelToken );
							RefreshBaiduData( cancelToken );
						}
					}
					catch( Exception ex ) {
						netEx = ex;
						Task.Delay( 200, cancelToken ).Wait( cancelToken );
					}

					if( cancelToken.IsCancellationRequested ) {
						r2 = 0;
						break;
					}
					if( response != null && response.IsSuccessStatusCode )
						break;
				}

				if( r2 <= 0 && netEx != null ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, netEx.Message ), netEx );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) || responseString.StartsWith("<") )
					continue;

				BaiduTranslatorFormat2 translatedData = null;
				try {
					translatedData = JsonConvert.DeserializeObject<BaiduTranslatorFormat2>( responseString );
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

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
					//remove extra line
					sb.Remove( sb.Length - Environment.NewLine.Length, Environment.NewLine.Length );

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
						afterTgtLangs.Add( Profiles.BaiduXlationAfter2Cht );

					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
									@"[\s]?http://tmp58.org/a(?<SeqNum>[0-9]+)b[\s]?", "http://tmp58.org/a${SeqNum}b"))
						continue;


					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

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

					if( --retry >= 0 )
						goto reloaded;

					if( translatedData != null && string.IsNullOrWhiteSpace( translatedData.Message ) == false ) {
						Report( progress, -1, string.Format(Languages.WebXlator.Str2TranslateErrorMessage, translatedData.Error, translatedData.Message ),
								translatedData );
					}
					else {
						Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					}
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByIcibaFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = $"{sLocIcibaFree}?c=trans&m=fy&client=6&auth_user=key_web_fanyi", srcLang = "auto", tgtLang = "cht";
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

				var k1 = string.Format( "abc0{0}", num );
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				num++;

				CurrentUsedModels.Add( mm );
			}

			if( cancelToken.IsCancellationRequested )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Iciba, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "from", srcLang }, // from Source Language
							{ "to", tgtLang }, // to Target Language
						};

			MD5 md5Hash = MD5.Create();
			Regex rx = new Regex( @"(^\s*)|(\s*$)", RegexOptions.ECMAScript ); // RegexOptions.MatchCRLFNewLine
			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["q"] = rx.Replace( section, "" ); // words to be translated, remove leading and tail spaces

				// https://greasyfork.org/zh-TW/scripts/378277-%E7%BF%BB%E8%AF%91%E6%9C%BA/code
				//var sign = CalculteMd5( md5Hash, "6key_web_fanyi" + "ifanyiweb8hc9s98e" + rx.Replace(section, "") ).Substring(0, 16);
				var sign = CalculteMd5( md5Hash, "6key_web_fanyi" + "ifanyiweb8hc9s98e" + values["q"] ).Substring( 0, 16 );

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 5500, 9500 ), cancelToken ).Wait( cancelToken );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = client.PostAsync( $"{defLoc}&sign={sign}", content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null || response.IsSuccessStatusCode == false )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<IcibaTranslatorFormat1>( responseString );

				if( translatedData != null && translatedData.Status == 1 &&
					translatedData.ContentData != null ) {
					sb.Clear();

					sb.Append( translatedData.ContentData.Out ); // contains \n

					if( false == ReplaceAfterXlation( sb, tmpList, null,
										@"abc0(?<SeqNum>[0-9]+)", "abc0${SeqNum}" ) )
						continue;


					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}


		private static string lingoCloudBrowserId = string.Empty, lingoCloudJwt = string.Empty;
		public static IEnumerable<YieldResult> TranslateByLingoCloudFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocLingoCloudFree, srcLang2TgtLang = "auto2zh";
			switch( sourceLanguage ) {
				//case SupportedSourceLanguage.ChineseSimplified:
				//switch( targetLanguage ) {
				//	//case SupportedTargetLanguage.English:
				//	//	srcLang2TgtLang = "zh2en";
				//	//case SupportedTargetLanguage.Korea:
				//	//	srcLang2TgtLang = "zh2ko";
				//	//case SupportedTargetLanguage.Japanese:
				//	//	srcLang2TgtLang = "zh2ja";
				//	case SupportedTargetLanguage.ChineseTraditional:
				//	default:
				//		yield break;
				//}
				//break;
				//case SupportedSourceLanguage.English:
				//	switch( targetLanguage ) {
				//		//case SupportedTargetLanguage.Japanese:
				//		//	srcLang2TgtLang = "en2ja";
				//		case SupportedTargetLanguage.ChineseSimplified:
				//			srcLang2TgtLang = "en2zh";
				//			break;
				//		case SupportedTargetLanguage.ChineseTraditional:
				//		default:
				//			yield break;
				//	}
				//	break;
				case SupportedSourceLanguage.Japanese:
					switch( targetLanguage ) {
						//case SupportedTargetLanguage.English:
						//	srcLang2TgtLang = "ja2en";
						//	break;
						//case SupportedTargetLanguage.ChineseSimplified:
						case SupportedTargetLanguage.ChineseTraditional:
							srcLang2TgtLang = "ja2zh"; // only support CHS
							break;
						default:
							yield break;
					}
					break;
				default:
					yield break;
			}


			// prepare browser_id
			if( lingoCloudBrowserId == string.Empty ) {
				lingoCloudBrowserId = CalculteMd5( MD5.Create(), rnd.Next().ToString() );
			}

			var text = sourceText;
			CurrentUsedModels.Clear();
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "(abc73{0})", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.LingoCloud, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientLingoCloud.DefaultRequestHeaders.Accept.Count <= 0 ) {
				clientLingoCloud.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				clientLingoCloud.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );
				clientLingoCloud.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			}
			if( clientLingoCloud.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientLingoCloud.DefaultRequestHeaders.Add( sUserAgentCookieName, "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0" );

			if( clientLingoCloud.DefaultRequestHeaders.TryGetValues( "X-Authorization", out _ ) == false )
				clientLingoCloud.DefaultRequestHeaders.Add( "X-Authorization", "token:qgemv4jr1y38jyq6vhvi" );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			// prepare JWT
			if( lingoCloudJwt == string.Empty ) {
				LingoCloudJwtRequest1 data1 = new LingoCloudJwtRequest1 { BrowserId = lingoCloudBrowserId };
				string jwtReq = JsonConvert.SerializeObject( data1 );
				var rsp = clientLingoCloud.PostAsync( sLocLingoCloudGen, new StringContent( jwtReq, Encoding.UTF8, "application/json" ), cancelToken ).Result;
				if( rsp.IsSuccessStatusCode == false ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, rsp.StatusCode ), rsp );
					yield break;
				}

				var jwtRsp = JsonConvert.DeserializeObject<LingoCloudJwtResponse1>( rsp.Content.ReadAsStringAsync().Result );
				lingoCloudJwt = jwtRsp.Jwt;
			}

			if( clientLingoCloud.DefaultRequestHeaders.TryGetValues( "T-Authorization", out _ ) )
				clientLingoCloud.DefaultRequestHeaders.Remove( "T-Authorization" );

			clientLingoCloud.DefaultRequestHeaders.Add( "T-Authorization", lingoCloudJwt );

			// prepare form values};
			var reqData = new LingoCloudXlateRequest1() {
				TransType = srcLang2TgtLang,
				Detect = true,
				BrowserId = lingoCloudBrowserId,
			};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				// words to be translated
				reqData.Source = section.Split( sNewLineSeparator, StringSplitOptions.None ).ToList();

				if( reqData.Source.Count < 1 )
					continue;

				bool dummy = false;
				if( reqData.Source.Count == 1 ) {
					// add dummy empty line to avoid translatedData.Target become a string
					reqData.Source.Add( "" );
					dummy = true;
				}

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				var content = new StringContent( JsonConvert.SerializeObject( reqData ), Encoding.UTF8, "application/json" );
				HttpResponseMessage response = null;
				try {
					response = clientLingoCloud.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<LingoCloudXlateResponse1>( responseString );

				if( translatedData != null && translatedData.Target != null  ) {
					sb.Clear();

					// decode xlated data to string
					var origLines = reqData.Source;
					if( origLines.Count != translatedData.Target.Count ) {
						foreach( var target in translatedData.Target ) {
							if( string.IsNullOrWhiteSpace( target ) ) {
								sb.AppendLine( target );
								continue;
							}

							var result = DecodeLingoCloudXlatedData( target );
							if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
								sb.AppendLine( ChineseConverter.Convert( result, ChineseConversionDirection.SimplifiedToTraditional ) );
							}
							else {
								sb.AppendLine( result );
							}
						}
					} else {
						// ignore dummy empty string
						int cnt = dummy ? translatedData.Target.Count - 1 : translatedData.Target.Count;
						for( int i = 0; i < cnt; ++i ) {
							var target = translatedData.Target[i];
							if( string.IsNullOrWhiteSpace( target ) ) {
								sb.AppendLine( target );
							}
							else {
								// check original string start with whitespace or Fullwidth form space
								foreach( var ch in origLines[i] ) {
									if( char.IsWhiteSpace( ch ) || '　' == ch )
										sb.Append( ch );
									else
										break;
								}

								var result = DecodeLingoCloudXlatedData( target );
								if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
									sb.AppendLine( ChineseConverter.Convert( result, ChineseConversionDirection.SimplifiedToTraditional ) );
								}
								else {
									sb.AppendLine( result );
								}
							}
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.YoudaoXlationAfter2Cht );
						afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[(]?abc73(?<SeqNum>[0-9]+)[)]?", "(abc73${SeqNum})" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						PercentOrErrorCode = percent,
						OriginalSection = section,
						TranslatedSection = sb.ToString()
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
			string defLoc = sLocYoudaoFree, srcLang = "ja", tgtLang = "zh-CHT"; // "zh-CHS" from Japanese to ChineseSimplified
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "(abc5{0})", null, ref sb );
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

			if( clientYoudao.DefaultRequestHeaders.Accept.Count <= 0 ) {
				clientYoudao.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				clientYoudao.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );
				clientYoudao.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			}
			if( clientYoudao.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientYoudao.DefaultRequestHeaders.Add( sUserAgentCookieName, "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0" );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "from", srcLang}, // from Source Language
							{ "to", tgtLang}, // to Target Language, useless, most are xlate to ChineseSimplified
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["q"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 1500, 3000 ) , cancelToken ).Wait(cancelToken) ;
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientYoudao.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null || response.IsSuccessStatusCode == false )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<YoudaoTranslatorFormat3>( responseString );

				if( translatedData != null && translatedData.ErrorCode == "0" &&
					translatedData.Translation != null ) {
					sb.Clear();

					var origLines = translatedData.Query.Split( sNewLineSeparator, StringSplitOptions.None );
					int origIndex = 0;

					foreach( var ts in translatedData.Translation ) {
						if( origIndex >= origLines.Length ) {
							sb.Append( ts );
							continue;
						}

						// insert leading whitespace char.
						var xlateLines = ts.Split( sNewLineSeparator, StringSplitOptions.None );
						foreach( var xline in xlateLines ) {
							if( origIndex >= origLines.Length ) {
								sb.AppendLine( xline );
							} else {
								var orig = origLines[origIndex++];
								foreach( var ch in orig ) {
									if( char.IsWhiteSpace( ch ) || '　'.Equals( ch ) )
										sb.Append( ch );
									else
										break;
								}
								sb.AppendLine( xline.TrimStart() );
							}
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.YoudaoXlationAfter2Cht );
						//afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[(]?abc5(?<SeqNum>[0-9]+)[)]?", "(abc5${SeqNum})" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByTencentFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocTencentFree, srcLang = "ja", tgtLang = "zh"; // from Japanese to ChineseSimplified
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					//tgtLang = "en"; // Not Support!!
					//break;
					progress?.Report( new ProgressInfo {
						PercentOrErrorCode = -1,
						Message = string.Format( Languages.WebXlator.Str2YoudaoNotSupportSrc2Tgt, sourceLanguage.ToL10nString(), targetLanguage.ToL10nString() )
					} );
					yield break;
			}

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "abc789{0}", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Tencent, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientTencent.DefaultRequestHeaders.Accept.Count <= 0 ) {
				clientTencent.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				clientTencent.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );
				clientTencent.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			}
			if( clientTencent.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientTencent.DefaultRequestHeaders.Add( sUserAgentCookieName, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" );

			if( clientTencent.DefaultRequestHeaders.TryGetValues( "Origin", out _ ) == false )
				clientTencent.DefaultRequestHeaders.Add( "Origin", "https://fanyi.qq.com" );

			if( clientTencent.DefaultRequestHeaders.TryGetValues( "X-Requested-With", out _ ) == false )
				clientTencent.DefaultRequestHeaders.Add( "X-Requested-With", "XMLHttpRequest" );

			if( clientTencent.DefaultRequestHeaders.Referrer == null )
				clientTencent.DefaultRequestHeaders.Referrer = new Uri( "https://fanyi.qq.com/" );

			clientTencent.DefaultRequestHeaders.Host = "fanyi.qq.com";

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			if( tencentQtk == null || tencentQtv == null ) {
				RefreshTencentData( cancelToken );
			}

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "qtv", tencentQtv },
							{ "qtk", tencentQtk },
							{ "source", "auto" },
							//{ "source", srcLang}, // from Source Language
							{ "target", tgtLang}, // to Target Language, useless, most are xlate to ChineseSimplified
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["sourceText"] = section; // words to be translated

				int retry = 3;
	retryStart:
				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 75000, 95000 ), cancelToken ).Wait( cancelToken );
				}

				RefreshTencentData( cancelToken );
				values["qtv"] = tencentQtv;
				values["qtk"] = tencentQtk;
				values["sessionUuid"] = $"translate_uuid{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientTencent.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null )
					continue;

				if( response.IsSuccessStatusCode == false && retry-- > 0 ) {
					if( tencentQtk == null || tencentQtv == null ) {
						RefreshTencentData( cancelToken );
						values["qtv"] = tencentQtv;
						values["qtk"] = tencentQtk;
					}
					goto retryStart;
				}
				
				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<TencentTranslatorFormat1>( responseString );

				if( translatedData != null && translatedData.ErrorCode == 0 && translatedData.TranslateData != null &&
						translatedData.TranslateData.Records != null && translatedData.TranslateData.Records.Length > 0 ) {
					sb.Clear();

					foreach( var rec in translatedData.TranslateData.Records ) {
						if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
							sb.Append( ChineseConverter.Convert( rec.TargetText, ChineseConversionDirection.SimplifiedToTraditional ) );
						else
							sb.Append( rec.TargetText );
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.YoudaoXlationAfter2Cht );
						afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"[aA]bc789(?<SeqNum>[0-9]+)", "abc789${SeqNum}" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}

		private static string papagoAuthKey = string.Empty;
		public static IEnumerable<YieldResult> TranslateByPapagoFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocPapagoFree, srcLang = "ja", tgtLang = "zh-TW"; // from Japanese to ChineseTraditional

			switch( sourceLanguage ) {
				case SupportedSourceLanguage.English:
					srcLang = "en";
					break;
			}
			switch( targetLanguage ) {
				case SupportedTargetLanguage.English:
					tgtLang = "en";
					break;
			}


			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "abc0{0}", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Papago, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientPapago.DefaultRequestHeaders.Accept.Count <= 0 ) {
				clientPapago.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				clientPapago.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );
			}
			if( clientPapago.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientPapago.DefaultRequestHeaders.Add( sUserAgentCookieName, "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0" );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			// get AUTH_KEY
			if( papagoAuthKey == string.Empty ) {
				var rsp = clientPapago.GetAsync( "https://papago.naver.com/" ).Result;
				
				if( rsp.IsSuccessStatusCode == false ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, rsp.StatusCode ), rsp );
					yield break;
				}
				Report( progress, 2, Languages.WebXlator.Str0PreparingTranslation, null );

				string rspString = rsp.Content.ReadAsStringAsync().Result;
				var match = Regex.Match( rspString, @"home.*.chunk.js", RegexOptions.IgnoreCase );
				if( match.Success ) {
					var chunkFileName = match.Groups[0].ToString();
					rsp = clientPapago.GetAsync( $"https://papago.naver.com/{chunkFileName}" ).Result;
					
					if( rsp.IsSuccessStatusCode == false ) {
						Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, rsp.StatusCode ), rsp );
						yield break;
					}
					Report( progress, 3, Languages.WebXlator.Str0PreparingTranslation, null );

					rspString = rsp.Content.ReadAsStringAsync().Result;
					match = Regex.Match( rspString, @"AUTH_KEY:""(.*?)""", RegexOptions.IgnoreCase );
					if( match.Success == false ) {
						Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, rsp.StatusCode ), rsp );
						yield break;
					}

					papagoAuthKey = match.Groups[1].ToString();
				} else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, rsp.StatusCode ), rsp );
					yield break;
				}

				clientPapago.DefaultRequestHeaders.Add( "x-apigw-partnerid", "papago" );
				clientPapago.DefaultRequestHeaders.Add( "device-type", "pc" );
			}

			var time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
			if( clientPapago.DefaultRequestHeaders.TryGetValues( "Timestamp", out _ ) )
				clientPapago.DefaultRequestHeaders.Remove( "Timestamp" );

			clientPapago.DefaultRequestHeaders.Add( "Timestamp", time );

			// from https://greasyfork.org/zh-TW/scripts/378277-%E7%BF%BB%E8%AF%91%E6%9C%BA/code
			// "authorization":'PPG '+time+':'+CryptoJS.HmacMD5(time+'\nhttps://papago.naver.com/apis/n2mt/translate\n'+time, sessionStorage.getItem('papago_key')).toString(CryptoJS.enc.Base64),
			if( clientPapago.DefaultRequestHeaders.TryGetValues( "Authorization", out _ ) )
				clientPapago.DefaultRequestHeaders.Remove( "Authorization" );

			var hmac = new HMACMD5( Encoding.UTF8.GetBytes( papagoAuthKey) );
			var hashBytes = hmac.ComputeHash( Encoding.UTF8.GetBytes( $"{time}\nhttps://papago.naver.com/apis/n2mt/translate\n{time}" ) );
			var auth = $"PPG {time}:{Convert.ToBase64String( (hashBytes) )}";
			clientPapago.DefaultRequestHeaders.Add( "Authorization", auth );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "deviceId", time },
							{ "source", srcLang}, // from Source Language
							{ "target", tgtLang}, // to Target Language, useless, most are xlate to ChineseSimplified
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["text"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 200, 1000 ), cancelToken ).Wait( cancelToken );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientPapago.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null )
					continue;

				response.EnsureSuccessStatusCode();
				
				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				var translatedData = JsonConvert.DeserializeObject<PapagoFormat1>( responseString );

				if( translatedData != null && string.IsNullOrEmpty(translatedData.ErrorCode) &&
						translatedData.TranslatedText != null ) {
					sb.Clear();
					sb.Append( translatedData.TranslatedText );

					if( false == ReplaceAfterXlation( sb, tmpList, null,
										@"abc0(?<SeqNum>[0-9]+)", "abc0${SeqNum}" ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					// maybe AUTH_KEY updated, so remove old value
					papagoAuthKey = string.Empty;

					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
				clientGoogle.DefaultRequestHeaders.Add( sUserAgentCookieName,
						 "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0" );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientGoogle.PostAsync( defLoc, content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
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
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByMicrosoftFree( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocBingFree, srcLang, tgtLang = "zh-Hant";
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

				var k1 = string.Format( "abc31{0}", num );
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

				num++;

				CurrentUsedModels.Add( mm );
			}

			if( cancelToken.IsCancellationRequested )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Iciba, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			if( bingToken == null ) {
				RefreshBingData( cancelToken );
				Report( progress, 3, Languages.WebXlator.Str0PreparingTranslation, null );
				Task.Delay( rnd.Next( 500, 1500 ), cancelToken ).Wait( cancelToken );
			}

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "fromLang", srcLang }, // from Source Language
							{ "to", tgtLang }, // to Target Language
							{ "key", bingKey },
							{ "token", bingToken },
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["text"] = section; // words to be translated, remove leading and tail spaces

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 3500, 6500 ), cancelToken ).Wait( cancelToken );
				}

				var content = new FormUrlEncodedContent( values );
				HttpResponseMessage response = null;
				try {
					response = clientBing.PostAsync( $"{defLoc}&IG={bingIg}&IID={bingIid}", content, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null || response.IsSuccessStatusCode == false )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				List<MicrosoftBingResponse3> translatedData = null;
				try {
					translatedData = JsonConvert.DeserializeObject<List<MicrosoftBingResponse3>>( responseString );
				} catch {
					bingToken = null;
					var error = JsonConvert.DeserializeObject<MicrosoftBingResponse3Error>( responseString );
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, error.StatusCode ), error );
					yield break;
				}

				if( translatedData != null && translatedData.Count > 0 ) {
					sb.Clear();

					foreach( var data in translatedData ) {
						if( data.Translations == null || data.Translations.Count <= 0 )
							continue;

						foreach( var trans in data.Translations ) {
							sb.Append( trans.Text );
						}
					}

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.MicrosoftXlationAfter2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs,
										@"abc31(?<SeqNum>[0-9]+)", "abc31${SeqNum}" ) )
						continue;


					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					bingToken = null;
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}


		private static Jurassic.ScriptEngine jsEngine = new ScriptEngine();
		private static HttpClientHandler handlerBaidu = new HttpClientHandler() { UseCookies = true };
		private static string baiduToken = null, baiduGtk = "320305.131321201", baiduCookie;
		private const string baiduCookieName = "BAIDUID";
		private static void RefreshBaiduData( CancellationToken cancelToken )
		{

			if( clientBaidu.DefaultRequestHeaders.Accept.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientBaidu.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBaidu.DefaultRequestHeaders.Add( sUserAgentCookieName, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" );

			//if( clientBaidu.DefaultRequestHeaders.TryGetValues( "Referer", out _ ) == false )
			//	clientBaidu.DefaultRequestHeaders.Add( "Referer", "https://fanyi.baidu.com" );


			try {
				var uri = new Uri( sLocBaiduXlator );

				var rsp = clientBaidu.GetAsync( uri, cancelToken ).Result;
				var content = rsp.Content.ReadAsStringAsync().Result;

				var tokenMatch = Regex.Match( content, @"token\s*:\s*['""](.*?)['""]" );
				// for Desktop(window.gtk = "320305.131321201";) and Mobile(gtk: '320305.131321201')
				var gtkMatch = Regex.Match( content, @"(?:window\.)?gtk\s*[\:\=]?\s*['""]?(.*?)['""]+" );
				if( gtkMatch.Success && gtkMatch.Groups.Count > 1 )
					baiduGtk = gtkMatch.Groups[1].ToString();
				if( tokenMatch.Success && tokenMatch.Groups.Count > 1 )
					baiduToken = tokenMatch.Groups[1].ToString();

				IEnumerable<Cookie> rspCookies = handlerBaidu.CookieContainer.GetCookies( uri ).Cast<Cookie>();
				var rspCookie = rspCookies.FirstOrDefault( x => x.Name == baiduCookieName );
				if( rspCookie != null ) {
					baiduCookie = rspCookie.Value.ToString();

					// BAIDUID=99C3CE30DA0FE8FD336F4428DA0DFDFC:FG=1; BIDUPSID=99C3CE30DA0FE8FD336F4428DA0DFDFC; PSTM=1515055428; REALTIME_TRANS_SWITCH=1; FANYI_WORD_SWITCH=1; HISTORY_SWITCH=1; SOUND_SPD_SWITCH=1; SOUND_PREFER_SWITCH=1; locale=zh; PSINO=7; H_PS_PSSID=1443_25549_21127_22157; Hm_lvt_64ecd82404c51e03dc91cb9e8c025574=1514859052,1515028770,1515029153,1515114213; Hm_lpvt_64ecd82404c51e03dc91cb9e8c025574=1515134327; from_lang_often=%5B%7B%22value%22%3A%22zh%22%2C%22text%22%3A%22%u4E2D%u6587%22%7D%2C%7B%22value%22%3A%22en%22%2C%22text%22%3A%22%u82F1%u8BED%22%7D%5D; to_lang_often=%5B%7B%22value%22%3A%22en%22%2C%22text%22%3A%22%u82F1%u8BED%22%7D%2C%7B%22value%22%3A%22zh%22%2C%22text%22%3A%22%u4E2D%u6587%22%7D%5D
					if( baiduCookie != null ) {
						if( clientBaidu.DefaultRequestHeaders.TryGetValues( baiduCookieName, out _ ) )
							clientBaidu.DefaultRequestHeaders.Remove( baiduCookieName );

						clientBaidu.DefaultRequestHeaders.Add( baiduCookieName, baiduCookie );
					}
				}
			}
			catch { }
		}


		private static Dictionary<char, char> lingoCloudMapData = null;
		private static string DecodeLingoCloudXlatedData( string target ) {

			// from https://greasyfork.org/zh-TW/scripts/378277-%E7%BF%BB%E8%AF%91%E6%9C%BA/code
			// const source="NOPQRSTUVWXYZABCDEFGHIJKLMnopqrstuvwxyzabcdefghijklm";
			// const dic=[..."ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"].reduce((dic,current,index)=>{dic[current]=source[index];return dic},{});
			// const decoder = line => Base64.decode([...line].map(i=>dic[i]||i).join(""))
			// return await BaseTranslate('彩雲小譯',raw,options,res=>JSON.parse(res).target.map(decoder).join('\n'))
			// origin from https://fanyi.caiyunapp.com/assets/index.8784f25a.js
			if( lingoCloudMapData == null ) {
				lingoCloudMapData = new Dictionary<char, char>();
				const string keys = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
				const string source = "NOPQRSTUVWXYZABCDEFGHIJKLMnopqrstuvwxyzabcdefghijklm";
				for( int i = 0; i < keys.Length; ++i ) {
					lingoCloudMapData.Add( keys[i], source[i] );
				}
			}

			var x2 = from ch in target select lingoCloudMapData.ContainsKey( ch ) ? lingoCloudMapData[ch] : ch;
			return Encoding.UTF8.GetString( Convert.FromBase64String( new string( x2.ToArray() ) ) );
		}

		private static string bingKey, bingToken = null, bingIg, bingIid;
		private static void RefreshBingData( CancellationToken cancelToken ) {
			if( clientBing.DefaultRequestHeaders.Accept.Count <= 0 )
				clientBing.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientBing.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBing.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgent1 );

			try {
				//var uri = new Uri( "https://www.bing.com/translator" );
				var rsp = clientBing.GetAsync( "https://www.bing.com/translator", cancelToken ).Result;
				if( rsp.IsSuccessStatusCode != true )
					return;

				string html = rsp.Content.ReadAsStringAsync().Result;
				var matchIid = Regex.Match( html, @"<div id\s*=\s*""rich_tta"".*data-iid\s*=\s*""(.*?)""", RegexOptions.IgnoreCase );
				if( matchIid.Success && matchIid.Groups.Count > 1 )
					bingIid = matchIid.Groups[1].ToString() + ".1";

				var matchIg = Regex.Match( html, @"""ig""\:""([a-zA-Z0-9]+)", RegexOptions.IgnoreCase );
				if( matchIg.Success && matchIg.Groups.Count > 1 )
					bingIg = matchIg.Groups[1].ToString();

				var matchParams = Regex.Match( html, @"var params_RichTranslateHelper\s*=\s*\[([0-9]+)\s*,\s*""([a-zA-Z0-9_]*)""", RegexOptions.IgnoreCase );
				if( matchParams.Success ) {
					if( matchParams.Groups.Count > 1 ) {
						bingKey = matchParams.Groups[1].ToString();
					}
					if( matchParams.Groups.Count > 2 ) {
						bingToken = matchParams.Groups[2].ToString();
					}
				}
			} catch { }
		}

		private static HttpClientHandler handlerMirai = new HttpClientHandler() {
			AllowAutoRedirect = true, UseCookies = true,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate, };
		private static string miraiCookie = null, miraiTran = null;
		private static void RefreshMiraiData( CancellationToken cancelToken ) {

			//if( clientMiraiXlate.BaseAddress == null )
			//	clientMiraiXlate.BaseAddress = new Uri( "https://trial.miraitranslate.com" );

			if( clientMiraiXlate.DefaultRequestHeaders.Accept.Count <= 0 ) {
				clientMiraiXlate.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			}
			if( clientMiraiXlate.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientMiraiXlate.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgent1 );

			try {
				var uri = new Uri( sLoclMiraiXlator );

				var rsp = clientMiraiXlate.GetAsync( uri, cancelToken ).Result;
				if( rsp.IsSuccessStatusCode != true )
					return;

				if( miraiTran == null ) {
					// extract 'tran' variable and it would be changed with each session
					string html = rsp.Content.ReadAsStringAsync().Result;
					var match = Regex.Match( html, @"var tran\s*=\s*""(.*?)""", RegexOptions.IgnoreCase );
					if( match.Success && match.Groups.Count > 1 ) {
						miraiTran = match.Groups[1].ToString();
					}
				}

				IEnumerable<Cookie> rspCookies = handlerMirai.CookieContainer.GetCookies( uri ).Cast<Cookie>();
				miraiCookie = string.Join( "; ", from co in rspCookies select $"{co.Name}={co.Value}" );

				rsp.Headers.TryGetValues( "set-cookie", out var cookies );
				if( string.IsNullOrWhiteSpace( miraiCookie ) && cookies != null ) {
					foreach( var cookie in cookies ) {
						var match = Regex.Match( cookie, @"translate_session=[a-zA-Z0-9]+" );
						if( match.Success && match.Groups.Count > 0 ) {
							miraiCookie = match.Groups[0].ToString();
							break;
						}
					}
				}

				if( clientMiraiXlate.DefaultRequestHeaders.TryGetValues( "Cache-Control", out _ ) == false )
					clientMiraiXlate.DefaultRequestHeaders.Add( "Cache-Control", "no-cache" );
				if( clientMiraiXlate.DefaultRequestHeaders.TryGetValues( "Connection", out _ ) == false )
					clientMiraiXlate.DefaultRequestHeaders.Add( "Connection", "keep-alive" );
				if( clientMiraiXlate.DefaultRequestHeaders.TryGetValues( "Accept-Encoding", out _ ) == false )
					clientMiraiXlate.DefaultRequestHeaders.Add( "Accept-Encoding", "gzip, deflate" );
			}
			catch { }
		}

		private static readonly HttpClientHandler handlerTencent = new HttpClientHandler() {
			AllowAutoRedirect = true, UseCookies = true,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
		};
		private static string tencentQtv = null, tencentQtk = null;
		private static void RefreshTencentData( CancellationToken cancelToken ) {

			if( clientTencent.DefaultRequestHeaders.Accept.Count <= 0 ) {
				clientTencent.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				clientTencent.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "text/html" ) );
				clientTencent.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			}
			if( clientTencent.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientTencent.DefaultRequestHeaders.Add( sUserAgentCookieName, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" );

			if( clientTencent.DefaultRequestHeaders.TryGetValues( "Accept-Encoding", out _ ) == false )
				clientTencent.DefaultRequestHeaders.Add( "Accept-Encoding", "gzip, deflate" );

			try {
				var rsp = clientTencent.GetAsync( new Uri( "https://fanyi.qq.com" ), cancelToken ).Result;
				if( rsp.IsSuccessStatusCode == false )
					return;

				var html = rsp.Content.ReadAsStringAsync().Result;
				var match = Regex.Match( html, @"var reauthuri\s*=\s*""(.*?)""", RegexOptions.IgnoreCase );
				if( match.Success == false )
					return;

				var reauthuri = match.Groups[1].ToString();
				rsp = clientTencent.PostAsync( "https://fanyi.qq.com/api/" + reauthuri, null, cancelToken ).Result;
				if( rsp.IsSuccessStatusCode == false )
					return;

				var jsonStr = rsp.Content.ReadAsStringAsync().Result;
				var tokens = JsonConvert.DeserializeObject<TencentTranslatorTokens1>( jsonStr );
				tencentQtv = tokens.Qtv;
				tencentQtk= tokens.Qtk;
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

				var k1 = string.Format( "http://tmp58.org/a{0}b", num );
				tmpList.Add( (mm.OriginalText, k1, mm.MappingText) );
				sb.Replace( mm.OriginalText, k1 );

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
				clientBaidu.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentCurl1 );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				HttpResponseMessage response = null;
				try {
					response = clientBaidu.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
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
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
				clientYoudao.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentCurl1 );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				HttpResponseMessage response = null;
				try {
					response = clientYoudao.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
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
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
				clientGoogle.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentCurl1 );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				HttpResponseMessage response = null;
				try {
					response = clientGoogle.PostAsync( loc, contentPost, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
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
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "<p class=\"notranslate\">ZKS{0}</p>", null, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSectionsToHtml( text, 4800, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			if( clientBing.DefaultRequestHeaders.Accept.Count <= 0 )
				clientBing.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "*/*" ) );
			if( clientBing.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientBing.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentCurl1 );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				HttpResponseMessage response = null;
				try {
					response = clientBing.PostAsync( loc, contentPost, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
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
						afterTgtLangs.Add( Profiles.MicrosoftXlationAfter2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
			//var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", 5.61359, ref sb );
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", 7.1, ref sb );
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

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			// prepare form values
			var values = new Dictionary<string, string> {
							{ "wb_lp", srcLang2TgtLang}, // from Source Language to Target Language
						};

			if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
				values["big5_lang"] = "yes"; // translated to Chinese Traditional

			if (clientMobile.DefaultRequestHeaders.UserAgent.Count <= 0)
				clientMobile.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentMobile1 );

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["before"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				HttpResponseMessage response = null;
				try {
					response = clientMobile.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}
				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				mHtmlDoc.LoadHtml( responseString );
				var afterTextBox = mHtmlDoc.GetElementbyId( "afterText" );
				HtmlAgilityPack.HtmlNode afterText = null;
				if( afterTextBox != null && afterTextBox.ChildNodes.Count >= 2 ) {
					// get div#afterText.afterTextBox -> p.inputText node
					//afterText = afterTextBox.ChildNodes[afterTextBox.ChildNodes.Count - 2];
					afterText = afterTextBox.Descendants("p").FirstOrDefault(p => p.GetAttributeValue("class", "") == "inputText");
				}

				if( afterText != null ) {
					sb.Clear();
					// filter out usless html code
					sb.Append( afterText.InnerText.Replace( "&#010;", Environment.NewLine ) );
					foreach( var tuple2 in Profiles.HtmlCodeRecoveryText )
						sb.Replace( tuple2.From, tuple2.To );

					if( MarkMappedTextWithHtmlBoldTag ) {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, $"<b>{tuple3.MappingText}</b>" );
					}
					else {
						foreach( var tuple3 in tmpList )
							sb.Replace( tuple3.TmpText, tuple3.MappingText );
					}


					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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
				//if( TargetLanguage == SupportedTargetLanguage.Japanese) // only support English2Japanese
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

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

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
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				sb.Clear();
				HttpResponseMessage response = null;
				try {
					response = client.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;

					if( response.IsSuccessStatusCode != true ) {
						Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
						yield break;
					}
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				string responseString = response.Content.ReadAsStringAsync().Result;

				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				mHtmlDoc.LoadHtml( responseString );
				var inputXlatedText = mHtmlDoc.GetElementbyId( "translatedText" );
				if( inputXlatedText == null ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
				var translatedText = inputXlatedText.GetAttributeValue( "value", string.Empty );
				if( translatedText != null && string.IsNullOrWhiteSpace( translatedText ) == false ) {
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
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};

				}

				if( sb.Length <= 0 ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByGoogleMobile( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location, source language, target language parameters for HttpClient
			string defLoc = sLocGoogleMobileFree, srcLang, tgtLang = "zh-TW";
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

			// slice to some sections. each section maxlength="2048"
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.Google + 500, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			//if( clientGoogle.DefaultRequestHeaders.Accept.Count <= 0 )
			//	clientGoogle.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
			if( clientMobile.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientMobile.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentMobile1 );

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			// prepare form values
			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				HttpResponseMessage response = null;
				try {
					var loc = $"{defLoc}?tl={tgtLang}&q={Uri.EscapeDataString(section)}";
					response = clientMobile.GetAsync( loc, cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response == null || response.IsSuccessStatusCode == false )
					continue;

				string responseString = response.Content.ReadAsStringAsync().Result;
				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				mHtmlDoc.LoadHtml( responseString );
				var nodes = mHtmlDoc.DocumentNode.SelectNodes( "//div[@class='result-container']" );

				if( nodes != null && nodes.Count > 0 ) {
					sb.Clear();
					foreach( HtmlNode node in nodes ) {
						sb.Append( node.InnerText );
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
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};
				}
				else {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}
			}
		}

		public static IEnumerable<YieldResult> TranslateByYoudaoMobile( string sourceText,
															SupportedSourceLanguage sourceLanguage,
															SupportedTargetLanguage targetLanguage,
															CancellationToken cancelToken,
															IProgress<ProgressInfo> progress ) {
			// prepare default location
			string defLoc = sLocYoudaoMobileFree, srcLang2TgtLang = "JA2ZH_CN"; // from Japanese to Chinese...CJ

			// Only support Japanese <-> ChineseSimp., English <-> ChineseSimp.
			switch( sourceLanguage ) {
				case SupportedSourceLanguage.Japanese:
					// default is Japanese -> ChineseSimp.
					//if( targetLanguage == SupportedTargetLanguage.English ) {
					//	srcLang2TgtLang = "JA2EN";
					//}
					break;

				//case SupportedSourceLanguage.English:
				//	if( targetLanguage == SupportedTargetLanguage.ChineseSimplified ) {
				//		srcLang2TgtLang = "EN2ZH_CN";
				//	} else {
				//		yield break;
				//	}
				//	break;

				default:
					yield break;
			}

			var text = sourceText;
			var sb = new StringBuilder();
			var tmpList = ReplaceBeforeXlation( text, sourceLanguage, "{0}", 7.1, ref sb );
			if( tmpList == null || sb.Length <= 0 )
				yield break;

			text = sb.ToString();
			sb.Clear();

			// slice to some sections
			List<string> sections = null;
			if( false == SliceSections( text, Profiles.MaxWords.YoudaoMobile, out sections ) ||
				sections == null || sections.Count <= 0 )
				yield break;

			if( cancelToken.IsCancellationRequested )
				yield break;

			Report( progress, 1, Languages.WebXlator.Str0PreparingTranslation, null );

			if( clientMobile.DefaultRequestHeaders.UserAgent.Count <= 0 )
				clientMobile.DefaultRequestHeaders.Add( sUserAgentCookieName, sUserAgentMobile1 );


			// prepare form values
			var values = new Dictionary<string, string> {
							{ "type", srcLang2TgtLang }, // from Source Language to Target Language
						};

			int xlatedSectionCnt = 0;
			foreach( var section in sections ) {
				values["inputtext"] = section; // words to be translated

				if( cancelToken.IsCancellationRequested )
					yield break;

				if( xlatedSectionCnt > 0 ) {
					Task.Delay( rnd.Next( 500, 1000 ), cancelToken ).Wait( cancelToken );
				}

				sb.Clear();
				HttpResponseMessage response = null;
				try {
					response = clientMobile.PostAsync( defLoc, new FormUrlEncodedContent( values ), cancelToken ).Result;
				}
				catch( Exception ex ) {
					Report( progress, -1, string.Format( Languages.Global.Str1GotException, ex.Message ), ex );
					yield break;
				}

				if( response.IsSuccessStatusCode != true ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}

				var stream = response.Content.ReadAsStreamAsync().Result;
				var ms = new MemoryStream();
				stream.CopyTo( ms );
				string responseString = Encoding.UTF8.GetString( ms.GetBuffer() );
				//string responseString = Encoding.UTF8.GetString( ms.ToArray() );
				//string responseString = response.Content.ReadAsStringAsync().Result;  // exception why???

				if( string.IsNullOrWhiteSpace( responseString ) )
					continue;

				mHtmlDoc.LoadHtml( responseString );
				var translateResult = mHtmlDoc.GetElementbyId( "translateResult" );
				if( translateResult == null ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
					yield break;
				}

				var translatedText = translateResult.InnerText;
				if( translatedText != null && string.IsNullOrWhiteSpace( translatedText ) == false ) {
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional )
						sb.AppendLine( ChineseConverter.Convert( translatedText,
										ChineseConversionDirection.SimplifiedToTraditional ) );
					else
						sb.AppendLine( translatedText );

					List<IReadOnlyList<(string From, string To)>> afterTgtLangs = new List<IReadOnlyList<(string From, string To)>>();
					if( targetLanguage == SupportedTargetLanguage.ChineseTraditional ) {
						afterTgtLangs.Add( Profiles.YoudaoXlationAfter2Cht );
						afterTgtLangs.Add( Profiles.XlationAfterMsConvert2Cht );
					}
					if( false == ReplaceAfterXlation( sb, tmpList, afterTgtLangs, null, null ) )
						continue;

					int percent = (int)(++xlatedSectionCnt * 100 / sections.Count);
					Report( progress, percent, string.Format( Languages.WebXlator.Str2FractionsTranslated, xlatedSectionCnt, sections.Count ), section );

					yield return new YieldResult {
						OriginalSection = section,
						TranslatedSection = sb.ToString(),
						PercentOrErrorCode = percent,
					};

				}

				if( sb.Length <= 0 ) {
					Report( progress, -1, string.Format( Languages.WebXlator.Str1TranslateError, response.StatusCode ), response );
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

			int NLLEN = Environment.NewLine.Length;
			StringBuilder sb = new StringBuilder();
			using( System.IO.StringReader reader = new System.IO.StringReader( text ) ) {
				try {
					string line = null;
					while( (line = reader.ReadLine()) != null ) {

						if( line.Length + NLLEN > maxWords ) {
							// a single line is larger than maxWords
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
						else if( sb.Length + line.Length + NLLEN > maxWords ||
								  sb.Length + NLLEN > maxWords ) {
							if( sb.Length >= NLLEN )
								sb.Remove( sb.Length - NLLEN, NLLEN ); // remove tail NewLine
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

		private static readonly string sUserAgentCookieName = "User-Agent";
		private static readonly string sUserAgent1 = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:56.0) Gecko/20100101 Firefox/56.0";
		private static readonly string sUserAgentMobile1 = "Mozilla/5.0 (Linux; Android 12) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.71 Mobile Safari/537.36";
		private static readonly string sUserAgentCurl1 = "curl/7.47.7";

		private static readonly Random rnd = new Random();

		//private static readonly string sLocExciteCht = "https://www.excite.co.jp/world/fantizi/";
		private static readonly string sLocExciteCht = "https://www.excite.co.jp/world/chinese/";
		private static readonly string sLocExciteEn = "https://www.excite.co.jp/world/english/";
		private static readonly string sLocWeblioCht = "https://translate.weblio.jp/chinese/";
		private static readonly string sLocWeblioEn = "https://translate.weblio.jp/";
		private static readonly string sLocCrossLang = "http://cross.transer.com/";
		private static readonly HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();

		private static readonly HttpClient client = new HttpClient(), clientXLang = new HttpClient();
		public static readonly HttpClient clientYoudao = new HttpClient(), clientTencent = new HttpClient( handlerTencent );
		private static readonly HttpClient clientLingoCloud = new HttpClient(), clientPapago = new HttpClient();
		private static readonly HttpClient clientGoogle = new HttpClient(), clientBing = new HttpClient();
		private static readonly HttpClient clientMobile = new HttpClient();
		private static readonly HttpClient clientBaidu = new HttpClient( handlerBaidu ) { Timeout = TimeSpan.FromSeconds( 30 ) },
									clientMiraiXlate = new HttpClient( handlerMirai ) { Timeout = TimeSpan.FromSeconds( 30 ) };

		private static readonly string sLocCrossLangFree = "http://cross.transer.com/text/exec_tran";
		private static readonly string sLocMiraiXlateFree = "https://trial.miraitranslate.com/trial/api/translate.php";
		private static readonly string sLoclMiraiXlator = "https://miraitranslate.com/trial/";
		private static readonly string sLocBaiduFree = "https://fanyi.baidu.com/v2transapi";
		private static readonly string sLocBaiduMobileFree = "https://fanyi.baidu.com/basetrans";
		private static readonly string sLocBaiduFreeDetectLang = "https://fanyi.baidu.com/langdetect"; // [Form Data] query: xxxxx
		private static readonly string sLocIcibaFree = "https://ifanyi.iciba.com/index.php";
		private static readonly string sLocLingoCloudFree = "https://api.interpreter.caiyunai.com/v1/translator";
		private static readonly string sLocLingoCloudGen = "https://api.interpreter.caiyunai.com/v1/user/jwt/generate";
		//private static readonly string sLocYoudaoFree = "https://fanyi.youdao.com/translate";
		private static readonly string sLocYoudaoFree = "https://aidemo.youdao.com/trans";
		private static readonly string sLocYoudaoMobileFree = "https://m.youdao.com/translate";
		private static readonly string sLocTencentFree = "https://fanyi.qq.com/api/translate";
		private static readonly string sLocPapagoFree = "https://papago.naver.com/apis/n2mt/translate";
		private static readonly string sLocGoogleFree = "https://translate.google.com/translate_a/single";
		private static readonly string sLocGoogleMobileFree = "https://translate.google.com/m";

		// useless... often encounter   Message: AppId is over the quota
		//private static readonly string sLocBingFree = "https://api.microsofttranslator.com/v2/Http.svc/Translate?appId=AFC76A66CF4F434ED080D245C30CF1E71C22959C&from=&to=zh-TW&text=レシピ";
		private static readonly string sLocBingFree = "https://www.bing.com/ttranslatev3?isVertical=1"; // https://www.bing.com/ttranslatev3?isVertical=1&IG={{bing_ig}}&IID={{bing_iid}}


		// https://api.fanyi.baidu.com/api/trans/product/index   AppId, Secret   // appid + salt + sign
		private static readonly string sLocBaiduChargedCommon = "https://fanyi-api.baidu.com/api/trans/vip/translate";
		private static readonly string sLocBaiduChargedField = "https://fanyi-api.baidu.com/api/trans/vip/fieldtranslate";
		private static readonly string sLocBaiduChargedCustom = "https://fanyi-api.baidu.com/api/trans/private/translate";
		private static readonly string sLocBaiduChargedDetectLang = "https://fanyi-api.baidu.com/api/trans/vip/language";

		private static readonly string sLocYoudaoChargedCommon = "https://openapi.youdao.com/api";
		private static readonly string sLocYoudaoChargedV2 = "https://openapi.youdao.com/v2/api"; // batch procssing

		// https://cloud.google.com/translate/
		//    https://cloud.google.com/translate/docs/reference/rest/v2/translate
		private static readonly string sLocGoogleCharged = "https://translation.googleapis.com/language/translate/v2?";
		private static readonly string sLocGoogleChargedDetectLang = "https://translation.googleapis.com/language/translate/v2/detect/?q=Change&key=XXXXXX";

		// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v2-0-reference
		// Microsoft Translator Hub will be retired on May 17, 2019. And V2 APIs have also been deprecated on March 30, 2018.
		//private static readonly string sLocBingCharged = "https://api.microsofttranslator.com/V2/Http.svc/Translate";
		// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-reference
		private static readonly string sLocBingChargedV3 = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";


		private static readonly string sLocBaiduXlator = "https://fanyi.baidu.com/#jp/cht/まさか";
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

		private static List<(string OrigText, string TmpText, string MappingText)>
			ReplaceBeforeXlation( string text, SupportedSourceLanguage srcLang,
									string patternWithNum1, double? extraNum,
									ref StringBuilder replacedSb )
		{
			if( replacedSb == null )
				replacedSb = new StringBuilder();
			replacedSb.Clear();
			replacedSb.Append( text );

			// replace some text to avoid collision
			//if( srcLang == SupportedSourceLanguage.Japanese ) {
			//	foreach( var (From, To) in Profiles.JapaneseEscapeHtmlText ) {
			//		replacedSb.Replace( From, To );
			//	}
			//}

			CurrentUsedModels.Clear();
			if( DescendedModels == null )
				DescendedModels = new ObservableList<MappingMonitor.MappingModel>();

			// Tulpe3 => <Original text, tmp. text, Mapping text>
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
